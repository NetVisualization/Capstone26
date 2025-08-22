CREATE TABLE net.packets
(
    -- core timing
    ts            DateTime64(6, 'UTC'),

    -- capture/source metadata (optional but useful)
    iface         LowCardinality(String)      DEFAULT '',
    sensor_id     LowCardinality(String)      DEFAULT '',

    -- protocol layering
    l2_proto Enum8(
        'ETHERNET' = 1, 'ARP' = 2, 'PPPoE' = 3, 'IEEE802_11' = 4, 'OTHER' = 127
        ),
    l3_proto Enum8(
        'IPv4' = 1, 'IPv6' = 2, 'MPLS' = 3, 'ARP' = 4, 'NON_IP' = 127
        ),
    l4_proto Enum16(
        'NONE' = 0, 'ICMP' = 1, 'TCP' = 6, 'UDP' = 17, 'SCTP' = 132
        ),
    -- application “protocol” label (flexible)
    l7_proto      LowCardinality(String)      DEFAULT '',

    -- link layer
    src_mac       FixedString(6),
    dst_mac       FixedString(6),
    vlan_id       Nullable(UInt16),

    -- network & transport (store IPs as IPv6; upcast IPv4 -> IPv6)
    src_ip        IPv6,
    dst_ip        IPv6,
    src_port      Nullable(UInt16),
    dst_port      Nullable(UInt16),

    -- helpful header bits (optional, keep nullable)
    ip_ttl        Nullable(UInt8),
    ip_tos        Nullable(UInt8),     -- DSCP/ECN packed
    tcp_flags     Nullable(UInt8),

    -- sizes
    packet_len    UInt16,              -- fits up to 65535 (pcap max)

    -- payload handling
    payload       String CODEC(ZSTD(9)),          -- binary-safe raw bytes
    payload_sha256 FixedString(32) DEFAULT '',    -- hex digest for dedup/lookup
    payload_ref   Nullable(String)                -- e.g., S3/MinIO key if externalized
)
    ENGINE = MergeTree
        PARTITION BY toDate(ts)
        ORDER BY (ts, src_ip, dst_ip, src_port, dst_port)
        SETTINGS index_granularity = 8192, allow_nullable_key = 1;
