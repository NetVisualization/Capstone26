use anyhow::Result;
use clickhouse::Client;

use crate::db::{DbPacket, l4_label_from_code, l7_label_from_code, mac_to_hex, ts_to_str};
use crate::util::escape_sql_single_quotes;

pub async fn run_file_and_insert(
    client: &Client,
    table: &str,
    rows: Vec<DbPacket>,
    batch_size: usize,
) -> Result<()> {
    if rows.is_empty() {
        tracing::info!("no rows parsed");
        return Ok(());
    }

    let mut written = 0usize;
    let mut i = 0usize;
    while i < rows.len() {
        let end = (i + batch_size).min(rows.len());
        let slice = &rows[i..end];

        let mut sql = String::with_capacity(slice.len() * 256);
        sql.push_str("INSERT INTO ");
        sql.push_str(table);
        sql.push_str(" (ts, src_ip, dst_ip, src_mac, dst_mac, l4_proto, l7_proto, src_port, dst_port, packet_len, raw) VALUES ");

        for (j, row) in slice.iter().enumerate() {
            let ts = ts_to_str(row.ts);
            let src_ip = row.src_ip.to_string();
            let dst_ip = row.dst_ip.to_string();
            let src_mac_hex = mac_to_hex(&row.src_mac);
            let dst_mac_hex = mac_to_hex(&row.dst_mac);
            let l4 = l4_label_from_code(row.l4_proto);
            let l7 = l7_label_from_code(row.l7_proto);

            let src_port_lit = row
                .src_port
                .map(|v| v.to_string())
                .unwrap_or_else(|| "NULL".to_string());
            let dst_port_lit = row
                .dst_port
                .map(|v| v.to_string())
                .unwrap_or_else(|| "NULL".to_string());

            let raw_sql = escape_sql_single_quotes(&row.raw);

            if j > 0 {
                sql.push_str(", ");
            }
            use std::fmt::Write as _;
            write!(
                &mut sql,
                "(toDateTime64('{ts}', 6, 'UTC'), \
                  toIPv6('{src_ip}'), toIPv6('{dst_ip}'), \
                  CAST(unhex('{src_mac_hex}') AS FixedString(6)), \
                  CAST(unhex('{dst_mac_hex}') AS FixedString(6)), \
                  '{l4}', '{l7}', {src_port_lit}, {dst_port_lit}, {packet_len}, '{raw_sql}')",
                packet_len = row.packet_len
            )
            .unwrap();
        }

        client.query(&sql).execute().await?;
        written += slice.len();
        i = end;
    }

    tracing::info!(%written, table, "inserted rows into clickhouse (text/VALUES mode)");
    Ok(())
}
