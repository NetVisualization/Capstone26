use anyhow::{Context, Result};
use chrono::{DateTime, TimeZone, Utc};
use clap::Parser;
use clickhouse; // uses the HTTP client
use etherparse::{InternetSlice, LinkSlice, SlicedPacket, TransportSlice};
use pcap::Capture;
use std::fmt::Write as _;
use std::net::Ipv6Addr;

#[derive(Debug, Parser)]
#[command(
    name = "pcap2ch",
    about = "pcap -> ClickHouse ingestor for net.packets"
)]
struct Args {
    /// ClickHouse HTTP(S) URL, e.g. http://host:8123 or https://host:8443
    #[arg(long, env = "CH_DSN")]
    dsn: String,
    /// Database (must contain the target table)
    #[arg(long, default_value = "net")]
    database: String,
    /// Target table name (in the database above)
    #[arg(long, default_value = "packets")]
    table: String,
    /// Batch size (rows per INSERT)
    #[arg(long, default_value_t = 5000)]
    batch: usize,
    /// Optional username/password (if not embedded in DSN)
    #[arg(long, env = "CH_USER")]
    user: Option<String>,
    #[arg(long, env = "CH_PASSWORD")]
    password: Option<String>,
    /// One or more PCAP/PCAPNG files to ingest
    files: Vec<String>,
}

#[derive(Clone)]
struct RowBuf {
    ts: DateTime<Utc>,
    src_ip: Ipv6Addr,
    dst_ip: Ipv6Addr,
    src_mac: [u8; 6],
    dst_mac: [u8; 6],
    l4_proto: i16,
    src_port: Option<u16>,
    dst_port: Option<u16>,
    packet_len: u32,
}

#[tokio::main]
async fn main() -> Result<()> {
    let args = Args::parse();
    if args.files.is_empty() {
        anyhow::bail!("Provide at least one pcap: pcap2ch --dsn http://host:8123 ./capture.pcap");
    }

    // Build CH client
    let mut client = clickhouse::Client::default()
        .with_url(&args.dsn)
        .with_database(&args.database);
    if let Some(u) = &args.user {
        client = client.with_user(u);
    }
    if let Some(p) = &args.password {
        client = client.with_password(p);
    }

    // Preflight: ensure table exists
    let exists_sql = format!("EXISTS TABLE {}.{}", args.database, args.table);
    let exists: u8 = client
        .query(&exists_sql)
        .fetch_one()
        .await
        .with_context(|| format!("Preflight failed: {}", exists_sql))?;
    if exists == 0 {
        anyhow::bail!("Table does not exist: {}.{}", args.database, args.table);
    }

    let mut buf: Vec<RowBuf> = Vec::with_capacity(args.batch);
    let mut total: usize = 0;

    for path in &args.files {
        ingest_file(path, &mut buf).with_context(|| format!("ingesting {}", path))?;

        // flush in batches
        while buf.len() >= args.batch {
            let batch: Vec<RowBuf> = buf.drain(..args.batch).collect();
            insert_batch(&client, &args.database, &args.table, &batch).await?;
            total += batch.len();
        }
    }

    if !buf.is_empty() {
        insert_batch(&client, &args.database, &args.table, &buf).await?;
        total += buf.len();
    }

    eprintln!(
        "Inserted {} rows into {}.{}",
        total, args.database, args.table
    );
    Ok(())
}

