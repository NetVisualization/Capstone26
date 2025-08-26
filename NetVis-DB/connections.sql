-- NODE_A

-- NODE_B

-- NUM_PACKETS

-- PORTS (Array)

-- PROTOCOLS (TCP = 6, UDP = 17, ICMP = 1, etc.)

-- LAST_SEEN (DateTime)

CREATE TABLE net.connections
(
    -- core timing
    first_seen    DateTime64(6, 'UTC'),
    last_seen     DateTime64(6, 'UTC'),

    -- capture/source metadata (optional but useful)
    iface         LowCardinality(String)      DEFAULT '',

    -- network & transport (store IPs as IPv6; upcast IPv4 -> IPv6)
    node_a        IPv6,
    node_b        IPv6,
    ports         Array(UInt16),               -- all observed ports for this connection
    protocols     Array(UInt16),               -- all observed L4 protocols for this connection

    num_packets   UInt32,                      -- total packets observed in this connection

    -- helpful header bits (optional, keep nullable)
    ip_ttl_a      Nullable(UInt8),             -- TTL from node_a to node_b
    ip_ttl_b      Nullable(UInt8),             -- TTL from node_b to node_a

    tcp_flags_a   Nullable(UInt16),            -- aggregated TCP flags from node_a to node_b
    tcp_flags_b   Nullable(UInt16),            -- aggregated TCP flags from node_b to node_a

    -- sizes
    total_bytes_a UInt64,                      -- total bytes from node_a to node_b
    total_bytes_b UInt64,                      -- total bytes from node_b to node_a

    total_payload_bytes_a UInt64,              -- total payload bytes from node_a to node_b
    total_payload_bytes_b UInt64               -- total payload bytes from node_b to node_a
)
    ENGINE = MergeTree
        PARTITION BY toDate(first_seen)
        ORDER BY (node_a, node_b, first_seen)
        SETTINGS index_granularity = 8192, allow_nullable_key = 1;

