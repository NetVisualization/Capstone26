#!/usr/bin/env bash
set -euo pipefail

# --- Config (fixed for demo) ---
BIN_PATH="../../parser/NetVis/pcap2ch/target/release/pcap2ch"
RUST_LOG_LEVEL="info"

CH_URL="http://localhost:8123"
CH_USER="capstone"
CH_PASSWORD="boogle"
CH_HEAD_TABLE="packets"
CH_RAW_TABLE="raw_bytes"
CH_DB="net"

# --- Helpers ---
ask() {
  # ask "Prompt" VAR_NAME [default]
  local prompt var default _input
  prompt="$1"; var="$2"; default="${3-}"
  read -r -p "$prompt${default:+ [$default]}: " _input || true
  printf -v "$var" "%s" "${_input:-$default}"
}

require_binary() {
  if [[ ! -x "$BIN_PATH" ]]; then
    echo "Error: binary not found or not executable at: $BIN_PATH"
    echo "Update BIN_PATH in this script or build the project first."
    exit 1
  fi
}

# --- Collect Inputs ---
echo "NetVis live capture → ClickHouse ingest"
echo

# Support zero-prompt mode: iface as first arg, or ask if not provided
IFACE="${1-}"
if [[ -z "${IFACE}" ]]; then
  ask "Live capture interface (e.g., eth0)" IFACE
fi

if [[ -z "${IFACE}" ]]; then
  echo "Error: interface is required."
  exit 1
fi

# --- Summary (optional, nice for demo) ---
cat <<EOF

Running:
  RUST_LOG=$RUST_LOG_LEVEL "$BIN_PATH" \\
    --ch-url "$CH_URL" \\
    --ch-db "$CH_DB" \\
    --ch-user "$CH_USER" \\
    --ch-password "$CH_PASSWORD" \\
    --ch-head-table "$CH_HEAD_TABLE" \\
    --ch-raw-table "$CH_RAW_TABLE" \\
    live \\
    --iface "$IFACE"

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
  live \
  --iface "$IFACE"
