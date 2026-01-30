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

ask_secret_required() {
  # ask_secret_required "Prompt" VAR_NAME
  local prompt var
  prompt="$1"; var="$2"
  while true; do
    read -r -s -p "$prompt: " _input || true
    echo
    if [[ -z "$_input" ]]; then
      echo "This value cannot be empty. Please enter a password."
    else
      printf -v "$var" "%s" "$_input"
      break
    fi
  done
}

ask_required() {
  # ask_required "Prompt" VAR_NAME
  local prompt var
  prompt="$1"; var="$2"
  while true; do
    read -r -p "$prompt: " _input || true
    if [[ -z "$_input" ]]; then
      echo "This value cannot be empty."
    else
      printf -v "$var" "%s" "$_input"
      break
    fi
  done
}

require_binary() {
  if [[ ! -x "$BIN_PATH" ]]; then
    echo "Error: binary not found or not executable at: $BIN_PATH"
    echo "Update BIN_PATH in this script or build the project first."
    exit 1
  fi
}

# --- Collect Inputs ---
echo "Configure NetVis file capture → ClickHouse ingest"
echo "(You may leave fields blank except Password and Path.)"
echo

ask       "ClickHouse URL (e.g., http://localhost:8123)" CH_URL
ask       "ClickHouse Database"                          CH_DB
ask       "ClickHouse User"                              CH_USER
ask_secret_required "ClickHouse Password"                CH_PASSWORD
ask       "ClickHouse Head table"                        CH_HEAD_TABLE
ask       "ClickHouse Raw  table"                        CH_RAW_TABLE
ask_required "Path to pcap/pcapng file"                  FILE_PATH

# --- Summary ---
cat <<EOF

Summary:
  CH URL:            ${CH_URL:-<empty>}
  CH DB:             ${CH_DB:-<empty>}
  CH User:           ${CH_USER:-<empty>}
  CH Password:       (hidden)
  CH Head Table:     ${CH_HEAD_TABLE:-<empty>}
  CH Raw  Table:     ${CH_RAW_TABLE:-<empty>}
  File Path:         $FILE_PATH

Command to run:
  RUST_LOG=$RUST_LOG_LEVEL "$BIN_PATH" \\
    ${CH_URL:+--ch-url "$CH_URL"} \\
    ${CH_DB:+--ch-db "$CH_DB"} \\
    ${CH_USER:+--ch-user "$CH_USER"} \\
    --ch-password "$CH_PASSWORD" \\
    ${CH_HEAD_TABLE:+--ch-head-table "$CH_HEAD_TABLE"} \\
    ${CH_RAW_TABLE:+--ch-raw-table "$CH_RAW_TABLE"} \\
    file \\
    --path "$FILE_PATH" \\
    --insert \\
    --batch-size 5000

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
  --ch-password "$CH_PASSWORD" \
  ${CH_HEAD_TABLE:+--ch-head-table "$CH_HEAD_TABLE"} \
  ${CH_RAW_TABLE:+--ch-raw-table "$CH_RAW_TABLE"} \
  file \
  --path "$FILE_PATH" \
  --insert \
  --batch-size 5000
