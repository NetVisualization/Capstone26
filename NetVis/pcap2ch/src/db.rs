use clickhouse::Row;
use serde::Serialize;

#[derive(Debug, Serialize, Row)]
pub struct DbPacket {
    // EXACT physical column order from your DDL (packets.sql)
    pub ts: chrono::DateTime<chrono::Utc>, // 1
    pub src_ip: std::net::Ipv6Addr,        // 2
    pub dst_ip: std::net::Ipv6Addr,        // 3
    pub src_mac: [u8; 6],                  // 4 FixedString(6)
    pub dst_mac: [u8; 6],                  // 5 FixedString(6)
    pub l4_proto: i16,                     // 6 Enum16 as i16
    pub l7_proto: i16,                     // 7 Enum16 as i16
    pub src_port: Option<u16>,             // 8 Nullable(UInt16)
    pub dst_port: Option<u16>,             // 9 Nullable(UInt16)
    pub packet_len: u32,                   // 10 UInt32
    pub raw: String,                       // 11 String (non-null)
}

pub fn to_db_packet(rec: &pcap2ch::PacketRecord) -> DbPacket {
    DbPacket {
        ts: rec.ts,
        src_ip: rec.src_ip,
        dst_ip: rec.dst_ip,
        src_mac: rec.src_mac, // [u8; 6]
        dst_mac: rec.dst_mac, // [u8; 6]
        l4_proto: rec.l4_proto as i16,
        l7_proto: rec.l7_proto as i16,
        src_port: rec.src_port,
        dst_port: rec.dst_port,
        packet_len: rec.packet_len,
        raw: String::new(), // you can fill this later with payload if desired
    }
}

pub fn mac_to_hex(m: &[u8; 6]) -> String {
    // Uppercase hex without separators
    m.iter().map(|b| format!("{:02X}", b)).collect()
}

// ISO-like string with microseconds for ClickHouse toDateTime64(…, 6, 'UTC')
pub fn ts_to_str(ts: chrono::DateTime<chrono::Utc>) -> String {
    ts.format("%Y-%m-%d %H:%M:%S%.6f").to_string()
}

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
