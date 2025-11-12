//! Core types & helpers for the packet parser.
//! No IO here—just pure data transformations.

use chrono::{DateTime, TimeZone, Utc};
use serde::{Deserialize, Serialize};
use std::net::{Ipv4Addr, Ipv6Addr};

/// L4 protocol (values align with your ClickHouse Enum16 for l4_proto).
#[derive(Debug, Copy, Clone, Serialize, Deserialize)]
pub enum L4Proto {
    None = 0,
    Icmp = 1, // ICMPv4
    Tcp = 6,
    Udp = 17,
    Icmpv6 = 58,
    Sctp = 132,
}
impl Default for L4Proto {
    fn default() -> Self {
        L4Proto::None
    }
}

/// L7 application protocol (values align with your Enum16 in net.packets).
#[derive(Debug, Copy, Clone, Serialize, Deserialize)]
pub enum L7Proto {
    UNKNOWN = 0,
    SSH = 22,
    SMTP = 25,
    DNS = 53,
    DHCP = 67,
    HTTP = 80,
    POP3 = 110,
    NTP = 123,
    IMAP = 143,
    TLS = 443,
    SMB = 445,
    QUIC = 1000,
    SSDP = 1900,
    RDP = 3389,
    MDNS = 5353,
}
impl Default for L7Proto {
    fn default() -> Self {
        L7Proto::UNKNOWN
    }
}

/// Neutral, schema-aligned packet record (no DB client types).
/// This maps 1:1 to columns in `net.packets`.
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct PacketRecord {
    /// UTC timestamp with microsecond precision
    pub ts: DateTime<Utc>,

    /// IPv6 addresses (IPv4 must be written as v4-mapped ::ffff:a.b.c.d)
    pub src_ip: Ipv6Addr,
    pub dst_ip: Ipv6Addr,

    /// Raw MAC bytes (exactly 6 bytes each). These go straight to FixedString(6).
    pub src_mac: [u8; 6],
    pub dst_mac: [u8; 6],

    pub l4_proto: L4Proto,
    pub l7_proto: L7Proto,

    /// Nullable ports — use None when not TCP/UDP
    pub src_port: Option<u16>,
    pub dst_port: Option<u16>,

    /// Captured packet length (in bytes)
    pub packet_len: u32,

    /// Optional info payload/frame (leave empty unless you choose to store it)
    pub info: Option<Vec<u8>>,
}

impl Default for PacketRecord {
    fn default() -> Self {
        Self {
            // epoch (UTC) with 0 µs
            ts: chrono::Utc.timestamp_opt(0, 0).single().unwrap(),

            // :: (unspecified) for both IPs
            src_ip: std::net::Ipv6Addr::UNSPECIFIED,
            dst_ip: std::net::Ipv6Addr::UNSPECIFIED,

            // all-zero MACs
            src_mac: [0u8; 6],
            dst_mac: [0u8; 6],

            l4_proto: L4Proto::None,
            l7_proto: L7Proto::UNKNOWN,

            src_port: None,
            dst_port: None,

            packet_len: 0,
            info: None,
        }
    }
}

/// Convert (sec, micros) into UTC DateTime with microsecond precision.
/// Used for classic pcap and for normalizing pcapng/live captures to µs.
pub fn ts_sec_usec_to_dt64_us(sec: i64, usec: u32) -> DateTime<Utc> {
    // (usec * 1000) converts µs → ns for chrono
    Utc.timestamp_opt(sec, usec.saturating_mul(1_000))
        .single()
        .unwrap_or_else(|| Utc.timestamp_opt(0, 0).unwrap())
}

/// Convert a timestamp with base 10^-resolution to UTC µs.
/// For pcapng: ticks are fractional-second units with 10^-res precision.
/// Example: res=6 means ticks are microseconds.
pub fn ts_pow10_to_dt64_us(ticks: u64, res_pow10: u8) -> DateTime<Utc> {
    let base = 10u64.saturating_pow(res_pow10 as u32);
    let secs = (ticks / base) as i64;
    let sub = (ticks % base) as u32;

    // Convert sub to nanoseconds (pad/truncate as necessary)
    let nanos: u32 = if res_pow10 == 9 {
        sub // already ns
    } else if res_pow10 < 9 {
        // scale up (e.g., µs→ns: * 1000)
        let factor = 10u32.pow(9 - res_pow10 as u32);
        sub.saturating_mul(factor)
    } else {
        // res > 9 (extremely rare): scale down with rounding
        let divisor = 10u32.pow(res_pow10 as u32 - 9);
        (sub as f64 / divisor as f64).round() as u32
    };

    Utc.timestamp_opt(secs, nanos)
        .single()
        .unwrap_or_else(|| Utc.timestamp_opt(0, 0).unwrap())
}

/// Map IPv4 address to IPv6 v4-mapped form (::ffff:a.b.c.d).
pub fn v4_to_v6_mapped(v4: Ipv4Addr) -> Ipv6Addr {
    v4.to_ipv6_mapped()
}

/// Port → L7 mapping aligned to your Enum16.
/// Keep this small and deterministic; we’ll extend later if needed.
pub fn classify_l7(port_opt: Option<u16>, l4: L4Proto) -> L7Proto {
    use L4Proto::*;
    use L7Proto::*;
    match l4 {
        Tcp | Udp => match port_opt {
            Some(22) => SSH,
            Some(25) => SMTP,
            Some(53) => DNS,
            Some(67) => DHCP,
            Some(80) => HTTP,
            Some(110) => POP3,
            Some(123) => NTP,
            Some(143) => IMAP,
            Some(443) => TLS,
            Some(445) => SMB,
            Some(1000) => QUIC,
            Some(1900) => SSDP,
            Some(3389) => RDP,
            Some(5353) => MDNS,
            _ => UNKNOWN,
        },
        Icmp | Icmpv6 | None | Sctp => UNKNOWN,
    }
}

/// Utility: format 6-byte MAC as lowercase hex (for logs/diagnostics only).
pub fn mac_hex(mac: &[u8; 6]) -> String {
    let mut s = String::with_capacity(12);
    for b in mac {
        use std::fmt::Write as _;
        let _ = write!(&mut s, "{:02x}", b);
    }
    s
}
