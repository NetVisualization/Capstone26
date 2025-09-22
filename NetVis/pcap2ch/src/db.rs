use clickhouse::Row;
use serde::Serialize;

#[derive(Debug, Serialize, Row)]
pub struct DbPacket {
    // Physical column order from your DDL (packets.sql)
    pub ts: chrono::DateTime<chrono::Utc>,
    pub src_ip: std::net::Ipv6Addr,
    pub dst_ip: std::net::Ipv6Addr,
    pub src_mac: [u8; 6], // FixedString(6)
    pub dst_mac: [u8; 6], // FixedString(6)
    pub l4_proto: i16,    // Enum16 value
    pub l7_proto: i16,    // Enum16 value
    pub src_port: Option<u16>,
    pub dst_port: Option<u16>,
    pub packet_len: u32,
    pub raw: String, // String (non-null)
}

impl DbPacket {
    pub fn from_record_with_raw(rec: &pcap2ch::PacketRecord, raw: String) -> Self {
        Self {
            ts: rec.ts,
            src_ip: rec.src_ip,
            dst_ip: rec.dst_ip,
            src_mac: rec.src_mac,
            dst_mac: rec.dst_mac,
            l4_proto: rec.l4_proto as i16,
            l7_proto: rec.l7_proto as i16,
            src_port: rec.src_port,
            dst_port: rec.dst_port,
            packet_len: rec.packet_len,
            raw,
        }
    }
}

/* ---------- Helpers used by the INSERT VALUES builder ---------- */

pub fn mac_to_hex(m: &[u8; 6]) -> String {
    m.iter().map(|b| format!("{:02X}", b)).collect()
}

pub fn ts_to_str(ts: chrono::DateTime<chrono::Utc>) -> String {
    ts.format("%Y-%m-%d %H:%M:%S%.6f").to_string()
}

// We send Enum16s by label in the VALUES SQL; map numeric -> label:
pub fn l4_label_from_code(v: i16) -> &'static str {
    match v {
        0 => "NONE",
        1 => "ICMP",
        6 => "TCP",
        17 => "UDP",
        132 => "SCTP",
        _ => "NONE",
    }
}

pub fn l7_label_from_code(v: i16) -> &'static str {
    match v {
        0 => "UNKNOWN",
        22 => "SSH",
        25 => "SMTP",
        53 => "DNS",
        67 => "DHCP",
        80 => "HTTP",
        110 => "POP3",
        123 => "NTP",
        143 => "IMAP",
        443 => "TLS",
        445 => "SMB",
        1000 => "QUIC",
        1900 => "SSDP",
        3389 => "RDP",
        5353 => "MDNS",
        _ => "UNKNOWN",
    }
}
