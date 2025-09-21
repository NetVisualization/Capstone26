use anyhow::{Context, Result};
use chrono::{DateTime, Utc};
use clap::Parser;
use clickhouse::{Client, Row};
use etherparse::{LinkSlice, NetSlice, SlicedPacket, TransportSlice};
use pcap::Capture;
use serde::Serialize;
use std::net::{Ipv4Addr, Ipv6Addr};

#[derive(Parser, Debug)]
#[command(
    author,
    version,
    about = "Parse a PCAP and stream packets into ClickHouse (net.packets)"
)]
struct Args {
    /// Path to input PCAP
    #[arg(long, short = 'f', env = "PCAP_FILE")]
    pcap: String,

    /// ClickHouse HTTP endpoint (e.g., http://localhost:8123)
    #[arg(long, env = "CH_URL", default_value = "http://localhost:8123")]
    ch_url: String,

    /// ClickHouse database name
    #[arg(long, env = "CH_DB", default_value = "net")]
    ch_db: String,

    /// Batch size (rows per HTTP chunk)
    #[arg(long, default_value_t = 10_000)]
    batch: usize,

    /// Optional ClickHouse user
    #[arg(long, env = "CH_USER")]
    ch_user: Option<String>,

    /// Optional ClickHouse password
    #[arg(long, env = "CH_PASSWORD")]
    ch_password: Option<String>,
}

#[derive(Debug, Clone, Copy)]
enum L4 {
    None = 0,
    Icmp = 1,
    Tcp = 6,
    Udp = 17,
    Sctp = 132,
}

#[derive(Debug, Clone, Copy)]
enum L7 {
    UNKNOWN = 0,
    HTTP = 80,
    TLS = 443,
    DNS = 53,
    MDNS = 5353,
    SSDP = 1900,
    DHCP = 67,
    NTP = 123,
    SSH = 22,
    SMTP = 25,
    IMAP = 143,
    POP3 = 110,
    QUIC = 1000,
    SMB = 445,
    RDP = 3389,
}

#[derive(Row, Serialize, Debug)]
struct PacketRow {
    ts: DateTime<Utc>,
    src_ip: Ipv6Addr,
    dst_ip: Ipv6Addr,
    src_mac: Vec<u8>,
    dst_mac: Vec<u8>,
    l4_proto: i16, // Enum16 -> i16
    l7_proto: i16, // Enum16 -> i16
    src_port: Option<u16>,
    dst_port: Option<u16>,
    packet_len: u32,
    raw: Vec<u8>, // full captured bytes
}

