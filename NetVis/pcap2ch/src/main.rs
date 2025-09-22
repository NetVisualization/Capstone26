use clap::{Parser, Subcommand};
use etherparse::{SlicedPacket, TransportSlice};
use pcap_parser::pcapng::{Block, PcapNGReader};
use pcap_parser::{LegacyPcapReader, PcapBlockOwned, PcapError};
// IMPORTANT: bring iterator trait into scope for next/consume/refill
use pcap_parser::traits::PcapReaderIterator;

use std::{fs::File, io::Read, path::Path};
use tracing_subscriber::{EnvFilter, fmt};

mod db;

use clickhouse::Client;
use db::{DbPacket, l4_label_from_code, l7_label_from_code, mac_to_hex, ts_to_str};

/// Packet parser/ingestor — offline pcap/pcapng; live capture next.
#[derive(Debug, Parser)]
#[command(author, version, about)]
struct Cli {
    /// ClickHouse URL (e.g., http://localhost:8123)
    #[arg(long, env = "CH_URL", default_value = "http://localhost:8123")]
    ch_url: String,

    /// ClickHouse database name
    #[arg(long, env = "CH_DB", default_value = "default")]
    ch_db: String,

    /// ClickHouse table (must be net.packets)
    #[arg(long, env = "CH_TABLE", default_value = "net.packets")]
    ch_table: String,

    /// ClickHouse user
    #[arg(long, env = "CH_USER", default_value = "default")]
    ch_user: String,

    /// ClickHouse password
    #[arg(long, env = "CH_PASSWORD", default_value = "")]
    ch_password: String,

    #[command(subcommand)]
    cmd: Cmd,
}

#[derive(Debug, Subcommand)]
enum Cmd {
    /// Parse an offline capture file (.pcap or .pcapng)
    File {
        /// Path to capture file
        #[arg(long)]
        path: String,
        /// Optional: limit number of packets (debug/testing)
        #[arg(long)]
        limit: Option<usize>,
        /// Insert into ClickHouse
        #[arg(long, default_value_t = false)]
        insert: bool,
        /// Batch size for inserts
        #[arg(long, default_value_t = 5_000)]
        batch_size: usize,
    },
    /// Live capture (to be implemented later)
    Live {
        iface: String,
        #[arg(long)]
        filter: Option<String>,
        #[arg(long, default_value_t = 262_144)]
        snaplen: i32,
        #[arg(long, default_value_t = 10_000)]
        batch_size: usize,
    },
}

#[derive(Debug, Clone, Copy)]
enum CaptureFormat {
    Pcap,
    PcapNg,
}

/* --------------------- init & utils ---------------------- */

fn init_tracing() {
    let filter = EnvFilter::try_from_default_env().unwrap_or_else(|_| EnvFilter::new("info"));
    fmt().with_env_filter(filter).init();
}

fn read_all<P: AsRef<Path>>(path: P) -> anyhow::Result<Vec<u8>> {
    let mut v = Vec::new();
    File::open(path.as_ref())?.read_to_end(&mut v)?;
    Ok(v)
}

fn sniff_format(buf: &[u8]) -> anyhow::Result<CaptureFormat> {
    if buf.len() < 4 {
        anyhow::bail!("file too small to detect format");
    }
    let m_be = u32::from_be_bytes([buf[0], buf[1], buf[2], buf[3]]);
    match m_be {
        0x0A0D0D0A => Ok(CaptureFormat::PcapNg),
        0xA1B2C3D4 | 0xD4C3B2A1 | 0xA1B23C4D | 0x4D3CB2A1 => Ok(CaptureFormat::Pcap),
        _ => anyhow::bail!("unrecognized capture magic: 0x{m_be:08x}"),
    }
}

/* --------------------- summarizers ---------------------- */

fn escape_sql_single_quotes(s: &str) -> String {
    s.replace('\'', "''")
}

