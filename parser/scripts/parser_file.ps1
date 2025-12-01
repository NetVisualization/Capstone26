# Configuration
$repoRoot = Split-Path -Parent $PSScriptRoot
$projectRoot = Join-Path -Path $repoRoot -ChildPath "NetVis\pcap2ch"
$exePath = "$projectRoot\target\release\pcap2ch.exe"
$env:LIB = "$projectRoot\lib\npcap-sdk-1.15\Lib\x64"

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
  file `
  --path "C:\Users\eoliv\development\test.pcapng" `
  --insert `
  --batch-size 5000