#[tokio::main]
async fn main() -> Result<()> {
    let args = Args::parse();

    let mut client = Client::default()
        .with_url(&args.ch_url)
        .with_database(&args.ch_db);
    if let Some(u) = &args.ch_user {
        client = client.with_user(u);
    }
    if let Some(p) = &args.ch_password {
        client = client.with_password(p);
    }

    // Long-lived inserter (good for streaming / live later)
    let mut inserter = client.insert("net.packets")?; // NOTE: not async

    let mut cap =
        Capture::from_file(&args.pcap).with_context(|| format!("opening pcap {}", args.pcap))?;

    let mut batch_count: usize = 0;

    loop {
        match cap.next_packet() {
            Ok(pkt) => {
                let ts =
                    pcap_ts_to_dt64us(pkt.header.ts.tv_sec as i64, pkt.header.ts.tv_usec as u32);
                let raw = pkt.data.to_vec();
                let packet_len = pkt.header.len;

                // defaults
                let mut src_mac = vec![0u8; 6];
                let mut dst_mac = vec![0u8; 6];
                let mut src_ip = Ipv6Addr::UNSPECIFIED;
                let mut dst_ip = Ipv6Addr::UNSPECIFIED;
                let mut l4_proto = L4::None as i16;
                let mut l7_proto = L7::UNKNOWN as i16;
                let mut src_port: Option<u16> = None;
                let mut dst_port: Option<u16> = None;

                // parse (Ethernet expected; unknown link types => defaults)
                if let Ok(sp) = SlicedPacket::from_ethernet(&raw) {
                    if let Some(LinkSlice::Ethernet2(eth)) = sp.link {
                        dst_mac.copy_from_slice(&eth.destination());
                        src_mac.copy_from_slice(&eth.source());
                    }

                    match sp.net {
                        Some(NetSlice::Ipv4(ip4)) => {
                            let s4 = Ipv4Addr::from(ip4.header().source());
                            let d4 = Ipv4Addr::from(ip4.header().destination());
                            src_ip = s4.to_ipv6_mapped();
                            dst_ip = d4.to_ipv6_mapped();

                            match sp.transport {
                                Some(TransportSlice::Tcp(tcp)) => {
                                    l4_proto = L4::Tcp as i16;
                                    src_port = Some(tcp.source_port());
                                    dst_port = Some(tcp.destination_port());
                                    l7_proto = detect_l7_tcp(src_port, dst_port) as i16; // port-only
                                }
                                Some(TransportSlice::Udp(udp)) => {
                                    l4_proto = L4::Udp as i16;
                                    src_port = Some(udp.source_port());
                                    dst_port = Some(udp.destination_port());
                                    l7_proto = detect_l7_udp(src_port, dst_port) as i16; // port-only
                                }
                                Some(TransportSlice::Icmpv4(_)) => {
                                    l4_proto = L4::Icmp as i16;
                                }
                                _ => {}
                            }
                        }
                        Some(NetSlice::Ipv6(ip6)) => {
                            src_ip = Ipv6Addr::from(ip6.header().source());
                            dst_ip = Ipv6Addr::from(ip6.header().destination());

                            match sp.transport {
                                Some(TransportSlice::Tcp(tcp)) => {
                                    l4_proto = L4::Tcp as i16;
                                    src_port = Some(tcp.source_port());
                                    dst_port = Some(tcp.destination_port());
                                    l7_proto = detect_l7_tcp(src_port, dst_port) as i16; // port-only
                                }
                                Some(TransportSlice::Udp(udp)) => {
                                    l4_proto = L4::Udp as i16;
                                    src_port = Some(udp.source_port());
                                    dst_port = Some(udp.destination_port());
                                    l7_proto = detect_l7_udp(src_port, dst_port) as i16; // port-only
                                }
                                Some(TransportSlice::Icmpv6(_)) => {
                                    l4_proto = L4::Icmp as i16;
                                }
                                _ => {}
                            }
                        }
                        _ => {}
                    }
                }

                let row = PacketRow {
                    ts,
                    src_ip,
                    dst_ip,
                    src_mac,
                    dst_mac,
                    l4_proto,
                    l7_proto,
                    src_port,
                    dst_port,
                    packet_len,
                    raw,
                };

                inserter.write(&row).await?;
                batch_count += 1;

                if batch_count >= args.batch {
                    inserter.end().await?; // finish this chunk
                    inserter = client.insert("net.packets")?; // start next chunk
                    batch_count = 0;
                }
            }
            Err(pcap::Error::NoMorePackets) => break,
            Err(e) => return Err(e).context("reading packet"),
        }
    }

    // flush remainder
    inserter.end().await?;
    Ok(())
}

// ---------- helpers ----------

// pcap ts: seconds + microseconds -> DateTime64(6)
fn pcap_ts_to_dt64us(sec: i64, usec: u32) -> DateTime<Utc> {
    // microseconds -> nanoseconds component
    let nanos = (usec as u32) * 1_000; // 1e-6 s -> 1e-9 s
    // chrono 0.4.41: from_timestamp returns Option
    DateTime::from_timestamp(sec, nanos * 1_000).unwrap_or(DateTime::UNIX_EPOCH)
}

// Port-only L7 (etherparse 0.19 has no sp.payload on SlicedPacket)
fn detect_l7_tcp(sp: Option<u16>, dp: Option<u16>) -> L7 {
    let (sp, dp) = (sp.unwrap_or(0), dp.unwrap_or(0));
    if sp == 22 || dp == 22 {
        return L7::SSH;
    }
    if sp == 25 || dp == 25 {
        return L7::SMTP;
    }
    if sp == 110 || dp == 110 {
        return L7::POP3;
    }
    if sp == 143 || dp == 143 {
        return L7::IMAP;
    }
    if sp == 445 || dp == 445 {
        return L7::SMB;
    }
    if sp == 3389 || dp == 3389 {
        return L7::RDP;
    }
    if [80, 8080, 8000].contains(&sp) || [80, 8080, 8000].contains(&dp) {
        return L7::HTTP;
    }
    if sp == 443 || dp == 443 {
        return L7::TLS;
    }
    L7::UNKNOWN
}

fn detect_l7_udp(sp: Option<u16>, dp: Option<u16>) -> L7 {
    let (sp, dp) = (sp.unwrap_or(0), dp.unwrap_or(0));
    if sp == 53 || dp == 53 {
        return L7::DNS;
    }
    if sp == 67 || dp == 67 || sp == 68 || dp == 68 {
        return L7::DHCP;
    }
    if sp == 123 || dp == 123 {
        return L7::NTP;
    }
    if sp == 5353 || dp == 5353 {
        return L7::MDNS;
    }
    if sp == 1900 || dp == 1900 {
        return L7::SSDP;
    }
    L7::UNKNOWN
}
