CREATE TABLE net.packets
(
    ts            DateTime64(6, 'UTC'),
    iface         LowCardinality(String) DEFAULT '',
    sensor_id     LowCardinality(String) DEFAULT '',

    l2_proto Enum8('ETHERNET' = 1, 'ARP' = 2, 'PPPoE' = 3, 'IEEE802_11' = 4, 'OTHER' = 127),
    l3_proto Enum8('IPv4' = 1, 'IPv6' = 2, 'MPLS' = 3, 'ARP' = 4, 'NON_IP' = 127),
    l4_proto Enum16('NONE' = 0, 'ICMP' = 1, 'TCP' = 6, 'UDP' = 17, 'SCTP' = 132),
    l7_proto      LowCardinality(String) DEFAULT '',

    src_mac       FixedString(6),
    dst_mac       FixedString(6),
    vlan_id       Nullable(UInt16),

    src_ip        IPv6,
    dst_ip        IPv6,
    src_port      Nullable(UInt16),
    dst_port      Nullable(UInt16),

    ip_ttl        Nullable(UInt8),
    tcp_flags     Nullable(UInt16),

    packet_len    UInt16,

    payload       String CODEC(LZ4),
    payload_sha256 FixedString(32) DEFAULT '',
    payload_ref   Nullable(String)
)
    ENGINE = MergeTree
        PARTITION BY toDate(ts)
        ORDER BY (ts, src_ip, dst_ip, src_port, dst_port)
        SETTINGS index_granularity = 8192, allow_nullable_key = 1;