// DNS/mDNS minimal summary for Wireshark-like "Info"
fn try_mdns_summary(payload: &[u8]) -> Option<String> {
    use dns_parser::Packet;
    if let Ok(pkt) = Packet::parse(payload) {
        if pkt.header.query {
            if let Some(q) = pkt.questions.first() {
                // QueryType is not Display; use {:?}
                return Some(format!("Standard query {} {:?}", q.qname, q.qtype));
            }
        } else {
            if let Some(ans) = pkt.answers.first() {
                // Class is not Display; use {:?}
                return Some(format!(
                    "Standard query response {} {:?}",
                    ans.name, ans.cls
                ));
            } else {
                return Some(format!(
                    "Standard query response {} answers",
                    pkt.answers.len()
                ));
            }
        }
    }
    None
}

fn tcp_flags_string(t: &etherparse::TcpSlice<'_>) -> String {
    let mut flags = Vec::new();
    if t.syn() {
        flags.push("SYN");
    }
    if t.fin() {
        flags.push("FIN");
    }
    if t.rst() {
        flags.push("RST");
    }
    if t.psh() {
        flags.push("PSH");
    }
    if t.ack() {
        flags.push("ACK");
    }
    if t.urg() {
        flags.push("URG");
    }
    if t.ece() {
        flags.push("ECE");
    }
    if t.cwr() {
        flags.push("CWR");
    }
    if t.ns() {
        flags.push("NS");
    }
    if flags.is_empty() {
        String::from("—")
    } else {
        format!("[{}]", flags.join(","))
    }
}

/// Build a short "Info" string similar to Wireshark for our current protocols (mDNS, TLS, generic TCP/UDP).
fn summarize_packet(sp: &etherparse::SlicedPacket<'_>) -> String {
    use etherparse::TransportSlice;

    match &sp.transport {
        Some(TransportSlice::Udp(u)) => {
            let sp_ = u.source_port();
            let dp_ = u.destination_port();
            let payload = u.payload();
            if sp_ == 5353 || dp_ == 5353 {
                if let Some(s) = try_mdns_summary(payload) {
                    return format!("MDNS {}", s);
                } else {
                    return format!("MDNS len={}", payload.len());
                }
            }
            format!("UDP len={}", payload.len())
        }
        Some(TransportSlice::Tcp(t)) => {
            let sp_ = t.source_port();
            let dp_ = t.destination_port();
            let flags = tcp_flags_string(&t);
            let payload_len = t.payload().len();
            if sp_ == 443 || dp_ == 443 {
                if payload_len > 0 {
                    return String::from("TLSv1.x Application Data");
                } else {
                    return format!(
                        "TCP {} Seq={} Ack={} Win={} Len={}",
                        flags,
                        t.sequence_number(),
                        t.acknowledgment_number(),
                        t.window_size(),
                        payload_len
                    );
                }
            }
            format!(
                "TCP {} Seq={} Ack={} Win={} Len={}",
                flags,
                t.sequence_number(),
                t.acknowledgment_number(),
                t.window_size(),
                payload_len
            )
        }
        Some(TransportSlice::Icmpv4(_)) => String::from("ICMP"),
        Some(TransportSlice::Icmpv6(_)) => String::from("ICMPv6"),
        None => String::from("len=0"),
    }
}

/* ------------------- record building -------------------- */

