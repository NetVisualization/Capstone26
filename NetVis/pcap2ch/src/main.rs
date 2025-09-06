use anyhow::{Context, Result};
use chrono::{DateTime, TimeZone, Utc};
use etherparse::{NetHeaders, PacketHeaders, TransportHeader};
use hex;
use pcap::{Capture, Offline};
use reqwest::Client;
use std::env;
use std::net::{Ipv4Addr, Ipv6Addr};
use std::time::Duration;
use urlencoding;

mod ch_types {
    pub const DST_IP: &str = "IPv6";
    pub const SRC_IP: &str = "IPv6";
    pub const DST_MAC: &str = "FixedString(6)";
    pub const SRC_MAC: &str = "FixedString(6)";
    pub const DST_PORT: &str = "Nullable(UInt16)";
    pub const SRC_PORT: &str = "Nullable(UInt16)";
    pub const TCP_FLAGS: &str = "Nullable(UInt16)";
    pub const TS: &str = "DateTime64(6, 'UTC')";

    pub fn create_table_sql(table: &str) -> String {
        format!(
            r#"
CREATE TABLE IF NOT EXISTS {table} (
  dst_ip    {DST_IP},
  dst_mac   {DST_MAC},
  dst_port  {DST_PORT},
  src_ip    {SRC_IP},
  src_mac   {SRC_MAC},
  src_port  {SRC_PORT},
  tcp_flags {TCP_FLAGS},
  ts        {TS}
) ENGINE = MergeTree
ORDER BY (ts, src_ip, dst_ip)
SETTINGS index_granularity = 8192"#,
            table = table,
            DST_IP = DST_IP,
            SRC_IP = SRC_IP,
            DST_MAC = DST_MAC,
            SRC_MAC = SRC_MAC,
            DST_PORT = DST_PORT,
            SRC_PORT = SRC_PORT,
            TCP_FLAGS = TCP_FLAGS,
            TS = TS
        )
    }
}

#[derive(Debug)]
struct Row {
    dst_ip_s: String, // textual IP (IPv4 dotted or IPv6)
    dst_mac_hex: String,
    dst_port: Option<u16>,
    src_ip_s: String, // textual IP (IPv4 dotted or IPv6)
    src_mac_hex: String,
    src_port: Option<u16>,
    tcp_flags: Option<u16>,
    ts: chrono::DateTime<chrono::Utc>,
}

#[tokio::main]
async fn main() -> Result<()> {
    let args: Vec<String> = env::args().collect();
    if args.len() < 4 {
        eprintln!(
            "Usage: {} <pcap_path> <clickhouse_url> <table>\n\
             Example URL: http://localhost:8123?database=netcap",
            args.get(0).unwrap_or(&"pcap2ch".to_string())
        );
        std::process::exit(2);
    }
    let pcap_path = &args[1];
    let ch_url = &args[2];
    let table = &args[3];

    let http = Client::builder()
        .timeout(Duration::from_secs(120))
        .build()?;

    exec_sql(&http, ch_url, &ch_types::create_table_sql(table)).await?;

    let mut cap: Capture<Offline> =
        Capture::from_file(pcap_path).with_context(|| format!("open {}", pcap_path))?;

    const BATCH_SIZE: usize = 10_000;
    let mut batch: Vec<Row> = Vec::with_capacity(BATCH_SIZE);
    let mut inserted_total = 0usize;

    while let Ok(pkt) = cap.next_packet() {
        let ts = pcap_ts_to_utc(pkt.header.ts.tv_sec as i64, pkt.header.ts.tv_usec as i64);
        if pkt.data.len() < 14 {
            continue;
        }

        // L2 MACs

        let dst_mac_hex = hex::encode(&pkt.data[0..6]);
        let src_mac_hex = hex::encode(&pkt.data[6..12]);
        let ethertype = u16::from_be_bytes([pkt.data[12], pkt.data[13]]);

        let mut src_ip = Ipv6Addr::UNSPECIFIED;
        let mut dst_ip = Ipv6Addr::UNSPECIFIED;
        let mut src_port: Option<u16> = None;
        let mut dst_port: Option<u16> = None;
        let mut tcp_flags: Option<u16> = None;

        let mut src_ip_s = String::from("::"); // safe default
        let mut dst_ip_s = String::from("::");

        if let Some(net) = headers.net {
            match net {
                NetHeaders::Ipv4(h, _) => {
                    src_ip_s = std::net::Ipv4Addr::from(h.source).to_string(); // "a.b.c.d"
                    dst_ip_s = std::net::Ipv4Addr::from(h.destination).to_string();
                }
                NetHeaders::Ipv6(h, _) => {
                    src_ip_s = std::net::Ipv6Addr::from(h.source).to_string(); // "2001:db8::1"
                    dst_ip_s = std::net::Ipv6Addr::from(h.destination).to_string();
                }
                _ => {}
            }
        } else if ethertype == 0x0800 && pkt.data.len() >= 34 {
            // minimal IPv4 fallback
            let s = std::net::Ipv4Addr::new(pkt.data[26], pkt.data[27], pkt.data[28], pkt.data[29]);
            let d = std::net::Ipv4Addr::new(pkt.data[30], pkt.data[31], pkt.data[32], pkt.data[33]);
            src_ip_s = s.to_string();
            dst_ip_s = d.to_string();
        }

        // when pushing the row:
        batch.push(Row {
            dst_ip_s,
            dst_mac_hex,
            dst_port,
            src_ip_s,
            src_mac_hex,
            src_port,
            tcp_flags,
            ts,
        });

        if batch.len() >= BATCH_SIZE {
            inserted_total += flush_batch(&http, ch_url, table, &mut batch).await?;
        }
    }

    if !batch.is_empty() {
        inserted_total += flush_batch(&http, ch_url, table, &mut batch).await?;
    }

    eprintln!("Inserted {} rows.", inserted_total);
    Ok(())
}

