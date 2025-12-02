# Configuration
$exePath = "..\..\parser\NetVis\pcap2ch\target\release\pcap2ch.exe"

# Execute
& $exePath `
  --ch-url "http://10.200.1.13:8123" `
  --ch-db "net" `
  --ch-user "capstone" `
  --ch-password "boogle" `
  --ch-head-table "packets" `
  --ch-raw-table "raw_bytes" `
  file `
  --path "..\scans\demo.pcapng" `
  --insert `
  --batch-size 5000