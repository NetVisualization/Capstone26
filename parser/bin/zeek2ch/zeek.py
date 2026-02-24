#!/usr/bin/env python3
r"""
Zeek weird.log & notice.log -> ClickHouse net.weird / net.notice ingester (HTTP / port 8123)

- Reads Zeek TSV logs (with #separator/#fields header)
- Supports:
    * weird.log  -> net.weird
    * notice.log -> net.notice
- Skips comment/meta lines
- Converts:
    * ts epoch -> "YYYY-MM-DD HH:MM:SS.ffffff" UTC (DateTime64(6,'UTC'))
    * '-' (unset_field)       -> NULL (\N) for Nullable columns
    * '(empty)' (empty_field) -> "" (empty string) for string columns
    * weird.notice T/F        -> 1/0
    * IPv4 -> IPv6-mapped (::ffff:a.b.c.d) for Nullable(IPv6) columns
    * notice.actions set[enum] -> Array(Enum8) e.g. ['Notice::ACTION_LOG', ...]

Example:
    python3 zeek.py --host localhost --port 8123 --user capstone --password boogle \
        --weird ../../zeek/zeek-logs/weird.log \
        --notice ../../zeek/zeek-logs/notice.log
"""

import argparse
import datetime as dt
import re
import sys
from typing import Dict, List, Optional, Tuple

import requests

# --- ClickHouse column layouts (excluding materialized "day") -----------------

WEIRD_COLS = [
    "ts",
    "uid",
    "orig_h",
    "orig_p",
    "resp_h",
    "resp_p",
    "name",
    "addl",
    "notice",
    "peer",
    "source",
    "identifier",
]

NOTICE_COLS = [
    "ts",
    "uid",
    "id_orig_h",
    "id_resp_h",
    "note",
    "msg",
    "sub",
    "src",
    "actions",
]

# Map Zeek field -> ClickHouse column for each log type
WEIRD_ZEEK_TO_CH = {
    "ts": "ts",
    "uid": "uid",
    "id.orig_h": "orig_h",
    "id.orig_p": "orig_p",
    "id.resp_h": "resp_h",
    "id.resp_p": "resp_p",
    "name": "name",
    "addl": "addl",
    "notice": "notice",
    "peer": "peer",
    "source": "source",
    "identifier": "identifier",
}

NOTICE_ZEEK_TO_CH = {
    "ts": "ts",
    "uid": "uid",
    "id.orig_h": "id_orig_h",
    "id.resp_h": "id_resp_h",
    "note": "note",
    "msg": "msg",
    "sub": "sub",
    "src": "src",
    "actions": "actions",
}


# --- Zeek / ClickHouse helpers ------------------------------------------------


def decode_zeek_separator(token: str) -> str:
    """
    Zeek logs include: '#separator \\x09'
    Return the actual separator character.
    """
    token = token.strip()
    if token.startswith(r"\x") and len(token) == 4:
        try:
            return bytes([int(token[2:], 16)]).decode("ascii")
        except Exception:
            return "\t"
    return token


def ch_tsv_escape(s: str) -> str:
    """
    Escape special chars for ClickHouse TabSeparated format.
    """
    return (
        s.replace("\\", "\\\\")
        .replace("\t", "\\t")
        .replace("\n", "\\n")
        .replace("\r", "\\r")
    )


def ipv4_to_ipv6_mapped(ip: str) -> str:
    """
    Store IPv4 text into IPv6 column as IPv4-mapped address.
    If already IPv6, return as-is.
    """
    if ":" in ip:
        return ip
    return f"::ffff:{ip}"


def ts_epoch_to_ch_datetime64(ts: str) -> str:
    """
    Zeek ts is seconds since epoch with fractional part.
    Convert to 'YYYY-MM-DD HH:MM:SS.ffffff' in UTC.
    """
    sec = float(ts)
    whole = int(sec)
    frac = sec - whole
    micros = int(round(frac * 1_000_000))

    base = dt.datetime.fromtimestamp(whole, tz=dt.timezone.utc)
    base += dt.timedelta(microseconds=micros)
    return base.strftime("%Y-%m-%d %H:%M:%S.%f")


def parse_zeek_header(lines_iter) -> Tuple[str, str, str, str, List[str]]:
    """
    Read header lines until #fields and return:
      separator, unset_field, empty_field, set_separator, fields_list
    """
    sep = "\t"
    unset = "-"
    empty = "(empty)"
    set_sep = ","
    fields: Optional[List[str]] = None

    for line in lines_iter:
        if not line.startswith("#"):
            raise RuntimeError("Unexpected: data line before #fields header")

        if line.startswith("#separator"):
            parts = line.split(None, 1)
            if len(parts) == 2:
                sep = decode_zeek_separator(parts[1].strip())

        elif line.startswith("#set_separator"):
            parts = line.split(None, 1)
            if len(parts) == 2:
                set_sep = parts[1].strip()

        elif line.startswith("#unset_field"):
            parts = line.split(None, 1)
            if len(parts) == 2:
                unset = parts[1].strip()

        elif line.startswith("#empty_field"):
            parts = line.split(None, 1)
            if len(parts) == 2:
                empty = parts[1].strip()

        elif line.startswith("#fields"):
            parts = line.strip().split()
            fields = parts[1:]
            break

        # ignore other header lines (#path, #open, #types, ...)

    if fields is None:
        raise RuntimeError("Could not find #fields header in file.")

    return sep, unset, empty, set_sep, fields


