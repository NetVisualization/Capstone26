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

    /* OUI (first 3 bytes) */
    src_oui_u32 UInt32 MATERIALIZED
        (
            toUInt32(reinterpretAsUInt8(substring(src_mac, 1, 1))) * 65536 +
            toUInt32(reinterpretAsUInt8(substring(src_mac, 2, 1))) * 256 +
            toUInt32(reinterpretAsUInt8(substring(src_mac, 3, 1)))
            ),

    dst_oui_u32 UInt32 MATERIALIZED
        (
            toUInt32(reinterpretAsUInt8(substring(dst_mac, 1, 1))) * 65536 +
            toUInt32(reinterpretAsUInt8(substring(dst_mac, 2, 1))) * 256 +
            toUInt32(reinterpretAsUInt8(substring(dst_mac, 3, 1)))
            ),



    /* Vendor enrichment (requires net.oui_dict to exist) */
    src_vendor_id UInt32 MATERIALIZED dictGetUInt32OrDefault('net.oui_dict', 'vendor_id', src_oui_u32, 0),
    dst_vendor_id UInt32 MATERIALIZED dictGetUInt32OrDefault('net.oui_dict', 'vendor_id', dst_oui_u32, 0),

    src_vendor String MATERIALIZED dictGetStringOrDefault('net.oui_dict', 'vendor', src_oui_u32, 'Unknown'),
    dst_vendor String MATERIALIZED dictGetStringOrDefault('net.oui_dict', 'vendor', dst_oui_u32, 'Unknown'),

    l4_proto Enum16('NONE' = 0, 'ICMP' = 1, 'TCP' = 6, 'UDP' = 17, 'SCTP' = 132),

    l7_proto UInt16 DEFAULT 0,

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

    /* Vendor fields exposed for frontend */
    src_vendor_id,
    dst_vendor_id,
    src_vendor,
    dst_vendor,

    l4_proto,
    l7_proto,
    src_port,
    dst_port,
    packet_len,
    info
FROM net.packets;
