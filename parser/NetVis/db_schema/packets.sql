CREATE DATABASE IF NOT EXISTS net;

DROP TABLE IF EXISTS net.packets;

CREATE TABLE net.packets
(
    packet_id UUID DEFAULT generateUUIDv4(),
    ts DateTime64(6, 'UTC'),
    src_ip IPv6,
    dst_ip IPv6,
    src_mac FixedString(6),
    dst_mac FixedString(6),

    l4_proto Enum16('NONE' = 0, 'ICMP' = 1, 'TCP' = 6, 'UDP' = 17, 'SCTP' = 132),

    l7_proto UInt16 DEFAULT 0,

    src_vendor LowCardinality(String) DEFAULT 'Unknown',
    dst_vendor LowCardinality(String) DEFAULT 'Unknown',

    src_port Nullable(UInt16),
    dst_port Nullable(UInt16),
    packet_len UInt32,
    info String DEFAULT '' CODEC(ZSTD(6))

)
    ENGINE = MergeTree
        PARTITION BY toDate(ts)
        ORDER BY (ts, src_ip, dst_ip, packet_id)
        SETTINGS index_granularity = 8192;

/* Helper to render FixedString(6) -> "aa:bb:cc:dd:ee:ff" */
CREATE FUNCTION IF NOT EXISTS format_mac AS (x) ->
    concat(
            lower(substring(hex(x), 1, 2)), ':',
            lower(substring(hex(x), 3, 2)), ':',
            lower(substring(hex(x), 5, 2)), ':',
            lower(substring(hex(x), 7, 2)), ':',
            lower(substring(hex(x), 9, 2)), ':',
            lower(substring(hex(x),11, 2))
    );

/* Readability view for dashboards & ad-hoc queries */
CREATE OR REPLACE VIEW net.display_packets AS
SELECT
    packet_id,
    ts,
    src_ip,
    dst_ip,
    format_mac(src_mac) AS src_mac,
    format_mac(dst_mac) AS dst_mac,
    l4_proto,
    l7_proto,
    src_vendor,
    dst_vendor,
    src_port,
    dst_port,
    packet_len,
    info
FROM net.packets;

