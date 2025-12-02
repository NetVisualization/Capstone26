<#
.SYNOPSIS
    Executes a ClickHouse query and displays the output as PowerShell objects.
    Usage: .\query_db.ps1 "SELECT * FROM system.databases"
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory=$true, Position=0)]
    [string]$Query,

    [string]$HostName = "localhost",
    [string]$Port = "8123",
    [string]$User = "capstone",
    [string]$Password = "boogle"
)

$ErrorActionPreference = "Stop"

# ===== Connection Setup =====
$BaseUrl = "http://${HostName}:${Port}"
$AuthBytes = [System.Text.Encoding]::ASCII.GetBytes("${User}:${Password}")
$Base64Auth = [System.Convert]::ToBase64String($AuthBytes)
$Headers = @{ Authorization = "Basic $Base64Auth" }

# ===== Core Function =====
function Invoke-ClickHouseQuery {
    param([string]$Sql)

    # 1. Strip existing FORMAT clause to prevent conflicts
    $CleanSql = $Sql -replace '(?i)\s+FORMAT\s+\w+\s*$', ''
    
    # 2. Force JSON format. 
    # This allows PowerShell to parse the response into objects automatically.
    $FinalSql = "$CleanSql FORMAT JSON"

    try {
        $Response = Invoke-RestMethod -Uri "$BaseUrl/?wait_end_of_query=1" `
            -Method Post `
            -Body $FinalSql `
            -Headers $Headers `
            -ErrorAction Stop

        # 3. Return the 'data' block. 
        # ClickHouse JSON responses look like: { "meta": [...], "data": [ ... ], "rows": N }
        if ($Response.data) {
            return $Response.data
        }
        else {
            Write-Host "Query executed successfully. No rows returned." -ForegroundColor Gray
        }
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

# ===== Execution =====
Write-Host "Viewing nodes table:" -ForegroundColor Green
Invoke-ClickHouseQuery "SELECT * from display_nodes"

Read-Host -Prompt "Press Enter to continue..."
Write-Host "Viewing connections table:" -ForegroundColor Green
Invoke-ClickHouseQuery "SELECT * from display_connections"