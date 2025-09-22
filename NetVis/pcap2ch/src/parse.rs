use anyhow::Result;
use etherparse::SlicedPacket;
use pcap_parser::pcapng::{Block, PcapNGReader};
use pcap_parser::traits::PcapReaderIterator; // for next/consume/refill
use pcap_parser::{LegacyPcapReader, PcapBlockOwned, PcapError};
use tracing::info;

use crate::db::DbPacket;
use crate::summary::summarize_packet;
use crate::util::{CaptureFormat, read_all, sniff_format};

pub fn run_file(path: &str, limit: Option<usize>) -> Result<Vec<DbPacket>> {
    let mut buf = read_all(path)?;
    let fmt = sniff_format(&buf)?;
    info!(%path, ?fmt, "detected capture format");

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
) -> Result<()> {
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
                if let Err(e) = r.refill() {
                    return Err(anyhow::anyhow!("{:?}", e));
                }
            }
            Err(e) => return Err(anyhow::anyhow!("{:?}", e)),
        }
    }
    info!(seen, "finished pcap file");
    Ok(())
}

fn parse_pcapng(
    buf: &mut [u8],
    limit: Option<usize>,
    mut sink: Option<&mut Vec<DbPacket>>,
) -> Result<()> {
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
                if let Err(e) = r.refill() {
                    return Err(anyhow::anyhow!("{:?}", e));
                }
            }
            Err(e) => return Err(anyhow::anyhow!("{:?}", e)),
        }
    }
    info!(seen, "finished pcapng file");
    Ok(())
}

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
        Some(etherparse::TransportSlice::Tcp(t)) => {
            rec.l4_proto = L4Proto::Tcp;
            let sp_ = t.source_port();
            let dp_ = t.destination_port();
            rec.src_port = Some(sp_);
            rec.dst_port = Some(dp_);
            rec.l7_proto = classify_l7(Some(dp_), rec.l4_proto);
        }
        Some(etherparse::TransportSlice::Udp(u)) => {
            rec.l4_proto = L4Proto::Udp;
            let sp_ = u.source_port();
            let dp_ = u.destination_port();
            rec.src_port = Some(sp_);
            rec.dst_port = Some(dp_);
            rec.l7_proto = classify_l7(Some(dp_), rec.l4_proto);
        }
        Some(etherparse::TransportSlice::Icmpv4(_)) => {
            rec.l4_proto = L4Proto::Icmp;
            rec.l7_proto = L7Proto::UNKNOWN;
        }
        Some(etherparse::TransportSlice::Icmpv6(_)) => {
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
