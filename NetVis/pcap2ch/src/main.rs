mod db;

use clap::{Parser, Subcommand};
use clickhouse::Client;
use db::{DbPacket, l4_label_from_code, l7_label_from_code, mac_to_hex, to_db_packet, ts_to_str};
use etherparse::{SlicedPacket, TransportSlice};
use pcap_parser::pcapng::{Block, PcapNGReader};
use pcap_parser::{LegacyPcapReader, PcapBlockOwned, PcapError};
use std::{fs::File, io::Read, path::Path};
use tracing_subscriber::{EnvFilter, fmt};

/// Packet parser — supports live capture (later) and offline pcap/pcapng (next milestone).
#[derive(Debug, Parser)]
#[command(author, version, about)]
struct Cli {
    /// ClickHouse URL (e.g., http://localhost:8123) — used in Milestone 3
    #[arg(long, env = "CH_URL", default_value = "http://10.200.1.13:8123")]
    ch_url: String,

    /// ClickHouse database name
    #[arg(long, env = "CH_DB", default_value = "net")]
    ch_db: String,

    /// ClickHouse user
    #[arg(long, env = "CH_USER", default_value = "default")]
    ch_user: String,

    /// ClickHouse password (leave blank if none)
    #[arg(long, env = "CH_PASSWORD", default_value = "")]
    ch_password: String,

    /// ClickHouse table (must be net.packets in your schema)
    #[arg(long, env = "CH_TABLE", default_value = "net.packets")]
    ch_table: String,

    #[command(subcommand)]
    cmd: Cmd,
}

#[derive(Debug, Subcommand)]
enum Cmd {
    File(FileArgs),
    Live(LiveArgs),
}

#[derive(Debug, clap::Args)]
struct FileArgs {
    /// Parse an offline capture file (.pcap or .pcapng)

    /// Path to capture file
    #[arg(long)]
    path: String,
    /// Optional: limit number of packets (debug/testing)
    #[arg(long)]
    limit: Option<usize>,
    /// If set, write parsed packets to ClickHouse (otherwise just log)
    #[arg(long, default_value_t = false)]
    insert: bool,
    /// Batch size for ClickHouse inserts
    #[arg(long, default_value_t = 10_000)]
    batch_size: usize,
}
/// (Milestone 4) Live-capture from a network interface
#[derive(Debug, clap::Args)]
struct LiveArgs {
    /// Interface name, e.g., eth0
    iface: String,
    /// Optional BPF filter string
    #[arg(long)]
    filter: Option<String>,
    /// Snaplen bytes
    #[arg(long, default_value_t = 262144)]
    snaplen: i32,
    /// Batch size before flush
    #[arg(long, default_value_t = 10_000)]
    batch_size: usize,
}

#[derive(Debug, Clone, Copy)]
enum CaptureFormat {
    Pcap,
    PcapNg,
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
        0x0A0D0D0A => Ok(CaptureFormat::PcapNg), // pcapng
        0xA1B2C3D4 | 0xD4C3B2A1 | 0xA1B23C4D | 0x4D3CB2A1 => Ok(CaptureFormat::Pcap), // pcap (incl. ns variants)
        _ => anyhow::bail!("unrecognized capture magic: 0x{m_be:08x}"),
    }
}

fn run_file(path: &str, limit: Option<usize>) -> anyhow::Result<()> {
    let mut buf = read_all(path)?;
    let fmt = sniff_format(&buf)?;
    tracing::info!(%path, ?fmt, "detected capture format");

    match fmt {
        CaptureFormat::Pcap => parse_pcap(&mut buf, limit)?,
        CaptureFormat::PcapNg => parse_pcapng(&mut buf, limit)?,
    }
    Ok(())
}

fn init_tracing() {
    let filter = EnvFilter::try_from_default_env().unwrap_or_else(|_| EnvFilter::new("info"));
    fmt().with_env_filter(filter).init();
}

fn parse_pcap(buf: &mut [u8], limit: Option<usize>) -> anyhow::Result<()> {
    use pcap_parser::traits::PcapReaderIterator;

    let mut r = LegacyPcapReader::new(65536, &buf[..])?;
    let mut seen = 0usize;

    loop {
        if let Some(max) = limit {
            if seen >= max {
                break;
            }
        }
        match r.next() {
            Ok((offset, blk)) => {
                if let PcapBlockOwned::Legacy(b) = blk {
                    let ts = pcap2ch::ts_sec_usec_to_dt64_us(b.ts_sec as i64, b.ts_usec as u32);
                    let caplen = b.caplen;
                    let origlen = b.origlen;
                    if let Ok(sp) = SlicedPacket::from_ethernet(b.data) {
                        let rec = build_record_from_slice(ts, caplen, origlen, &sp);
                        log_packet_summary(&rec);
                        seen += 1;
                    }
                }
                // IMPORTANT: advance the internal buffer
                r.consume(offset);
            }
            Err(PcapError::Eof) => break,
            // Offline buffer is complete; nothing to refill → stop.
            Err(PcapError::Incomplete(_)) => break,
            Err(e) => return Err(anyhow::anyhow!("pcap read error: {e:?}")),
        }
    }

    tracing::info!(seen, "finished pcap file");
    Ok(())
}

