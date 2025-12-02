<#
.SYNOPSIS
    ClickHouse Data Wipe (schema preserved, engine-aware)
#>

[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"

# ===== Connection Defaults =====
$DEFAULT_HOST = "localhost"
$DEFAULT_PORT = "8123"
$DEFAULT_USER = "capstone"
$DEFAULT_PASSWORD = "boogle"
$DEFAULT_DB = "net"

# ===== Helper: Prompt with Default =====
function Read-Input {
    param($Prompt, $Default, $IsPassword=$false)
    if ($IsPassword) {
        $inputVal = Read-Host "$Prompt [$Default]" -AsSecureString
        if (-not $inputVal) { return $Default }
        $BSTR = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($inputVal)
        return [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR)
    }
    $inputVal = Read-Host "$Prompt [$Default]"
    if ([string]::IsNullOrWhiteSpace($inputVal)) { return $Default }
    return $inputVal
}

# ===== User Input =====
$CH_HOST = Read-Input "ClickHouse Host" $DEFAULT_HOST
$CH_PORT = Read-Input "ClickHouse Port" $DEFAULT_PORT
$CH_USER = Read-Input "ClickHouse User" $DEFAULT_USER
# Note: For simplicity in API calls, we convert the secure string back to plain text immediately, 
# but in a production script, you would handle credentials more securely.
$CH_PASSWORD = Read-Input "ClickHouse Password" $DEFAULT_PASSWORD $true
$DB_NAME = Read-Input "Database" $DEFAULT_DB

$BaseUrl = "http://${CH_HOST}:${CH_PORT}"
# Create Basic Auth Header
$AuthBytes = [System.Text.Encoding]::ASCII.GetBytes("${CH_USER}:${CH_PASSWORD}")
$Base64Auth = [System.Convert]::ToBase64String($AuthBytes)
$Headers = @{ Authorization = "Basic $Base64Auth" }

Write-Host "`n=== ClickHouse Data Wipe (schema preserved, engine-aware) ===" -ForegroundColor Cyan
Write-Host "DB: $DB_NAME"
Write-Host "Host: $CH_HOST"
Write-Host "Port: $CH_PORT`n"

# ===== Core Logic =====

function Invoke-ClickHouseQuery {
    param([string]$Query)
    try {
        # Using wait_end_of_query=1 to ensure operations complete before moving on
        Invoke-RestMethod -Uri "$BaseUrl/?wait_end_of_query=1" -Method Post -Body $Query -Headers $Headers -ErrorAction Stop
    }
    catch {
        # Return error details if possible
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
        Write-Host "   ✅ TRUNCATE $FqTable" -ForegroundColor Green
        return
    } catch {
        # Fallback to DELETE (needed for some engines or older versions)
    }

    # Try ALTER DELETE
    try {
        $null = Invoke-ClickHouseQuery "ALTER TABLE $FqTable DELETE WHERE 1 SETTINGS mutations_sync=2"
        Write-Host "   ✅ DELETE WHERE 1 $FqTable" -ForegroundColor Green
    } catch {
        Write-Host "   ❌ Could not clear $FqTable" -ForegroundColor Red
    }
}

function Get-RowCount {
    param([string]$FqTable)
    try {
        # Parse JSON output strictly
        $Res = Invoke-ClickHouseQuery "SELECT count() as c FROM $FqTable FORMAT JSON"
        return $Res.data[0].c
    } catch {
        return "__ERR__"
    }
}

# ===== Discovery =====

Write-Host "➡️  Discovering tables..." -ForegroundColor Yellow

# Query matches the Bash regex logic but requests JSON format for easier PowerShell parsing
$DiscoveryQuery = @"
SELECT
    name,
    engine,
    regexpExtract(create_table_query, '(?i)\\bTO\\s+([A-Za-z0-9_]+)\\.([A-Za-z0-9_]+)', 1) AS to_db,
    regexpExtract(create_table_query, '(?i)\\bTO\\s+([A-Za-z0-9_]+)\\.([A-Za-z0-9_]+)', 2) AS to_tbl
FROM system.tables
WHERE database = '$DB_NAME'
FORMAT JSON
"@

try {
    $Response = Invoke-ClickHouseQuery $DiscoveryQuery
} catch {
    Write-Error "Failed to retrieve table list. Check connection."
    exit 1
}

# Use a HashSet to store unique tables to clear (deduplication)
$ClearSet = [System.Collections.Generic.HashSet[string]]::new()
$SkipNotes = @{}

foreach ($row in $Response.data) {
    $Fq = "$DB_NAME.$($row.name)"
    
    switch -Regex ($row.engine) {
        "MergeTree" { 
            $null = $ClearSet.Add($Fq) 
        }
        "MaterializedView" {
            if (-not [string]::IsNullOrWhiteSpace($row.to_db) -and -not [string]::IsNullOrWhiteSpace($row.to_tbl)) {
                $Target = "$($row.to_db).$($row.to_tbl)"
                $null = $ClearSet.Add($Target)
                $SkipNotes[$Fq] = "MV -> clears target $Target"
            } else {
                $SkipNotes[$Fq] = "MV without TO (no storage)"
            }
        }
        "View" {
            $SkipNotes[$Fq] = "view (no storage)"
        }
        "Distributed" {
            $null = $ClearSet.Add($Fq)
            $SkipNotes[$Fq] = "Distributed (best effort)"
        }
        Default {
            # Default fallback for Log, TinyLog, etc.
            $null = $ClearSet.Add($Fq)
        }
    }
}

Write-Host "Targets:"
foreach ($tbl in $ClearSet) {
    Write-Host "  - $tbl"
}
Write-Host ""

# ===== Confirmation =====
$Confirm = Read-Host "Remove ALL rows from these tables? [y/N]"
if ($Confirm -notmatch "^[Yy]$") {
    Write-Host "Aborted." -ForegroundColor Yellow
    exit 0
}

# ===== Execution =====
foreach ($tbl in $ClearSet) {
    Write-Host "➡️  Clearing $tbl..." -ForegroundColor Cyan
    Clear-Table $tbl
}

# ===== Verification =====
Write-Host "`n🧪 Verifying row counts:" -ForegroundColor Cyan
$FailCount = 0

foreach ($tbl in $ClearSet) {
    $Count = Get-RowCount $tbl
    
    if ($Count -eq "__ERR__") {
        Write-Host "   ❌ $tbl : query error" -ForegroundColor Red
        $FailCount++
    } elseif ($Count -eq 0) {
        Write-Host "   ✅ $tbl : 0 rows" -ForegroundColor Green
    } else {
        Write-Host "   ⚠️  $tbl : $Count rows remain" -ForegroundColor Yellow
        $FailCount++
    }
}

if ($FailCount -eq 0) {
    Write-Host "🎯 All cleared." -ForegroundColor Green
} else {
    Write-Host "⚠️  Some tables still have data." -ForegroundColor Yellow
}