# --- Normalization per column / log type -------------------------------------


def normalize_weird_value(
    ch_col: str,
    raw: str,
    unset: str,
    empty: str,
    set_sep: str,  # unused but kept for signature symmetry
) -> str:
    """
    Map raw Zeek value -> ClickHouse TSV cell for net.weird.
    Returns '\\N' for NULL or the final (escaped) string/number.
    """
    # unset -> NULL for Nullable columns
    if raw == unset:
        return "\\N"

    if raw == empty:
        # treat Zeek '(empty)' as empty string for string columns
        if ch_col in ("uid", "name", "addl", "peer", "source", "identifier"):
            return ""
        # for non-string columns, treat as NULL
        return "\\N"

    if ch_col == "ts":
        return ts_epoch_to_ch_datetime64(raw)

    # Ports as UInt16
    if ch_col in ("orig_p", "resp_p"):
        return str(int(raw))

    # IP address columns as IPv6/IPv4-mapped
    if ch_col in ("orig_h", "resp_h"):
        return ipv4_to_ipv6_mapped(raw)

    # notice: T/F -> 1/0
    if ch_col == "notice":
        v = raw.strip()
        if v in ("T", "1", "true", "True"):
            return "1"
        if v in ("F", "0", "false", "False"):
            return "0"
        # fall back to 0 if weird value
        return "0"

    # all remaining as escaped strings
    return ch_tsv_escape(raw)


def normalize_actions_array(raw: str, unset: str, empty: str, set_sep: str) -> str:
    """
    Convert Zeek set[enum] for actions into ClickHouse Array(Enum8) text:
    Zeek: "Notice::ACTION_LOG,Notice::ACTION_EMAIL"
    CH:   "['Notice::ACTION_LOG','Notice::ACTION_EMAIL']"
    """
    if raw == unset or raw == empty or raw == "":
        return "[]"

    elems = [e.strip() for e in raw.split(set_sep) if e.strip()]

    if not elems:
        return "[]"

    items: List[str] = []
    for e in elems:
        if e in (unset, empty):
            continue
        esc = e.replace("\\", "\\\\").replace("'", "\\'")
        items.append(f"'{esc}'")

    return "[" + ",".join(items) + "]"


def normalize_notice_value(
    ch_col: str,
    raw: str,
    unset: str,
    empty: str,
    set_sep: str,
) -> str:
    """
    Map raw Zeek value -> ClickHouse TSV cell for net.notice.
    """
    if ch_col == "actions":
        return normalize_actions_array(raw, unset, empty, set_sep)

    # unset -> NULL for Nullable scalar columns
    if raw == unset:
        return "\\N"

    if raw == empty:
        # treat Zeek '(empty)' as empty string for strings, NULL otherwise
        if ch_col in ("uid", "note", "msg", "sub"):
            return ""
        return "\\N"

    if ch_col == "ts":
        return ts_epoch_to_ch_datetime64(raw)

    if ch_col in ("id_orig_h", "id_resp_h", "src"):
        return ipv4_to_ipv6_mapped(raw)

    # everything else is string-ish (note enum, msg, sub) -> escape
    return ch_tsv_escape(raw)


# --- Row builders -------------------------------------------------------------


def build_row(
    log_type: str,
    fields: List[str],
    values: List[str],
    unset: str,
    empty: str,
    set_sep: str,
) -> str:
    """
    Build one ClickHouse TSV row matching the appropriate column order.
    """
    if len(values) != len(fields):
        raise ValueError(
            f"Field/value count mismatch: {len(fields)} fields vs {len(values)} values"
        )

    zeek_map: Dict[str, str] = dict(zip(fields, values))

    if log_type == "weird":
        cols = WEIRD_COLS
        mapping = WEIRD_ZEEK_TO_CH
        normalizer = normalize_weird_value
    elif log_type == "notice":
        cols = NOTICE_COLS
        mapping = NOTICE_ZEEK_TO_CH
        normalizer = normalize_notice_value
    else:
        raise ValueError(f"Unknown log_type: {log_type}")

    out_cells: List[str] = []

    for ch_col in cols:
        # find Zeek field name that maps to this ClickHouse column
        zeek_field = None
        for zf, cc in mapping.items():
            if cc == ch_col:
                zeek_field = zf
                break

        raw = zeek_map.get(zeek_field, unset) if zeek_field else unset
        cell = normalizer(ch_col, raw, unset, empty, set_sep)
        out_cells.append(cell)

    return "\t".join(out_cells)


# --- ClickHouse insert helper -------------------------------------------------


