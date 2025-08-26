CREATE TABLE net.packets
(
    ts            DateTime64(6, 'UTC'), -- packet timestamp, in microsecond precision
    iface         LowCardinality(String) DEFAULT '', -- interface name or ID

    -- protocols per OSI layers
    l2_proto Enum8('ETHERNET' = 1, 'ARP' = 2, 'PPPoE' = 3, 'IEEE802_11' = 4, 'OTHER' = 127),
    l3_proto Enum8('IPv4' = 1, 'IPv6' = 2, 'MPLS' = 3, 'ARP' = 4, 'NON_IP' = 127),
    l4_proto Enum16('NONE' = 0, 'ICMP' = 1, 'TCP' = 6, 'UDP' = 17, 'SCTP' = 132),
    l7_proto      LowCardinality(String) DEFAULT '',

    src_mac       FixedString(6), -- MAC addr for source
    dst_mac       FixedString(6), -- MAC addr for destination

    src_ip        IPv6, -- IP addr for source
    dst_ip        IPv6, -- IP addr for destination
    src_port      Nullable(UInt16), -- L4 port for source (if applicable)
    dst_port      Nullable(UInt16), -- L4 port for destination (if applicable)

    ip_ttl        Nullable(UInt8), -- IP TTL/Hop Limit
    tcp_flags     Nullable(UInt16), -- TCP flags bitmask

    packet_len    UInt16, -- length of entire packet (including headers)

    payload       String CODEC(LZ4), -- raw packet payload (L7 data); may be large
    payload_sha256 FixedString(32) DEFAULT '', -- SHA-256 hash of payload for deduplication/reference
    payload_ref   Nullable(String) -- external reference/URL for payload (if stored outside ClickHouse
)
    ENGINE = MergeTree
        PARTITION BY toDate(ts)
        ORDER BY (ts, src_ip, dst_ip, src_port, dst_port)
        SETTINGS index_granularity = 8192, allow_nullable_key = 1;
