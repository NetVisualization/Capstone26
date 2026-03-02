#!/usr/bin/env bash

# ensure sudo
if [[ $EUID -ne 0 ]]; then
    echo "Error: This script must be run as root." >&2
    exit 1
fi

set -euo pipefail

# --- Config ---
BIN_PATH="../bin/pcap2ch/target/release/pcap2ch"
SCANS_DIR="../scans/"
DOCKER_DIR="../docker/"
RUST_LOG_LEVEL="info"

# Zeek/Weird import config
ZEEK2CH_DIR="../bin/zeek2ch"
WEIRD_PY="$ZEEK2CH_DIR/zeek.py"
ZEEK_WEIRD_LOG="../zeek/logs/weird.log"
ZEEK_NOTICE_LOG="../zeek/logs/notice.log"

# --- Helpers ---
ask() {
  local prompt var default
  prompt="$1"; var="$2"; default="${3-}"
  read -r -p "$prompt${default:+ [$default]}: " _input || true
  printf -v "$var" "%s" "${_input:-$default}"
}

ask_secret() {
  local prompt var
  prompt="$1"; var="$2"
  read -r -s -p "$prompt (press Enter to skip): " _input || true
  echo
  printf -v "$var" "%s" "$_input"
}

get_default_nic() {
  case "$OSTYPE" in
    darwin*)
      route -n get default 2>/dev/null | grep 'interface:' | awk '{print $2}' || echo "en0" ;;
    linux*)
      ip route | grep default | awk '{print $5}' | head -n1 || echo "eth0" ;;
    msys* | cygwin* | mingw*)
      netstat -rn | grep '0.0.0.0' | awk '{print $4}' | head -n1 || echo "eth0" ;;
    *) echo "eth0" ;;
  esac
}

require_binary() {
  if [[ ! -x "$BIN_PATH" ]]; then
    echo "❌ Error: binary not found at $BIN_PATH. Build the project first."
    exit 1
  fi
}

run_weird_import() {
  # Runs ../bin/zeek2ch/weird.py via its venv (if present) to import ../zeek/zeek-logs/weird.log
  if [[ ! -f "$WEIRD_PY" ]]; then
    echo "⚠️  Skipping weird.log import: script not found at $WEIRD_PY"
    return 0
  fi

  if [[ ! -f "$ZEEK_WEIRD_LOG" ]]; then
    echo "⚠️  Skipping weird.log import: log file not found at $ZEEK_WEIRD_LOG"
    return 0
  fi

  # Prefer venv in ../bin/zeek2ch, fall back to system python3
  local PYTHON_BIN
  if [[ -x "$ZEEK2CH_DIR/venv/bin/python" ]]; then
    PYTHON_BIN="$ZEEK2CH_DIR/venv/bin/python"
  elif [[ -x "$ZEEK2CH_DIR/.venv/bin/python" ]]; then
    PYTHON_BIN="$ZEEK2CH_DIR/.venv/bin/python"
  else
    PYTHON_BIN="python3"
  fi

  echo "➡️  Importing Zeek weird.log via zeek2ch..."
  "$PYTHON_BIN" "$WEIRD_PY" \
    --host "$CH_HOST" \
    --port "$CH_PORT" \
    --user "$CH_USER" \
    --password "$CH_PASSWORD" \
    --weird "$ZEEK_WEIRD_LOG" \
    --notice "$ZEEK_NOTICE_LOG"

  echo "✅ Zeek weird.log import complete."
}

# --- Main Menu ---
echo "========================================"
echo "    NetVis Centralized Controller       "
echo "========================================"
echo "1) Setup ClickHouse (Docker Compose)"
echo "2) Live Capture (NIC)"
echo "3) File Import (.pcap/.pcapng)"
echo "4) Reset/Wipe Database (Preserve Schema)"
echo "q) Quit"
read -r -p "Select an option: " MODE

