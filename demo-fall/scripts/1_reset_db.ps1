<#
.SYNOPSIS
    ClickHouse Data Wipe (schema preserved, engine-aware).
    Connection details are set as defaults in the param block.
    
    Usage: 
    .\reset_db.ps1
    .\reset_db.ps1 -Database "test_net" -Force
#>

[CmdletBinding()]
param(
    # ===== Connection Defaults (Hard-coded here) =====
    [string]$HostName = "10.200.1.13",
    [string]$Port = "8123",
    [string]$User = "capstone",
    [string]$Password = "boogle",
    [string]$Database = "net",

    # Use -Force to skip the "Are you sure?" confirmation prompt
    [Switch]$Force
)

$ErrorActionPreference = "Stop"

# ===== Connection Setup =====
$BaseUrl = "http://${HostName}:${Port}"
# Basic Auth Header
$AuthBytes = [System.Text.Encoding]::ASCII.GetBytes("${User}:${Password}")
$Base64Auth = [System.Convert]::ToBase64String($AuthBytes)
$Headers = @{ Authorization = "Basic $Base64Auth" }

Write-Host "`n=== ClickHouse Data Wipe (schema preserved, engine-aware) ===" -ForegroundColor Cyan
Write-Host "DB: $Database"
Write-Host "Host: $HostName"
Write-Host "Port: $Port`n"

# ===== Core Logic =====

function Invoke-ClickHouseQuery {
    param([string]$Query)
    try {
        # Using wait_end_of_query=1 to ensure operations complete
        Invoke-RestMethod -Uri "$BaseUrl/?wait_end_of_query=1" -Method Post -Body $Query -Headers $Headers -ErrorAction Stop
    }
    catch {
        if ($_.Exception.Response) {
            $Stream = $_.Exception.Response.GetResponseStream()
            $Reader = New-Object System.IO.StreamReader($Stream)
            Write-Error "ClickHouse Error: $($Reader.ReadToEnd())"
        }
        throw $_
    }
}

function Clear-Table {
    param([string]$FqTable)
    
    # Try TRUNCATE first
    try {
        $null = Invoke-ClickHouseQuery "TRUNCATE TABLE $FqTable"
        Write-Host "   TRUNCATE $FqTable" -ForegroundColor Green
        return
    } catch { }

    # Try ALTER DELETE (Fallback)
    try {
        $null = Invoke-ClickHouseQuery "ALTER TABLE $FqTable DELETE WHERE 1 SETTINGS mutations_sync=2"
        Write-Host "   DELETE WHERE 1 $FqTable" -ForegroundColor Green
    } catch {
        Write-Host "   Could not clear $FqTable" -ForegroundColor Red
    }
}

function Get-RowCount {
    param([string]$FqTable)
    try {
        $Res = Invoke-ClickHouseQuery "SELECT count() as c FROM $FqTable FORMAT JSON"
        return $Res.data[0].c
    } catch {
        return "__ERR__"
    }
}

# ===== Discovery =====

Write-Host "Discovering tables..." -ForegroundColor Yellow

$DiscoveryQuery = @"
SELECT
    name,
    engine,
    regexpExtract(create_table_query, '(?i)\\bTO\\s+([A-Za-z0-9_]+)\\.([A-Za-z0-9_]+)', 1) AS to_db,
    regexpExtract(create_table_query, '(?i)\\bTO\\s+([A-Za-z0-9_]+)\\.([A-Za-z0-9_]+)', 2) AS to_tbl
FROM system.tables
WHERE database = '$Database'
FORMAT JSON
"@

try {
    $Response = Invoke-ClickHouseQuery $DiscoveryQuery
} catch {
    Write-Error "Failed to retrieve table list. Check connection."
    exit 1
}

$ClearSet = [System.Collections.Generic.HashSet[string]]::new()
$SkipNotes = @{}

foreach ($row in $Response.data) {
    $Fq = "$Database.$($row.name)"
    
    switch -Regex ($row.engine) {
        "MergeTree" { $null = $ClearSet.Add($Fq) }
        "MaterializedView" {
            if (-not [string]::IsNullOrWhiteSpace($row.to_db) -and -not [string]::IsNullOrWhiteSpace($row.to_tbl)) {
                $Target = "$($row.to_db).$($row.to_tbl)"
                $null = $ClearSet.Add($Target)
                $SkipNotes[$Fq] = "MV -> clears target $Target"
            } else {
                $SkipNotes[$Fq] = "MV without TO (no storage)"
            }
        }
        "View" { $SkipNotes[$Fq] = "view (no storage)" }
        "Distributed" {
            $null = $ClearSet.Add($Fq)
            $SkipNotes[$Fq] = "Distributed (best effort)"
        }
        Default { $null = $ClearSet.Add($Fq) }
    }
}

if ($ClearSet.Count -eq 0) {
    Write-Host "No tables found in database '$Database'." -ForegroundColor Yellow
    exit 0
}

Write-Host "Targets:"
foreach ($tbl in $ClearSet) {
    Write-Host "  - $tbl"
}
Write-Host ""

# ===== Confirmation =====
if (-not $Force) {
    $Confirm = Read-Host "Remove ALL rows from these tables? [y/N]"
    if ($Confirm -notmatch "^[Yy]$") {
        Write-Host "Aborted." -ForegroundColor Yellow
        exit 0
    }
} else {
    Write-Host "Force switch detected. Skipping confirmation." -ForegroundColor DarkGray
}

# ===== Execution =====
foreach ($tbl in $ClearSet) {
    Write-Host "  Clearing $tbl..." -ForegroundColor Cyan
    Clear-Table $tbl
}

# ===== Verification =====
Write-Host "`nVerifying row counts:" -ForegroundColor Cyan
$FailCount = 0

foreach ($tbl in $ClearSet) {
    $Count = Get-RowCount $tbl
    
    if ($Count -eq "__ERR__") {
        Write-Host "   $tbl : query error" -ForegroundColor Red
        $FailCount++
    } elseif ($Count -eq 0) {
        Write-Host "   $tbl : 0 rows" -ForegroundColor Green
    } else {
        Write-Host "   $tbl : $Count rows remain" -ForegroundColor Yellow
        $FailCount++
    }
}

if ($FailCount -eq 0) {
    Write-Host "All cleared." -ForegroundColor Green
} else {
    Write-Host "Some tables still have data." -ForegroundColor Yellow
}