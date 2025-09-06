use anyhow::{Context, Result};
use chrono::{DateTime, Utc, TimeZone};
use clap::Parser;
use clickhouse::Row;
use etherparse::{InternetSlice, SlicedPacket, TransportSlice};
use pcap::Capture;
use std::net::Ipv6Addr;

#[derive(Debug, Parser)]
#[command(name = "pcap2ch", about = "pcap -> ClickHouse ingestor for net_min.packets")]
struct Args {
    /// ClickHouse HTTP(S) URL, e.g. http://localhost:8123 or https://host:8443
    /// You can include ?database=net_min in the DSN, or pass --database.
    #[arg(long, env = "CH_DSN")]
    dsn: String,

    /// Database (optional if provided via DSN query param)
    #[arg(long, default_value = "net_min")]
    database: String,

    /// Target table
    #[arg(long, default_value = "packets")]
    table: String,

    /// Batch size
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

#[derive(Row, serde::Serialize)]
struct PacketRow {
    // DateTime64(6, 'UTC')
    #[serde(with = "clickhouse::serde::chrono::datetime64::micros")]
    ts: DateTime<Utc>,

    // IPv6 columns (IPv4 mapped to ::ffff:a.b.c.d)
    src_ip: Ipv6Addr,
    dst_ip: Ipv6Addr,

    // Enum16 numeric code (Int16 in CH)
    // Expected mapping: NONE=0, ICMP=1, TCP=6, UDP=17, SCTP=132
    l4_proto: i16,

    // Nullable ports
    src_port: Option<u16>,
    dst_port: Option<u16>,

    // UInt32
    packet_len: u32,
}

#[tokio::main]
async fn main() -> Result<()> {
    let args = Args::parse();
    if args.files.is_empty() {
        anyhow::bail!("Provide at least one pcap: pcap2ch --dsn http://localhost:8123 ./capture.pcap");
    }

    // Build ClickHouse client
    let mut client = clickhouse::Client::default()
        .with_url(&args.dsn)
        .with_database(&args.database);
    if let Some(u) = &args.user { client = client.with_user(u); }
    if let Some(p) = &args.password { client = client.with_password(p); }

    // Create an inserter (sync)
    let mut insert = client
        .insert(&args.table)
        .with_context(|| format!("opening insert into {}.{}", args.database, args.table))?;

    // Buffer rows to flush in batches
    let mut buf: Vec<PacketRow> = Vec::with_capacity(args.batch);
    let mut total = 0usize;

    for path in &args.files {
        ingest_file(path, &mut buf).with_context(|| format!("ingesting {}", path))?;

        // flush in batches
        while buf.len() >= args.batch {
            for row in buf.drain(..args.batch) {
                insert.write(&row).await?;
                total += 1;
            }
            insert.end().await?;                  // commit batch
            insert = client.insert(&args.table)?; // reopen for next batch
        }
    }

    // final flush (any remainder)
    for row in buf.drain(..) {
        insert.write(&row).await?;
        total += 1;
    }
    insert.end().await?;

    eprintln!("Inserted {} rows into {}.{}", total, args.database, args.table);
    Ok(())
}

fn ingest_file(path: &str, out: &mut Vec<PacketRow>) -> Result<()> {
    let mut cap = Capture::from_file(path)
        .with_context(|| format!("open pcap {}", path))?;

    while let Ok(pkt) = cap.next_packet() {
        // timestamp: seconds + micros → DateTime<Utc>
        let ts = Utc
            .timestamp_opt(pkt.header.ts.tv_sec.into(), (pkt.header.ts.tv_usec as u32) * 1000)
            .single()
            .unwrap_or_else(|| Utc.timestamp_opt(0, 0).unwrap());

        let data = pkt.data;
        let packet_len = pkt.header.len;

        // Parse Ethernet/IP/Transport
        let sp = match SlicedPacket::from_ethernet(data) {
            Ok(s) => s,
            Err(_) => continue, // skip malformed/non-eth frames
        };

        // Defaults
        let mut src_ip = Ipv6Addr::UNSPECIFIED;
        let mut dst_ip = Ipv6Addr::UNSPECIFIED;
        let mut src_port: Option<u16> = None;
        let mut dst_port: Option<u16> = None;
        let mut l4_code: i16 = 0; // NONE

        if let Some(net) = &sp.net {
            match net {
                InternetSlice::Ipv4(v4) => {
                    src_ip = v4.header().source_addr().to_ipv6_mapped();
                    dst_ip = v4.header().destination_addr().to_ipv6_mapped();
                    l4_code = map_l4_code(sp.transport.as_ref(), Some(v4.header().protocol().into()));
                }
                InternetSlice::Ipv6(v6) => {
                    src_ip = v6.header().source_addr();
                    dst_ip = v6.header().destination_addr();
                    l4_code = map_l4_code(sp.transport.as_ref(), Some(v6.header().next_header().into()));
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

        out.push(PacketRow {
            ts,
            src_ip,
            dst_ip,
            l4_proto: l4_code,
            src_port,
            dst_port,
            packet_len: packet_len as u32,
        });
    }

    Ok(())
}

// Map to Enum16 numeric codes in your CH schema
fn map_l4_code(transport: Option<&TransportSlice>, ip_proto_u8: Option<u8>) -> i16 {
    if let Some(t) = transport {
        return match t {
            TransportSlice::Tcp(_) => 6,   // TCP
            TransportSlice::Udp(_) => 17,  // UDP
            _ => 0,                        // NONE
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