def insert_rows_http(
    host: str,
    port: int,
    user: str,
    password: Optional[str],
    database: str,
    table: str,
    cols: List[str],
    rows_tsv: str,
    timeout_s: int = 60,
) -> None:
    """
    Insert TSV rows into ClickHouse using HTTP interface.
    """
    col_list = ", ".join(cols)
    query = f"INSERT INTO {database}.{table} ({col_list}) FORMAT TabSeparated"
    url = f"http://{host}:{port}/"
    params = {"query": query}
    auth = (user, password or "")

    r = requests.post(
        url,
        params=params,
        data=rows_tsv.encode("utf-8"),
        auth=auth,
        timeout=timeout_s,
    )

    if not r.ok:
        raise RuntimeError(f"ClickHouse HTTP insert failed: {r.status_code} {r.text}")


# --- High level ingestion -----------------------------------------------------


def ingest_file(
    log_type: str,
    path: str,
    host: str,
    port: int,
    user: str,
    password: Optional[str],
    database: str,
    table: str,
    flush_bytes: int,
) -> int:
    """
    Ingest a single Zeek log file of a given type into ClickHouse.
    Returns the number of inserted rows.
    """
    if log_type == "weird":
        cols = WEIRD_COLS
    elif log_type == "notice":
        cols = NOTICE_COLS
    else:
        raise ValueError(f"Unknown log_type: {log_type}")

    buf: List[str] = []
    buf_bytes = 0
    total_rows = 0

    with open(path, "r", encoding="utf-8", errors="replace") as f:
        lines = iter(f)
        sep, unset, empty, set_sep, fields = parse_zeek_header(lines)

        for line in lines:
            if not line:
                continue
            if line.startswith("#"):
                continue

            line = line.rstrip("\n")
            if line == "":
                continue

            # --- PRIMARY SPLIT: use Zeek's declared separator ---
            parts = line.split(sep)

            # If that clearly failed (like in your notice.log),
            # fall back to splitting on runs of 2+ whitespace chars.
            if len(parts) != len(fields):
                parts = re.split(r"\s{2,}", line)

            if len(parts) != len(fields):
                # Still broken? Then bail with a clear error.
                raise ValueError(
                    f"[{log_type}] Field/value count mismatch even after fallback: "
                    f"{len(fields)} fields vs {len(parts)} values\n"
                    f"Line: {line!r}"
                )

            row = build_row(
                log_type,
                fields,
                parts,
                unset=unset,
                empty=empty,
                set_sep=set_sep,
            )
            out_line = row + "\n"

            buf.append(out_line)
            buf_bytes += len(out_line.encode("utf-8"))
            total_rows += 1

            if buf_bytes >= flush_bytes:
                insert_rows_http(
                    host=host,
                    port=port,
                    user=user,
                    password=password,
                    database=database,
                    table=table,
                    cols=cols,
                    rows_tsv="".join(buf),
                )
                buf.clear()
                buf_bytes = 0

    if buf:
        insert_rows_http(
            host=host,
            port=port,
            user=user,
            password=password,
            database=database,
            table=table,
            cols=cols,
            rows_tsv="".join(buf),
        )

    return total_rows


# --- CLI ----------------------------------------------------------------------


def main() -> None:
    ap = argparse.ArgumentParser(
        description="Ingest Zeek weird.log and notice.log into ClickHouse (net.weird / net.notice)."
    )
    ap.add_argument(
        "--weird",
        action="append",
        default=[],
        help="Path to weird.log (can be used multiple times).",
    )
    ap.add_argument(
        "--notice",
        action="append",
        default=[],
        help="Path to notice.log (can be used multiple times).",
    )
    ap.add_argument("--host", default="localhost")
    ap.add_argument("--port", type=int, default=8123)
    ap.add_argument("--user", default="default")
    ap.add_argument("--password", default=None)
    ap.add_argument("--database", default="net")
    ap.add_argument("--weird-table", default="weird")
    ap.add_argument("--notice-table", default="notice")
    ap.add_argument(
        "--flush-bytes",
        type=int,
        default=4 * 1024 * 1024,
        help="Flush insert buffer after N bytes (default 4MB).",
    )
    args = ap.parse_args()

    if not args.weird and not args.notice:
        ap.error("Must provide at least one --weird or --notice path.")

    grand_total = 0

    for path in args.weird:
        print(f"[weird] Ingesting {path} -> {args.database}.{args.weird_table} ...")
        n = ingest_file(
            log_type="weird",
            path=path,
            host=args.host,
            port=args.port,
            user=args.user,
            password=args.password,
            database=args.database,
            table=args.weird_table,
            flush_bytes=args.flush_bytes,
        )
        print(f"[weird] Inserted {n} rows from {path}")
        grand_total += n

    for path in args.notice:
        print(f"[notice] Ingesting {path} -> {args.database}.{args.notice_table} ...")
        n = ingest_file(
            log_type="notice",
            path=path,
            host=args.host,
            port=args.port,
            user=args.user,
            password=args.password,
            database=args.database,
            table=args.notice_table,
            flush_bytes=args.flush_bytes,
        )
        print(f"[notice] Inserted {n} rows from {path}")
        grand_total += n

    print(f"Done. Total rows inserted: {grand_total}")


if __name__ == "__main__":
    try:
        main()
    except KeyboardInterrupt:
        print("Interrupted.", file=sys.stderr)
        sys.exit(130)
