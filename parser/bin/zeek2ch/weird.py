#!/usr/bin/env python3
r"""
Zeek weird.log -> ClickHouse net.weird ingester (HTTP / port 8123)

- Reads Zeek TSV weird.log (with #separator/#fields header)
- Skips comment/meta lines
- Converts:
  - ts epoch -> "YYYY-MM-DD HH:MM:SS.ffffff" UTC (DateTime64(6,'UTC'))
  - '-' (unset_field) -> NULL (\N)
  - '(empty)' (empty_field) -> '' (empty string)  [changeable]
  - notice T/F -> 1/0
  - IPv4 -> IPv6-mapped (::ffff:a.b.c.d) for Nullable(IPv6) columns
- Streams inserts to ClickHouse over HTTP in chunks (no clickhouse-client required)

Example:
  python3 weird.py --host localhost --port 8123 --user capstone --password boogle ../../zeek/zeek-logs/weird.log
"""

import argparse
import datetime as dt
import sys
from typing import Dict, List, Optional, Tuple

import requests

# Target ClickHouse columns (exclude materialized "day")
CH_COLS = [
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

# Map Zeek field -> ClickHouse column
ZEEK_TO_CH = {
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
    "identifier": "identifier",  # not always present in weird.log
}


def decode_zeek_separator(token: str) -> str:
    """
    Zeek logs include: '#separator \\x09'
    Return the actual separator character.
    """
    token = token.strip()
    if token.startswith("\\x") and len(token) == 4:
        return chr(int(token[2:], 16))
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


def zeek_bool_to_u8(v: str, unset: str) -> str:
    if v == unset:
        return "\\N"
    if v == "T":
        return "1"
    if v == "F":
        return "0"
    vl = v.lower()
    if vl == "true":
        return "1"
    if vl == "false":
        return "0"
    raise ValueError(f"Unexpected bool value: {v!r}")


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


def normalize_value(ch_col: str, raw: str, unset: str, empty: str) -> str:
    """
    Map raw Zeek value -> ClickHouse TSV cell for the target column.
    Returns '\\N' for NULL or the final (escaped) string/number.
    """
    if raw == unset:
        return "\\N"

    if raw == empty:
        # Choose behavior:
        # - empty string (current)
        # - or NULL: return "\\N"
        return ""

    if ch_col == "ts":
        return ts_epoch_to_ch_datetime64(raw)

    if ch_col in ("orig_p", "resp_p"):
        return str(int(raw))

    if ch_col in ("orig_h", "resp_h"):
        return ipv4_to_ipv6_mapped(raw)

    if ch_col == "notice":
        return zeek_bool_to_u8(raw, unset)

    return ch_tsv_escape(raw)


def parse_zeek_header(lines_iter) -> Tuple[str, str, str, List[str]]:
    """
    Read header lines until #fields and return:
      separator, unset_field, empty_field, fields_list
    """
    sep = "\t"
    unset = "-"
    empty = "(empty)"
    fields: Optional[List[str]] = None

    for line in lines_iter:
        if not line.startswith("#"):
            raise RuntimeError("Unexpected: data line before #fields header")

        if line.startswith("#separator"):
            parts = line.split(None, 1)
            if len(parts) == 2:
                sep = decode_zeek_separator(parts[1].strip())

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

        # ignore other header lines

    if fields is None:
        raise RuntimeError("Could not find #fields header in file.")

    return sep, unset, empty, fields


def build_row(fields: List[str], values: List[str], unset: str, empty: str) -> str:
    """
    Build one ClickHouse TSV row matching CH_COLS order.
    """
    if len(values) != len(fields):
        raise ValueError(
            f"Field/value count mismatch: {len(fields)} fields vs {len(values)} values"
        )

    zeek_map: Dict[str, str] = dict(zip(fields, values))

    out_cells: List[str] = []
    for ch_col in CH_COLS:
        # find Zeek field name that maps to this ClickHouse column
        zeek_field = None
        for zf, cc in ZEEK_TO_CH.items():
            if cc == ch_col:
                zeek_field = zf
                break

        raw = zeek_map.get(zeek_field, unset) if zeek_field else unset
        cell = normalize_value(ch_col, raw, unset, empty)

        out_cells.append(cell)

    return "\t".join(out_cells)


def insert_rows_http(
    host: str,
    port: int,
    user: str,
    password: Optional[str],
    database: str,
    table: str,
    rows_tsv: str,
    timeout_s: int = 60,
) -> None:
    """
    Insert TSV rows into ClickHouse using HTTP interface.
    """
    query = f"INSERT INTO {database}.{table} ({', '.join(CH_COLS)}) FORMAT TabSeparated"
    url = f"http://{host}:{port}/"
    params = {"query": query}

    # ClickHouse HTTP basic auth
    auth = (user, password or "")

    r = requests.post(
        url,
        params=params,
        data=rows_tsv.encode("utf-8"),
        auth=auth,
        timeout=timeout_s,
    )

    # If it fails, include response body for debugging
    if not r.ok:
        raise RuntimeError(f"ClickHouse HTTP insert failed: {r.status_code}\n{r.text}")

    r.raise_for_status()


def main() -> None:
    ap = argparse.ArgumentParser(
        description="Ingest Zeek weird.log into ClickHouse net.weird (HTTP)."
    )
    ap.add_argument("weird_log", help="Path to weird.log (Zeek TSV)")
    ap.add_argument("--host", default="localhost")
    ap.add_argument("--port", type=int, default=8123)
    ap.add_argument("--user", default="default")
    ap.add_argument("--password", default=None)
    ap.add_argument("--database", default="net")
    ap.add_argument("--table", default="weird")
    ap.add_argument(
        "--flush-bytes",
        type=int,
        default=4 * 1024 * 1024,
        help="Flush insert buffer after N bytes (default 4MB)",
    )
    args = ap.parse_args()

    buf: List[str] = []
    buf_bytes = 0
    total_rows = 0

    with open(args.weird_log, "r", encoding="utf-8", errors="replace") as f:
        lines = iter(f)

        sep, unset, empty, fields = parse_zeek_header(lines)

        for line in lines:
            if not line:
                continue
            if line.startswith("#"):
                continue

            line = line.rstrip("\n")
            if line == "":
                continue

            parts = line.split(sep)
            row = build_row(fields, parts, unset=unset, empty=empty)
            out_line = row + "\n"

            buf.append(out_line)
            buf_bytes += len(out_line.encode("utf-8"))
            total_rows += 1

            if buf_bytes >= args.flush_bytes:
                insert_rows_http(
                    host=args.host,
                    port=args.port,
                    user=args.user,
                    password=args.password,
                    database=args.database,
                    table=args.table,
                    rows_tsv="".join(buf),
                )
                buf.clear()
                buf_bytes = 0

    # final flush
    if buf:
        insert_rows_http(
            host=args.host,
            port=args.port,
            user=args.user,
            password=args.password,
            database=args.database,
            table=args.table,
            rows_tsv="".join(buf),
        )

    print(f"Inserted {total_rows} rows into {args.database}.{args.table}")


if __name__ == "__main__":
    try:
        main()
    except KeyboardInterrupt:
        print("Interrupted.", file=sys.stderr)
        sys.exit(130)
