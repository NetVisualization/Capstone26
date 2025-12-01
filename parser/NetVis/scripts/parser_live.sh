#!/usr/bin/env bash
set -euo pipefail

# --- Config ---
BIN_PATH="../pcap2ch/target/release/pcap2ch"
RUST_LOG_LEVEL="info"

# --- Helpers ---
ask() {
  # ask "Prompt" VAR_NAME [default]
  local prompt var default
  prompt="$1"; var="$2"; default="${3-}"
  read -r -p "$prompt${default:+ [$default]}: " _input || true
  printf -v "$var" "%s" "${_input:-$default}"
}

ask_secret() {
  # ask_secret "Prompt" VAR_NAME
  local prompt var
  prompt="$1"; var="$2"
  read -r -s -p "$prompt (press Enter to skip): " _input || true
  echo
  printf -v "$var" "%s" "$_input"
}

require_binary() {
  if [[ ! -x "$BIN_PATH" ]]; then
    echo "Error: binary not found or not executable at: $BIN_PATH"
    echo "Update BIN_PATH in this script or build the project first."
    exit 1
  fi
}

# --- Collect Inputs ---
echo "Configure NetVis live capture → ClickHouse ingest"
echo "(You can leave any field blank except where noted.)"
echo

ask       "ClickHouse URL (e.g., http://localhost:8123)" CH_URL
ask       "ClickHouse Database"                          CH_DB
ask       "ClickHouse User"                              CH_USER
ask_secret "ClickHouse Password"                         CH_PASSWORD
ask       "ClickHouse Head table"                        CH_HEAD_TABLE
ask       "ClickHouse Raw  table"                        CH_RAW_TABLE
ask       "Live capture interface (e.g., eth0)"          IFACE

# --- Summary ---
cat <<EOF

Summary:
  CH URL:            ${CH_URL:-<empty>}
  CH DB:             ${CH_DB:-<empty>}
  CH User:           ${CH_USER:-<empty>}
  CH Password:       (hidden)
  CH Head Table:     ${CH_HEAD_TABLE:-<empty>}
  CH Raw  Table:     ${CH_RAW_TABLE:-<empty>}
  Live IFACE:        ${IFACE:-<empty>}

Command to run:
  RUST_LOG=$RUST_LOG_LEVEL "$BIN_PATH" \\
    ${CH_URL:+--ch-url "$CH_URL"} \\
    ${CH_DB:+--ch-db "$CH_DB"} \\
    ${CH_USER:+--ch-user "$CH_USER"} \\
    ${CH_PASSWORD:+--ch-password "$CH_PASSWORD"} \\
    ${CH_HEAD_TABLE:+--ch-head-table "$CH_HEAD_TABLE"} \\
    ${CH_RAW_TABLE:+--ch-raw-table "$CH_RAW_TABLE"} \\
    live \\
    ${IFACE:+--iface "$IFACE"}

EOF

read -r -p "Proceed? [y/N]: " CONFIRM
if [[ ! "$CONFIRM" =~ ^[Yy]$ ]]; then
  echo "Aborted."
  exit 0
fi

# --- Execute ---
require_binary
export RUST_LOG="$RUST_LOG_LEVEL"

"$BIN_PATH" \
  ${CH_URL:+--ch-url "$CH_URL"} \
  ${CH_DB:+--ch-db "$CH_DB"} \
  ${CH_USER:+--ch-user "$CH_USER"} \
  ${CH_PASSWORD:+--ch-password "$CH_PASSWORD"} \
  ${CH_HEAD_TABLE:+--ch-head-table "$CH_HEAD_TABLE"} \
  ${CH_RAW_TABLE:+--ch-raw-table "$CH_RAW_TABLE"} \
  live \
  ${IFACE:+--iface "$IFACE"}