fn build_record_from_slice(
    ts: chrono::DateTime<chrono::Utc>,
    caplen: u32,
    origlen: u32,
    sp: &SlicedPacket,
) -> pcap2ch::PacketRecord {
    use pcap2ch::{L4Proto, L7Proto, PacketRecord, classify_l7, v4_to_v6_mapped};

    let mut rec = PacketRecord::default();
    rec.ts = ts;
    rec.packet_len = caplen.max(origlen);

    // L2 MACs
    if let Some(link) = &sp.link {
        if let etherparse::LinkSlice::Ethernet2(eth) = link {
            rec.src_mac.copy_from_slice(&eth.source());
            rec.dst_mac.copy_from_slice(&eth.destination());
        }
    }

    // L3 IPs
    if let Some(net) = &sp.net {
        match net {
            etherparse::InternetSlice::Ipv4(h) => {
                let h4 = h.header();
                rec.src_ip = v4_to_v6_mapped(h4.source_addr());
                rec.dst_ip = v4_to_v6_mapped(h4.destination_addr());
            }
            etherparse::InternetSlice::Ipv6(h) => {
                let h6 = h.header();
                rec.src_ip = h6.source_addr();
                rec.dst_ip = h6.destination_addr();
            }
            _ => {}
        }
    }

    // L4 + ports (prefer destination for L7)
    match &sp.transport {
        Some(TransportSlice::Tcp(t)) => {
            rec.l4_proto = L4Proto::Tcp;
            let sp_ = t.source_port();
            let dp_ = t.destination_port();
            rec.src_port = Some(sp_);
            rec.dst_port = Some(dp_);
            // prefer destination port for app classification
            rec.l7_proto = classify_l7(Some(dp_), rec.l4_proto);
        }
        Some(TransportSlice::Udp(u)) => {
            rec.l4_proto = L4Proto::Udp;
            let sp_ = u.source_port();
            let dp_ = u.destination_port();
            rec.src_port = Some(sp_);
            rec.dst_port = Some(dp_);
            rec.l7_proto = classify_l7(Some(dp_), rec.l4_proto);
        }
        Some(TransportSlice::Icmpv4(_)) => {
            rec.l4_proto = L4Proto::Icmp;
            rec.l7_proto = L7Proto::UNKNOWN;
        }
        Some(TransportSlice::Icmpv6(_)) => {
            rec.l4_proto = L4Proto::Icmpv6;
            rec.l7_proto = L7Proto::UNKNOWN;
        }
        None => {
            rec.l4_proto = L4Proto::None;
            rec.l7_proto = L7Proto::UNKNOWN;
        }
    }

    rec
}

fn log_packet_summary(rec: &pcap2ch::PacketRecord) {
    use pcap2ch::mac_hex;
    tracing::info!(
        ts = %rec.ts,
        src_ip = %rec.src_ip,
        dst_ip = %rec.dst_ip,
        src_mac = %mac_hex(&rec.src_mac),
        dst_mac = %mac_hex(&rec.dst_mac),
        l4 = ?rec.l4_proto as i32,
        l7 = ?rec.l7_proto as i32,
        src_port = ?rec.src_port,
        dst_port = ?rec.dst_port,
        len = rec.packet_len,
        "packet"
    );
}

/* ----------------------- runners ------------------------ */

fn run_file(path: &str, limit: Option<usize>) -> anyhow::Result<Vec<DbPacket>> {
    let mut buf = read_all(path)?;
    let fmt = sniff_format(&buf)?;
    tracing::info!(%path, ?fmt, "detected capture format");

    let mut rows = Vec::<DbPacket>::new();

    match fmt {
        CaptureFormat::Pcap => parse_pcap(&mut buf, limit, Some(&mut rows))?,
        CaptureFormat::PcapNg => parse_pcapng(&mut buf, limit, Some(&mut rows))?,
    }
    Ok(rows)
}

fn parse_pcap(
    buf: &mut [u8],
    limit: Option<usize>,
    mut sink: Option<&mut Vec<DbPacket>>,
) -> anyhow::Result<()> {
    let mut r = LegacyPcapReader::new(65536, &buf[..])?;
    let mut seen = 0usize;

    loop {
        if let Some(max) = limit {
            if seen >= max {
                break;
            }
        }
        match r.next() {
            Ok((off, blk)) => {
                if let PcapBlockOwned::Legacy(b) = blk {
                    let ts = pcap2ch::ts_sec_usec_to_dt64_us(b.ts_sec as i64, b.ts_usec as u32);
                    let caplen = b.caplen;
                    let origlen = b.origlen;
                    if let Ok(sp) = SlicedPacket::from_ethernet(b.data) {
                        let rec = build_record_from_slice(ts, caplen, origlen, &sp);
                        log_packet_summary(&rec);
                        if let Some(ref mut rows) = sink {
                            let info = summarize_packet(&sp);
                            rows.push(DbPacket::from_record_with_raw(&rec, info));
                        }
                        seen += 1;
                    }
                }
                r.consume(off);
            }
            Err(PcapError::Eof) => break,
            Err(PcapError::Incomplete(_)) => {
                // NO `?` — convert to owned error if it happens
                if let Err(e) = r.refill() {
                    return Err(anyhow::anyhow!("{:?}", e));
                }
            }
            Err(e) => return Err(anyhow::anyhow!("{:?}", e)),
        }
    }

    tracing::info!(seen, "finished pcap file");
    Ok(())
}

