CREATE DATABASE IF NOT EXISTS net;

DROP TABLE IF EXISTS net.packets;

CREATE TABLE net.packets
(
    ts         DateTime64(6, 'UTC'),
    src_ip     IPv6,
    dst_ip     IPv6,
    src_mac    String,
    dst_mac    String,

    l4_proto   Enum16('NONE' = 0, 'ICMP' = 1, 'TCP' = 6, 'UDP' = 17, 'SCTP' = 132),

    l7_proto Enum16(
        'UNKNOWN' = 0,
        'HTTP'    = 80,
        'TLS'     = 443,   -- generic TLS (HTTPS/DoT/etc.)
        'DNS'     = 53,
        'MDNS'    = 5353,
        'SSDP'    = 1900,
        'DHCP'    = 67,    -- 67/68
        'NTP'     = 123,
        'SSH'     = 22,
        'SMTP'    = 25,
        'IMAP'    = 143,
        'POP3'    = 110,
        'QUIC'    = 1000,  -- custom code to avoid port collisions
        'SMB'     = 445,
        'RDP'     = 3389
        ) DEFAULT 'UNKNOWN',

    src_port   Nullable(UInt16),
    dst_port   Nullable(UInt16),
    packet_len UInt32,
    raw        String CODEC(ZSTD(6))

)
    ENGINE = MergeTree
        PARTITION BY toDate(ts)
        ORDER BY (ts, src_ip, dst_ip)
        SETTINGS index_granularity = 8192;