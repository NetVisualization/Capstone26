use etherparse::TransportSlice;

/// DNS/mDNS minimal summary for Wireshark-like "Info"
fn try_mdns_summary(payload: &[u8]) -> Option<String> {
    use dns_parser::Packet;
    if let Ok(pkt) = Packet::parse(payload) {
        if pkt.header.query {
            if let Some(q) = pkt.questions.first() {
                return Some(format!("Standard query {} {:?}", q.qname, q.qtype));
            }
        } else if let Some(ans) = pkt.answers.first() {
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
pub fn summarize_packet(sp: &etherparse::SlicedPacket<'_>) -> String {
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