fn parse_pcapng(buf: &mut [u8], limit: Option<usize>) -> anyhow::Result<()> {
    use pcap_parser::traits::PcapReaderIterator;

    let mut r = PcapNGReader::new(65536, &buf[..])?;
    let mut seen = 0usize;

    loop {
        if let Some(max) = limit {
            if seen >= max {
                break;
            }
        }
        match r.next() {
            Ok((offset, blk)) => {
                if let pcap_parser::PcapBlockOwned::NG(Block::EnhancedPacket(epb)) = blk {
                    // Default to microseconds for 0.17
                    let res_pow10: u8 = 6;
                    let ticks = ((epb.ts_high as u64) << 32) | (epb.ts_low as u64);
                    let ts = pcap2ch::ts_pow10_to_dt64_us(ticks, res_pow10);

                    let caplen = epb.caplen;
                    let origlen = epb.origlen;
                    let data = epb.data; // &[u8]
                    if let Ok(sp) = SlicedPacket::from_ethernet(data) {
                        let rec = build_record_from_slice(ts, caplen, origlen, &sp);
                        log_packet_summary(&rec);
                        seen += 1;
                    }
                }
                // IMPORTANT: advance the internal buffer
                r.consume(offset);
            }
            Err(PcapError::Eof) => break,
            Err(PcapError::Incomplete(_)) => break,
            Err(e) => return Err(anyhow::anyhow!("pcapng read error: {e:?}")),
        }
    }

    tracing::info!(seen, "finished pcapng file");
    Ok(())
}

