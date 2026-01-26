#!/usr/bin/env bash
set -euo pipefail

# --- Config (fixed for demo) ---
BIN_PATH="../../parser/NetVis/pcap2ch/target/release/pcap2ch"
RUST_LOG_LEVEL="info"

CH_URL="http://10.200.1.13:8123"
CH_USER="capstone"
CH_PASSWORD="boogle"
CH_HEAD_TABLE="packets"
CH_RAW_TABLE="raw_bytes"
CH_DB="net"
FILE_PATH="../scans/demo_v2.pcapng"

# --- Helpers ---
require_binary() {
  if [[ ! -x "$BIN_PATH" ]]; then
    echo "Error: binary not found or not executable at: $BIN_PATH"
    echo "Update BIN_PATH in this script or build the project first."
    exit 1
  fi
}

# --- Summary (nice for demo output) ---
echo "NetVis file capture → ClickHouse ingest"
echo
cat <<EOF
Running:
  RUST_LOG=$RUST_LOG_LEVEL "$BIN_PATH" \\
    --ch-url "$CH_URL" \\
    --ch-db "$CH_DB" \\
    --ch-user "$CH_USER" \\
    --ch-password "$CH_PASSWORD" \\
    --ch-head-table "$CH_HEAD_TABLE" \\
    --ch-raw-table "$CH_RAW_TABLE" \\
    file \\
    --path "$FILE_PATH" \\
    --insert \\
    --batch-size 5000

EOF

# --- Execute ---
require_binary
export RUST_LOG="$RUST_LOG_LEVEL"

"$BIN_PATH" \
  --ch-url "$CH_URL" \
  --ch-db "$CH_DB" \
  --ch-user "$CH_USER" \
  --ch-password "$CH_PASSWORD" \
  --ch-head-table "$CH_HEAD_TABLE" \
  --ch-raw-table "$CH_RAW_TABLE" \
  file \
  --path "$FILE_PATH" \
  --insert \
  --batch-size 5000
