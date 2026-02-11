# --- Config ---
$BIN_PATH = "../pcap2ch/target/release/pcap2ch.exe"
$SCANS_DIR = "../scans/"
$DOCKER_DIR = "../docker/"
$env:RUST_LOG = "info"

# --- Helpers ---
function Ask-User($Prompt, $Default) {
    $result = Read-Host "$Prompt [$Default]"
    if ([string]::IsNullOrWhiteSpace($result)) { return $Default }
    return $result
}

function Ask-Secret($Prompt) {
    # Note: Read-Host -MaskInput is available in PS 7. For PS 5.1, it shows text.
    $result = Read-Host "$Prompt (press Enter to skip)"
    return $result
}

function Get-DefaultNIC {
    # Finds the interface associated with the default gateway (0.0.0.0)
    $route = Get-NetRoute -DestinationPrefix "0.0.0.0/0" | Sort-Object RouteMetric | Select-Object -First 1
    if ($route) {
        $iface = Get-NetIPInterface -InterfaceIndex $route.InterfaceIndex -AddressFamily IPv4
        return (Get-NetAdapter -InterfaceIndex $route.InterfaceIndex).Name
    }
    return "Ethernet"
}

function Require-Binary {
    if (-not (Test-Path $BIN_PATH)) {
        Write-Error "❌ Error: binary not found at $BIN_PATH. Build the project first."
        exit 1
    }
}

# --- Main Menu ---
Clear-Host
Write-Host "========================================"
Write-Host "    NetVis Centralized Controller       "
Write-Host "========================================"
Write-Host "1) Setup ClickHouse (Docker Compose)"
Write-Host "2) Live Capture (NIC)"
Write-Host "3) File Import (.pcap/.pcapng)"
Write-Host "4) Reset/Wipe Database (Preserve Schema)"
Write-Host "q) Quit"
$MODE = Read-Host "Select an option"

if ($MODE -eq "q") { exit }

# --- Option 1: Docker (No DB Prompts) ---
if ($MODE -eq "1") {
    if (-not (Test-Path $DOCKER_DIR)) {
        Write-Host "❌ Error: Docker directory $DOCKER_DIR not found." -ForegroundColor Red
        exit 1
    }
    Write-Host "➡️ Launching NetVis stack..." -ForegroundColor Cyan
    Push-Location $DOCKER_DIR
    docker-compose up -d
    Pop-Location
    Write-Host "✅ Containers are starting. Database schema will be initialized automatically." -ForegroundColor Green
    exit
}

# --- DB Connection Inputs (Options 2, 3, 4) ---
Write-Host "`n--- ClickHouse Connection ---" -ForegroundColor Yellow
$CH_HOST     = Ask-User "ClickHouse Host" "localhost"
$CH_PORT     = Ask-User "ClickHouse Port" "8123"
$CH_DB       = Ask-User "ClickHouse Database" "net"
$CH_USER     = Ask-User "ClickHouse User" "capstone"
$CH_PASSWORD = Ask-Secret "ClickHouse Password"
if ([string]::IsNullOrWhiteSpace($CH_PASSWORD)) { $CH_PASSWORD = "boogle" }

$CH_URL = "http://$($CH_HOST):$($CH_PORT)"

switch ($MODE) {
    "2" { # Live Capture
        $DEFAULT_IFACE = Get-DefaultNIC
        $IFACE = Ask-User "Live interface" $DEFAULT_IFACE
        Require-Binary
        & $BIN_PATH --ch-url $CH_URL --ch-db $CH_DB --ch-user $CH_USER --ch-password $CH_PASSWORD live --iface $IFACE
    }

    "3" { # File Import
        if (-not (Test-Path $SCANS_DIR)) { New-Item -ItemType Directory -Path $SCANS_DIR | Out-Null }
        Write-Host "Files in $SCANS_DIR:" -ForegroundColor Cyan
        Get-ChildItem $SCANS_DIR | ForEach-Object { Write-Host "  - $($_.Name)" }

        $FILE_NAME = Read-Host "Enter pcap filename"
        $FILE_PATH = Join-Path $SCANS_DIR $FILE_NAME

        if (-not (Test-Path $FILE_PATH)) {
            Write-Host "❌ Error: File not found at $FILE_PATH" -ForegroundColor Red
            exit 1
        }

        Require-Binary
        & $BIN_PATH --ch-url $CH_URL --ch-db $CH_DB --ch-user $CH_USER --ch-password $CH_PASSWORD file --path $FILE_PATH --insert --batch-size 5000
    }

    "4" { # Reset DB
        Write-Host "➡️ Discovering tables in $CH_DB..." -ForegroundColor Cyan

        # Using curl.exe directly to maintain consistency with the binary data behavior
        $auth = "$($CH_USER):$($CH_PASSWORD)"
        $query = "SELECT name FROM system.tables WHERE database='$CH_DB' AND engine LIKE '%MergeTree%'"

        $TABLES = curl.exe -fsS -u $auth --data-binary $query "$CH_URL/?wait_end_of_query=1"

        if ([string]::IsNullOrWhiteSpace($TABLES)) {
            Write-Host "No MergeTree tables found in $CH_DB."
            exit
        }

        Write-Host "Tables to be wiped:" -ForegroundColor Yellow
        $tableList = $TABLES -split "`n" | Where-Object { $_ -ne "" }
        foreach ($T in $tableList) { Write-Host "  - $CH_DB.$T" }

        $ok = Read-Host "Confirm data wipe? [y/N]"
        if ($ok -eq "y") {
            foreach ($T in $tableList) {
                Write-Host "   Clearing $CH_DB.$T..." -ForegroundColor Cyan
                curl.exe -fsS -u $auth --data-binary "TRUNCATE TABLE $CH_DB.$T" "$CH_URL/?wait_end_of_query=1"
            }
            Write-Host "🎯 Reset complete." -ForegroundColor Green
        } else {
            Write-Host "Aborted."
        }
    }

    Default { Write-Host "Invalid option." }
}