fn parse_pcapng(
    buf: &mut [u8],
    limit: Option<usize>,
    mut sink: Option<&mut Vec<DbPacket>>,
) -> anyhow::Result<()> {
    let mut r = PcapNGReader::new(65536, &buf[..])?;
    let mut seen = 0usize;

    loop {
        if let Some(max) = limit {
            if seen >= max {
                break;
            }
        }
        match r.next() {
            Ok((off, blk)) => {
                if let pcap_parser::PcapBlockOwned::NG(Block::EnhancedPacket(epb)) = blk {
                    // Assume µs for now (iface tsresol later)
                    let res_pow10: u8 = 6;
                    let ticks = ((epb.ts_high as u64) << 32) | (epb.ts_low as u64);
                    let ts = pcap2ch::ts_pow10_to_dt64_us(ticks, res_pow10);
                    let caplen = epb.caplen;
                    let origlen = epb.origlen;
                    if let Ok(sp) = SlicedPacket::from_ethernet(epb.data) {
                        let rec = build_record_from_slice(ts, caplen, origlen, &sp);
                        log_packet_summary(&rec);
                        if let Some(ref mut rows) = sink {
                            let info = summarize_packet(&sp);
                            rows.push(DbPacket::from_record_with_raw(&rec, info));
                        }
                        seen += 1;
                    }
                }
                r.consume(off);
            }
            Err(PcapError::Eof) => break,
            Err(PcapError::Incomplete(_)) => {
                // NO `?` — convert to owned error if it happens
                if let Err(e) = r.refill() {
                    return Err(anyhow::anyhow!("{:?}", e));
                }
            }
            Err(e) => return Err(anyhow::anyhow!("{:?}", e)),
        }
    }

    tracing::info!(seen, "finished pcapng file");
    Ok(())
}

/* --------------- INSERT via text VALUES ----------------- */

async fn run_file_and_insert(
    client: &Client,
    table: &str,
    path: &str,
    limit: Option<usize>,
    batch_size: usize,
) -> anyhow::Result<()> {
    let rows = run_file(path, limit)?;
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

/* --------------------------- main --------------------------- */

#[tokio::main]
async fn main() -> anyhow::Result<()> {
    init_tracing();
    let args = Cli::parse();

    match args.cmd {
        Cmd::File {
            path,
            limit,
            insert,
            batch_size,
        } => {
            if insert {
                let mut client = Client::default()
                    .with_url(&args.ch_url)
                    .with_database(&args.ch_db);
                if !args.ch_user.is_empty() {
                    client = client.with_user(&args.ch_user);
                }
                if !args.ch_password.is_empty() {
                    client = client.with_password(&args.ch_password);
                }

                tracing::info!(%path, "detected capture format (insert mode)");
                run_file_and_insert(&client, &args.ch_table, &path, limit, batch_size).await?;
            } else {
                let rows = run_file(&path, limit)?;
                tracing::info!(count = rows.len(), "parsed rows (no insert)");
            }
        }
        Cmd::Live {
            iface,
            filter,
            snaplen,
            batch_size,
        } => {
            tracing::info!(%iface, ?filter, snaplen, batch_size, "live mode (todo)");
        }
    }

    Ok(())
}
