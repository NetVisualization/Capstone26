#!/usr/bin/env bash
set -euo pipefail

# ===== Connection Defaults =====
DEFAULT_HOST="localhost"
DEFAULT_PORT="8123"
DEFAULT_USER="capstone"
DEFAULT_PASSWORD="boogle"
DEFAULT_DB="net"

# Prompt user (allow Enter to keep defaults)
read -r -p "ClickHouse Host [$DEFAULT_HOST]: " CH_HOST
read -r -p "ClickHouse Port [$DEFAULT_PORT]: " CH_PORT
read -r -p "ClickHouse User [$DEFAULT_USER]: " CH_USER
read -r -s -p "ClickHouse Password [$DEFAULT_PASSWORD]: " CH_PASSWORD; echo
read -r -p "Database [$DEFAULT_DB]: " DB_NAME
DB_NAME="${DB_NAME:-$DEFAULT_DB}"

CH_HOST="${CH_HOST:-$DEFAULT_HOST}"
CH_PORT="${CH_PORT:-$DEFAULT_PORT}"
CH_USER="${CH_USER:-$DEFAULT_USER}"
CH_PASSWORD="${CH_PASSWORD:-$DEFAULT_PASSWORD}"

CH_HTTP_URL="http://${CH_HOST}:${CH_PORT}"

echo
echo "=== ClickHouse Data Wipe (schema preserved, engine-aware) ==="
echo "DB: $DB_NAME"
echo "Host: $CH_HOST"
echo "Port: $CH_PORT"
echo

# ===== Core Logic =====
q(){ curl -fsS -u "$CH_USER:$CH_PASSWORD" --data-binary "$1" "$CH_HTTP_URL/?wait_end_of_query=1"; }

truncate_or_delete_all(){
  local fq="$1"
  if q "TRUNCATE TABLE $fq" >/dev/null 2>&1; then
    echo "   ✅ TRUNCATE $fq"
  elif q "ALTER TABLE $fq DELETE WHERE 1 SETTINGS mutations_sync=2" >/dev/null 2>&1; then
    echo "   ✅ DELETE WHERE 1 $fq"
  else
    echo "   ❌ Could not clear $fq"
  fi
}

list_tables_tsv(){
  q "
  SELECT
    name,
    engine,
    regexpExtract(create_table_query, '(?i)\\\\bTO\\\\s+([A-Za-z0-9_]+)\\\\.([A-Za-z0-9_]+)', 1) AS to_db,
    regexpExtract(create_table_query, '(?i)\\\\bTO\\\\s+([A-Za-z0-9_]+)\\\\.([A-Za-z0-9_]+)', 2) AS to_tbl
  FROM system.tables
  WHERE database = '$DB_NAME'
  FORMAT TabSeparated
  "
}

count_rows(){
  q "SELECT count() FROM $1 FORMAT TabSeparated" 2>/dev/null || echo "__ERR__"
}

echo "➡️  Discovering tables..."
mapfile -t LINES < <(list_tables_tsv)
declare -A CLEAR_SET=()
declare -A SKIP_NOTE=()

for line in "${LINES[@]}"; do
  IFS=$'\t' read -r name engine to_db to_tbl <<< "$line"
  fq="$DB_NAME.$name"
  case "$engine" in
    *MergeTree* ) CLEAR_SET["$fq"]=1 ;;
    MaterializedView )
      if [[ -n "$to_db" && -n "$to_tbl" ]]; then
        CLEAR_SET["$to_db.$to_tbl"]=1
        SKIP_NOTE["$fq"]="MV → clears target $to_db.$to_tbl"
      else
        SKIP_NOTE["$fq"]="MV without TO (no storage)"
      fi ;;
    View ) SKIP_NOTE["$fq"]="view (no storage)" ;;
    Distributed )
      CLEAR_SET["$fq"]=1
      SKIP_NOTE["$fq"]="Distributed (best effort)" ;;
    * ) CLEAR_SET["$fq"]=1 ;;
  esac
done

echo "Targets:"
for fq in "${!CLEAR_SET[@]}"; do echo "  - $fq"; done
echo
read -r -p "Remove ALL rows from these tables? [y/N]: " ok
[[ "$ok" =~ ^[Yy]$ ]] || { echo "Aborted."; exit 0; }

for fq in "${!CLEAR_SET[@]}"; do
  echo "➡️  Clearing $fq..."
  truncate_or_delete_all "$fq"
done

echo
echo "🧪 Verifying row counts:"
fail=0
for fq in "${!CLEAR_SET[@]}"; do
  c="$(count_rows "$fq")"
  if [[ "$c" == "__ERR__" ]]; then
    echo "   ❌ $fq : query error"
    fail=1
  elif [[ "$c" == "0" ]]; then
    echo "   ✅ $fq : 0 rows"
  else
    echo "   ⚠️ $fq : $c rows remain"
    fail=1
  fi
done

[[ $fail -eq 0 ]] && echo "🎯 All cleared." || echo "⚠️  Some tables still have data."
