CREATE DATABASE IF NOT EXISTS net;

/* DROP old artifacts (safe if present) */
DROP VIEW IF EXISTS net.display_connections;
DROP VIEW IF EXISTS net.connections;
DROP VIEW IF EXISTS net.mv_packets_to_connections;
DROP TABLE IF EXISTS net.connections_state;

/* Directed state, keyed by src/dst MAC + IP */
CREATE TABLE net.connections_state
(
    /* key (directed) */
    src_mac FixedString(6),
    dst_mac FixedString(6),
    src_ip IPv6,
    dst_ip IPv6,

    /* rollups */
    pkts_state AggregateFunction(count),
    bytes_state AggregateFunction(sum, UInt64),
    first_seen_state AggregateFunction(min, DateTime64(6, 'UTC')),
    last_seen_state AggregateFunction(max, DateTime64(6, 'UTC')),
    l4_state AggregateFunction(groupUniqArray, UInt8),

    /* ports and L7 from the true source/dest perspective */
    src_ports_state AggregateFunction(groupUniqArray, UInt16),
    dst_ports_state AggregateFunction(groupUniqArray, UInt16),

    l7_state AggregateFunction(groupUniqArray, UInt16)
)
    ENGINE = AggregatingMergeTree
        ORDER BY (src_mac, dst_mac, src_ip, dst_ip);

/* Materialized view: map each packet into its directed bucket */
CREATE MATERIALIZED VIEW net.mv_packets_to_connections
            TO net.connections_state AS
SELECT
    minState(ts) AS first_seen_state,
    maxState(ts) AS last_seen_state,
    src_mac,
    dst_mac,
    src_ip,
    dst_ip,

    countState() AS pkts_state,
    sumState(toUInt64(packet_len)) AS bytes_state,
    groupUniqArrayState(toUInt8(l4_proto)) AS l4_state,

    /* collect both sides explicitly */
    groupUniqArrayStateIf(assumeNotNull(src_port), src_port IS NOT NULL) AS src_ports_state,
    groupUniqArrayStateIf(assumeNotNull(dst_port), dst_port IS NOT NULL) AS dst_ports_state,

    /* keep the exact Enum16 from packets.l7_proto */
    groupUniqArrayState(l7_proto) AS l7_state
FROM net.packets
GROUP BY src_mac, dst_mac, src_ip, dst_ip;

/* User-facing directed view */
CREATE OR REPLACE VIEW net.connections AS
SELECT
    minMerge(first_seen_state) AS first_seen,
    maxMerge(last_seen_state) AS last_seen,

    src_mac,
    dst_mac,
    src_ip,
    dst_ip,

    /* Vendor enrichment (computed from MAC) — OUI as UInt32 */
    dictGetUInt32OrDefault(
            'net.oui_dict', 'vendor_id',
            (
                toUInt32(reinterpretAsUInt8(substring(src_mac, 1, 1))) * 65536 +
                toUInt32(reinterpretAsUInt8(substring(src_mac, 2, 1))) * 256 +
                toUInt32(reinterpretAsUInt8(substring(src_mac, 3, 1)))
                ),
            0
    ) AS src_vendor_id,

    dictGetUInt32OrDefault(
            'net.oui_dict', 'vendor_id',
            (
                toUInt32(reinterpretAsUInt8(substring(dst_mac, 1, 1))) * 65536 +
                toUInt32(reinterpretAsUInt8(substring(dst_mac, 2, 1))) * 256 +
                toUInt32(reinterpretAsUInt8(substring(dst_mac, 3, 1)))
                ),
            0
    ) AS dst_vendor_id,

    dictGetStringOrDefault(
            'net.oui_dict', 'vendor',
            (
                toUInt32(reinterpretAsUInt8(substring(src_mac, 1, 1))) * 65536 +
                toUInt32(reinterpretAsUInt8(substring(src_mac, 2, 1))) * 256 +
                toUInt32(reinterpretAsUInt8(substring(src_mac, 3, 1)))
                ),
            'Unknown'
    ) AS src_vendor,

    dictGetStringOrDefault(
            'net.oui_dict', 'vendor',
            (
                toUInt32(reinterpretAsUInt8(substring(dst_mac, 1, 1))) * 65536 +
                toUInt32(reinterpretAsUInt8(substring(dst_mac, 2, 1))) * 256 +
                toUInt32(reinterpretAsUInt8(substring(dst_mac, 3, 1)))
                ),
            'Unknown'
    ) AS dst_vendor,

    countMerge(pkts_state) AS pkts,
    sumMerge(bytes_state) AS bytes,
    arraySort(groupUniqArrayMerge(l4_state)) AS protos,
    arraySort(arrayDistinct(groupUniqArrayMerge(src_ports_state))) AS src_ports,
    arraySort(arrayDistinct(groupUniqArrayMerge(dst_ports_state))) AS dst_ports,
    arraySort(arrayDistinct(groupUniqArrayMerge(l7_state))) AS l7_protos
FROM net.connections_state
GROUP BY src_mac, dst_mac, src_ip, dst_ip;

/* Readable version with colon MACs (inline formatting; no CREATE FUNCTION) */
CREATE OR REPLACE VIEW net.display_connections AS
SELECT
    first_seen,
    last_seen,

    concat(
            lower(substring(hex(src_mac), 1, 2)), ':',
            lower(substring(hex(src_mac), 3, 2)), ':',
            lower(substring(hex(src_mac), 5, 2)), ':',
            lower(substring(hex(src_mac), 7, 2)), ':',
            lower(substring(hex(src_mac), 9, 2)), ':',
            lower(substring(hex(src_mac), 11, 2))
    ) AS src_mac,

    concat(
            lower(substring(hex(dst_mac), 1, 2)), ':',
            lower(substring(hex(dst_mac), 3, 2)), ':',
            lower(substring(hex(dst_mac), 5, 2)), ':',
            lower(substring(hex(dst_mac), 7, 2)), ':',
            lower(substring(hex(dst_mac), 9, 2)), ':',
            lower(substring(hex(dst_mac), 11, 2))
    ) AS dst_mac,

    src_vendor_id,
    dst_vendor_id,
    src_vendor,
    dst_vendor,

    src_ip,
    dst_ip,
    pkts,
    bytes,
    protos,
    src_ports,
    dst_ports,
    l7_protos
FROM net.connections;
