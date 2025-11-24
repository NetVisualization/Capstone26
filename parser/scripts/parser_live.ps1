# Configuration
$repoRoot = Split-Path -Parent $PSScriptRoot
$projectRoot = Join-Path -Path $repoRoot -ChildPath "NetVis\pcap2ch"
$exePath = "$projectRoot\target\release\pcap2ch.exe"

# Check if executable exists; compile if not
if (-not (Test-Path -Path $exePath)) {
    Write-Host "Executable not found. Compiling from source..."
    Push-Location -Path $projectRoot
    cargo build --release
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Compilation failed. Exiting."
        Pop-Location
        exit 1
    }
    Pop-Location
    Write-Host "Compilation complete."
}

# Set Environment Variable
$env:RUST_LOG = "info"

# Execute
& $exePath `
 --ch-url "http://localhost:8123" `
 --ch-db "net" `
 --ch-user "capstone" `
 --ch-password "boogle" `
 --ch-head-table "packets" `
 --ch-raw-table "raw_bytes" `
 live `
 --iface "Ethernet"