# If user just hits Enter with no input
if [[ -z "$MODE" ]]; then
  echo "❌ Error: No option selected. Please run the script again and choose an option." >&2
  exit 1
fi

[[ "$MODE" == "q" ]] && exit 0

# --- Option 1: Docker (No DB Prompts) ---
if [[ "$MODE" == "1" ]]; then
    if [[ ! -d "$DOCKER_DIR" ]]; then
        echo "❌ Error: Docker directory $DOCKER_DIR not found."
        exit 1
    fi
    echo "➡️  Launching NetVis stack..."
    (cd "$DOCKER_DIR" && docker compose up -d)
    echo "✅ Containers are starting. Database schema will be initialized automatically."
    exit 0
fi

# --- DB Connection Inputs (For Options 2, 3, 4) ---
echo -e "\n--- ClickHouse Connection ---"
ask "ClickHouse Host"     CH_HOST     "localhost"
ask "ClickHouse Port"     CH_PORT     "8123"
ask "ClickHouse Database" CH_DB       "net"
ask "ClickHouse User"     CH_USER     "capstone"
ask_secret "ClickHouse Password"      CH_PASSWORD
CH_PASSWORD="${CH_PASSWORD:-boogle}"
CH_URL="http://${CH_HOST}:${CH_PORT}"

case "$MODE" in
  2) # Live Capture
    DEFAULT_IFACE=$(get_default_nic)
    ask "Live interface" IFACE "$DEFAULT_IFACE"
    require_binary
    export RUST_LOG="$RUST_LOG_LEVEL"
    "$BIN_PATH" --ch-url "$CH_URL" --ch-db "$CH_DB" --ch-user "$CH_USER" --ch-password "$CH_PASSWORD" \
      live --iface "$IFACE"

    # 🔁 After live capture finishes, run the Zeek weird.py import
    run_weird_import
    ;;

  3) # File Import
    mkdir -p "$SCANS_DIR"
    echo "Files in $SCANS_DIR:"
    ls -1 "$SCANS_DIR" 2>/dev/null | sed 's/^/  - /' || echo "  (Empty)"

    ask "Enter pcap filename" FILE_NAME
    FILE_PATH="${SCANS_DIR}${FILE_NAME}"

    if [[ ! -f "$FILE_PATH" ]]; then
      echo "❌ Error: File not found at $FILE_PATH"
      exit 1
    fi

    require_binary
    export RUST_LOG="$RUST_LOG_LEVEL"
    "$BIN_PATH" --ch-url "$CH_URL" --ch-db "$CH_DB" --ch-user "$CH_USER" --ch-password "$CH_PASSWORD" \
      file --path "$FILE_PATH" --insert --batch-size 5000

    # 🔁 After file import, run the Zeek weird.py import
    run_weird_import
    ;;

  4) # Reset DB (404 Fix: Target root / and use fully qualified names)
    echo "➡️  Discovering tables in $CH_DB..."

    TABLES=$(curl -fsS -u "$CH_USER:$CH_PASSWORD" \
      --data-binary "SELECT name FROM system.tables WHERE database='$CH_DB' AND engine LIKE '%MergeTree%'" \
      "$CH_URL/?wait_end_of_query=1")

    if [[ -z "$TABLES" ]]; then
        echo "No MergeTree tables found in $CH_DB."
        exit 0
    fi

    echo "Tables to be wiped:"
    for T in $TABLES; do echo "  - $CH_DB.$T"; done

    read -r -p "Confirm data wipe? [y/N]: " ok
    if [[ "$ok" =~ ^[Yy]$ ]]; then
      for T in $TABLES; do
        echo "   Clearing $CH_DB.$T..."
        curl -fsS -u "$CH_USER:$CH_PASSWORD" \
          --data-binary "TRUNCATE TABLE $CH_DB.$T" \
          "$CH_URL/?wait_end_of_query=1"
      done
      echo "🎯 Reset complete."
    else
      echo "Aborted."
    fi
    ;;

  *) echo "Invalid option." ;;
esac