fn ingest_file(path: &str, out: &mut Vec<RowBuf>) -> Result<()> {
    let mut cap = Capture::from_file(path).with_context(|| format!("open pcap {}", path))?;

    while let Ok(pkt) = cap.next_packet() {
        let ts = Utc
            .timestamp_opt(
                pkt.header.ts.tv_sec.into(),
                (pkt.header.ts.tv_usec as u32) * 1000,
            )
            .single()
            .unwrap_or_else(|| Utc.timestamp_opt(0, 0).unwrap());

        let data = pkt.data;
        let packet_len = pkt.header.len;

        let sp = match SlicedPacket::from_ethernet(data) {
            Ok(s) => s,
            Err(_) => continue,
        };

        // defaults
        let mut src_ip = Ipv6Addr::UNSPECIFIED;
        let mut dst_ip = Ipv6Addr::UNSPECIFIED;
        let mut src_mac = [0u8; 6];
        let mut dst_mac = [0u8; 6];
        let mut src_port: Option<u16> = None;
        let mut dst_port: Option<u16> = None;
        let mut l4_code: i16 = 0;

        if let Some(link) = &sp.link {
            if let LinkSlice::Ethernet2(eth) = link {
                src_mac.copy_from_slice(&eth.source());
                dst_mac.copy_from_slice(&eth.destination());
            }
        }

        if let Some(net) = &sp.net {
            match net {
                InternetSlice::Ipv4(v4) => {
                    src_ip = v4.header().source_addr().to_ipv6_mapped();
                    dst_ip = v4.header().destination_addr().to_ipv6_mapped();
                    l4_code =
                        map_l4_code(sp.transport.as_ref(), Some(v4.header().protocol().into()));
                }
                InternetSlice::Ipv6(v6) => {
                    src_ip = v6.header().source_addr();
                    dst_ip = v6.header().destination_addr();
                    l4_code = map_l4_code(
                        sp.transport.as_ref(),
                        Some(v6.header().next_header().into()),
                    );
                }
                _ => {}
            }
        }

        match &sp.transport {
            Some(TransportSlice::Tcp(t)) => {
                src_port = Some(t.source_port());
                dst_port = Some(t.destination_port());
            }
            Some(TransportSlice::Udp(u)) => {
                src_port = Some(u.source_port());
                dst_port = Some(u.destination_port());
            }
            _ => {}
        }

        out.push(RowBuf {
            ts,
            src_ip,
            dst_ip,
            src_mac,
            dst_mac,
            l4_proto: l4_code,
            src_port,
            dst_port,
            packet_len: packet_len as u32,
        });
    }

    Ok(())
}

async fn insert_batch(
    client: &clickhouse::Client,
    db: &str,
    table: &str,
    rows: &[RowBuf],
) -> Result<()> {
    // Named columns; store MACs as 12-char hex strings (no unhex()).
    let mut sql = String::with_capacity(rows.len() * 190);
    write!(
        sql,
        "INSERT INTO {}.{} (ts, src_ip, dst_ip, src_mac, dst_mac, l4_proto, src_port, dst_port, packet_len) VALUES ",
        db, table
    )?;

    for (i, r) in rows.iter().enumerate() {
        if i > 0 {
            sql.push(',');
        }

        // ts -> toDateTime64
        let ts_str = r.ts.format("%Y-%m-%d %H:%M:%S").to_string();
        let micros = r.ts.timestamp_subsec_micros();

        // IPs as strings; CH parses with toIPv6()
        let src_ip_s = r.src_ip.to_string();
        let dst_ip_s = r.dst_ip.to_string();

        // MACs as lowercase hex strings (length 12)
        let src_hex = mac_to_hex(&r.src_mac);
        let dst_hex = mac_to_hex(&r.dst_mac);

        write!(
            sql,
            "(toDateTime64('{}.{:06}', 6, 'UTC'), toIPv6('{}'), toIPv6('{}'), '{}', '{}', {}, {}, {}, {})",
            ts_str,
            micros,
            src_ip_s,
            dst_ip_s,
            src_hex,
            dst_hex,
            r.l4_proto,
            opt_u16_sql(r.src_port),
            opt_u16_sql(r.dst_port),
            r.packet_len
        )?;
    }

    client
        .query(&sql)
        .execute()
        .await
        .with_context(|| "insert batch failed")
}

fn mac_to_hex(bytes: &[u8; 6]) -> String {
    let mut s = String::with_capacity(12);
    for b in bytes {
        write!(&mut s, "{:02x}", b).unwrap();
    }
    s
}

fn opt_u16_sql(v: Option<u16>) -> String {
    match v {
        Some(x) => x.to_string(),
        None => "NULL".to_string(),
    }
}

fn map_l4_code(transport: Option<&TransportSlice>, ip_proto_u8: Option<u8>) -> i16 {
    if let Some(t) = transport {
        return match t {
            TransportSlice::Tcp(_) => 6,
            TransportSlice::Udp(_) => 17,
            _ => 0,
        };
    }
    match ip_proto_u8.unwrap_or(0) {
        1 => 1,     // ICMP
        6 => 6,     // TCP
        17 => 17,   // UDP
        132 => 132, // SCTP
        _ => 0,     // NONE
    }
}