fn build_record_from_slice(
    ts: chrono::DateTime<chrono::Utc>,
    caplen: u32,
    origlen: u32,
    sp: &SlicedPacket,
) -> pcap2ch::PacketRecord {
    use pcap2ch::{L4Proto, L7Proto, PacketRecord, v4_to_v6_mapped};

    let mut rec = PacketRecord::default();
    rec.ts = ts;
    rec.packet_len = caplen.max(origlen);

    let l7_try = |sp: u16, dp: u16, l4| {
        let d = pcap2ch::classify_l7(Some(dp), l4);
        if d as i32 != pcap2ch::L7Proto::UNKNOWN as i32 {
            d
        } else {
            pcap2ch::classify_l7(Some(sp), l4)
        }
    };

    // L2 MACs
    if let Some(etherparse::LinkSlice::Ethernet2(eth)) = sp.link.as_ref() {
        rec.src_mac = eth.source();
        rec.dst_mac = eth.destination();
    }

    // L3 IPs
    if let Some(net) = sp.net.as_ref() {
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

    // L4 + ports
    match sp.transport.as_ref() {
        Some(TransportSlice::Tcp(t)) => {
            rec.l4_proto = L4Proto::Tcp;
            let sp_ = t.source_port();
            let dp_ = t.destination_port();
            rec.src_port = Some(sp_);
            rec.dst_port = Some(dp_);
            rec.l7_proto = l7_try(sp_, dp_, rec.l4_proto);
        }
        Some(TransportSlice::Udp(u)) => {
            rec.l4_proto = L4Proto::Udp;
            let sp_ = u.source_port();
            let dp_ = u.destination_port();
            rec.src_port = Some(sp_);
            rec.dst_port = Some(dp_);
            rec.l7_proto = l7_try(sp_, dp_, rec.l4_proto);
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

async fn run_file_and_insert(
    client: &Client, // from db re-export
    table: &str,
    path: &str,
    limit: Option<usize>,
    batch_size: usize,
) -> anyhow::Result<()> {
    // A tiny channel-free approach: collect then insert in blocks.
    // For very large files you could stream, but this keeps the diff small.

    let mut rows: Vec<DbPacket> = Vec::with_capacity(limit.unwrap_or(batch_size).min(batch_size));

    // A local closure to process one PacketRecord -> maybe insert batch
    let mut push = |rec: pcap2ch::PacketRecord| -> anyhow::Result<()> {
        rows.push(to_db_packet(&rec));
        Ok(())
    };

    // Parse & collect rows (copy of your run_file path but with a sink)
    {
        let mut buf = read_all(path)?;
        let fmt = sniff_format(&buf)?;
        tracing::info!(%path, ?fmt, "detected capture format (insert mode)");

        match fmt {
            CaptureFormat::Pcap => parse_pcap_with_sink(&mut buf, limit, &mut push)?,
            CaptureFormat::PcapNg => parse_pcapng_with_sink(&mut buf, limit, &mut push)?,
        }
    }

    // Now write out in batches
    let mut written = 0usize;
    let mut i = 0usize;
    while i < rows.len() {
        let end = (i + batch_size).min(rows.len());
        let slice = &rows[i..end];

        // Build one INSERT ... VALUES with explicit casts and labels
        let mut sql = String::with_capacity(slice.len() * 256);
        sql.push_str("INSERT INTO ");
        sql.push_str(table);
        sql.push_str(" (ts, src_ip, dst_ip, src_mac, dst_mac, l4_proto, l7_proto, src_port, dst_port, packet_len, raw) VALUES ");

        for (j, row) in slice.iter().enumerate() {
            // Convert fields to proper literals
            let ts = ts_to_str(row.ts);
            let src_ip = row.src_ip.to_string();
            let dst_ip = row.dst_ip.to_string();
            let src_mac_hex = mac_to_hex(&row.src_mac);
            let dst_mac_hex = mac_to_hex(&row.dst_mac);
            let l4 = l4_label_from_code(row.l4_proto);
            let l7 = l7_label_from_code(row.l7_proto);

            // Nullable ports → literal NULL or number
            let src_port_lit = row
                .src_port
                .map(|v| v.to_string())
                .unwrap_or_else(|| "NULL".to_string());
            let dst_port_lit = row
                .dst_port
                .map(|v| v.to_string())
                .unwrap_or_else(|| "NULL".to_string());

            // raw is non-null String; we’re writing empty '' for now
            let raw = ""; // or escape if you later include data

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
                  '{l4}', '{l7}', {src_port_lit}, {dst_port_lit}, {packet_len}, '{raw}')",
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

fn parse_pcap_with_sink<F>(buf: &mut [u8], limit: Option<usize>, sink: &mut F) -> anyhow::Result<()>
where
    F: FnMut(pcap2ch::PacketRecord) -> anyhow::Result<()>,
{
    use pcap_parser::traits::PcapReaderIterator;

    let mut r = LegacyPcapReader::new(65536, &buf[..])?;
    let mut seen = 0usize;

    loop {
        if let Some(max) = limit {
            if seen >= max {
                break;
            }
        }
        match r.next() {
            Ok((offset, blk)) => {
                if let PcapBlockOwned::Legacy(b) = blk {
                    let ts = pcap2ch::ts_sec_usec_to_dt64_us(b.ts_sec as i64, b.ts_usec as u32);
                    let caplen = b.caplen;
                    let origlen = b.origlen;
                    if let Ok(sp) = SlicedPacket::from_ethernet(b.data) {
                        let rec = build_record_from_slice(ts, caplen, origlen, &sp);
                        sink(rec)?; // push to batch
                        seen += 1;
                    }
                }
                r.consume(offset);
            }
            Err(PcapError::Eof) => break,
            Err(PcapError::Incomplete(_)) => break,
            Err(e) => return Err(anyhow::anyhow!("pcap read error: {e:?}")),
        }
    }
    Ok(())
}

fn parse_pcapng_with_sink<F>(
    buf: &mut [u8],
    limit: Option<usize>,
    sink: &mut F,
) -> anyhow::Result<()>
where
    F: FnMut(pcap2ch::PacketRecord) -> anyhow::Result<()>,
{
    use pcap_parser::traits::PcapReaderIterator;

    let mut r = PcapNGReader::new(65536, &buf[..])?;
    let mut seen = 0usize;

    loop {
        if let Some(max) = limit {
            if seen >= max {
                break;
            }
        }
        match r.next() {
            Ok((offset, blk)) => {
                if let pcap_parser::PcapBlockOwned::NG(Block::EnhancedPacket(epb)) = blk {
                    let res_pow10: u8 = 6;
                    let ticks = ((epb.ts_high as u64) << 32) | (epb.ts_low as u64);
                    let ts = pcap2ch::ts_pow10_to_dt64_us(ticks, res_pow10);

                    let caplen = epb.caplen;
                    let origlen = epb.origlen;
                    let data = epb.data;
                    if let Ok(sp) = SlicedPacket::from_ethernet(data) {
                        let rec = build_record_from_slice(ts, caplen, origlen, &sp);
                        sink(rec)?; // push to batch
                        seen += 1;
                    }
                }
                r.consume(offset);
            }
            Err(PcapError::Eof) => break,
            Err(PcapError::Incomplete(_)) => break,
            Err(e) => return Err(anyhow::anyhow!("pcapng read error: {e:?}")),
        }
    }
    Ok(())
}

#[tokio::main]
async fn main() -> anyhow::Result<()> {
    init_tracing();
    let args = Cli::parse();

    match args.cmd {
        Cmd::File(file) => {
            if file.insert {
                let mut client = Client::default()
                    .with_url(&args.ch_url)
                    .with_database(&args.ch_db);

                // Only set user/password if provided (non-empty password)
                if !args.ch_user.is_empty() {
                    client = client.with_user(&args.ch_user);
                }
                if !args.ch_password.is_empty() {
                    client = client.with_password(&args.ch_password);
                }

                run_file_and_insert(
                    &client,
                    &args.ch_table,
                    &file.path,
                    file.limit,
                    file.batch_size,
                )
                .await?;
            } else {
                run_file(&file.path, file.limit)?; // current logging path
            }
        }
        Cmd::Live(live) => {
            tracing::info!(iface=%live.iface, ?live.filter, snaplen=live.snaplen, batch_size=live.batch_size,
                "live mode (to be implemented later)");
        }
    }
    Ok(())
}