fn pcap_ts_to_utc(sec: i64, usec: i64) -> DateTime<Utc> {
    let nsec = usec * 1000;
    Utc.timestamp_opt(sec, nsec as u32)
        .single()
        .unwrap_or_else(|| Utc.timestamp_nanos(0))
}

fn tcp_flags_to_mask(t: &etherparse::TcpHeader) -> u16 {
    let mut f = 0u16;
    if t.fin {
        f |= 0x01;
    }
    if t.syn {
        f |= 0x02;
    }
    if t.rst {
        f |= 0x04;
    }
    if t.psh {
        f |= 0x08;
    }
    if t.ack {
        f |= 0x10;
    }
    if t.urg {
        f |= 0x20;
    }
    if t.ece {
        f |= 0x40;
    }
    if t.cwr {
        f |= 0x80;
    }
    f
}

async fn exec_sql(http: &reqwest::Client, base_url: &str, sql: &str) -> anyhow::Result<()> {
    // POST the SQL in the request body so it’s not read-only and has a Content-Length
    let res = http
        .post(base_url) // keep ?database=...&user=...&password=... in base_url
        .header("Content-Type", "text/plain; charset=utf-8")
        .body(sql.to_owned())
        .send()
        .await?;
    if !res.status().is_success() {
        let status = res.status();
        let body = res.text().await.unwrap_or_default();
        anyhow::bail!("ClickHouse error: {} - {}", status, body);
    }
    Ok(())
}

async fn flush_batch(
    http: &Client,
    base_url: &str,
    table: &str,
    batch: &mut Vec<Row>,
) -> Result<usize> {
    if batch.is_empty() {
        return Ok(0);
    }

    let insert_sql = format!(
        r#"
    INSERT INTO {table}
        (dst_ip, dst_mac, dst_port, src_ip, src_mac, src_port, tcp_flags, ts)
    SELECT
      dst_ip,
      unhex(dst_mac_hex) AS dst_mac,
      dst_port,
      src_ip,
      unhex(src_mac_hex) AS src_mac,
      src_port,
      tcp_flags,
      ts
    FROM input(
      'dst_ip IPv6, dst_mac_hex String, dst_port Nullable(UInt16), \
       src_ip IPv6, src_mac_hex String, src_port Nullable(UInt16), \
       tcp_flags Nullable(UInt16), ts DateTime64(6, \'UTC\')'
    ) FORMAT TabSeparated
    "#,
        table = table
    );

    let mut payload = String::with_capacity(batch.len() * 120);
    for r in batch.iter() {
        let dst_port = r
            .dst_port
            .map(|v| v.to_string())
            .unwrap_or_else(|| "\\N".into());
        let src_port = r
            .src_port
            .map(|v| v.to_string())
            .unwrap_or_else(|| "\\N".into());
        let tcp_flags = r
            .tcp_flags
            .map(|v| v.to_string())
            .unwrap_or_else(|| "\\N".into());
        let ts = r.ts.format("%Y-%m-%d %H:%M:%S%.6f").to_string();

        use std::fmt::Write as _;
        let _ = write!(
            payload,
            "{}\t{}\t{}\t{}\t{}\t{}\t{}\t{}\n",
            r.dst_ip, r.dst_mac_hex, dst_port, r.src_ip, r.src_mac_hex, src_port, tcp_flags, ts
        );
    }

    let url = format!("{}&query={}", base_url, urlencoding::encode(&insert_sql));
    let resp = http.post(&url).body(payload).send().await?;
    if !resp.status().is_success() {
        let status = resp.status(); // capture before consuming
        let body = resp.text().await.unwrap_or_default(); // consumes resp
        anyhow::bail!("ClickHouse insert error: {} - {}", status, body);
    }

    let n = batch.len();
    batch.clear();
    Ok(n)
}
