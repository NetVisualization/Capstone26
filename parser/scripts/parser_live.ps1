# Configuration
$projectRoot = "C:\Users\eoliv\development\capstone26\parser\NetVis\pcap2ch"
$exePath = "$projectRoot\target\release\pcap2ch.exe"
$env:RUSTFLAGS="-L C:\Users\eoliv\development\deps\npcap-sdk-1.15\Lib\x64\"

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
 --ch-url "http://10.200.1.13:8123" `
 --ch-db "net" `
 --ch-user "capstone" `
 --ch-password "boogle" `
 --ch-head-table "packets" `
 --ch-raw-table "raw_bytes" `
 live `
 --iface "en0"