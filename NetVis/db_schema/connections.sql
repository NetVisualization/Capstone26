CREATE DATABASE IF NOT EXISTS net;

/* DROP old undirected artifacts (safe if present) */
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
    src_ip  IPv6,
    dst_ip  IPv6,

    /* rollups */
    pkts_state       AggregateFunction(count),
    bytes_state      AggregateFunction(sum, UInt64),
    first_seen_state AggregateFunction(min, DateTime64(6, 'UTC')),
    last_seen_state  AggregateFunction(max, DateTime64(6, 'UTC')),
    l4_state         AggregateFunction(groupUniqArray, UInt8),

    /* ports and L7 from the true source/dest perspective */
    src_ports_state  AggregateFunction(groupUniqArray, UInt16),
    dst_ports_state  AggregateFunction(groupUniqArray, UInt16),

    l7_state AggregateFunction(groupUniqArray, UInt16)
)
    ENGINE = AggregatingMergeTree
        ORDER BY (src_mac, dst_mac, src_ip, dst_ip);

/* Materialized view: map each packet into its directed bucket */
CREATE MATERIALIZED VIEW net.mv_packets_to_connections
            TO net.connections_state
AS
SELECT
    minState(ts)                               AS first_seen_state,
    maxState(ts)                               AS last_seen_state,
    src_mac,
    dst_mac,
    src_ip,
    dst_ip,

    countState()                               AS pkts_state,
    sumState(toUInt64(packet_len))             AS bytes_state,
    groupUniqArrayState(toUInt8(l4_proto))     AS l4_state,

    /* collect both sides explicitly */
    groupUniqArrayStateIf(assumeNotNull(src_port), src_port IS NOT NULL) AS src_ports_state,
    groupUniqArrayStateIf(assumeNotNull(dst_port), dst_port IS NOT NULL) AS dst_ports_state,

    /* keep the exact Enum16 from packets.l7_proto */
    groupUniqArrayState(l7_proto)              AS l7_state
FROM net.packets
GROUP BY
    src_mac, dst_mac, src_ip, dst_ip;

/* User-facing directed view */
CREATE OR REPLACE VIEW net.connections AS
SELECT
    minMerge(first_seen_state)                   AS first_seen,
    maxMerge(last_seen_state)                    AS last_seen,
    src_mac,
    dst_mac,
    src_ip,
    dst_ip,
    countMerge(pkts_state)                       AS pkts,
    sumMerge(bytes_state)                        AS bytes,
    arraySort(groupUniqArrayMerge(l4_state))     AS protos,
    arraySort(arrayDistinct(groupUniqArrayMerge(src_ports_state))) AS src_ports,
    arraySort(arrayDistinct(groupUniqArrayMerge(dst_ports_state))) AS dst_ports,
    arraySort(arrayDistinct(groupUniqArrayMerge(l7_state)))        AS l7_protos
FROM net.connections_state
GROUP BY
    src_mac, dst_mac, src_ip, dst_ip;

/* Pretty formatter for MACs (same helper as before) */
CREATE FUNCTION IF NOT EXISTS format_mac AS (x) ->
    concat(
            lower(substring(hex(x), 1, 2)),  ':',
            lower(substring(hex(x), 3, 2)),  ':',
            lower(substring(hex(x), 5, 2)),  ':',
            lower(substring(hex(x), 7, 2)),  ':',
            lower(substring(hex(x), 9, 2)),  ':',
            lower(substring(hex(x),11, 2))
    );

/* Readable version with colon MACs */
CREATE OR REPLACE VIEW net.display_connections AS
SELECT
    first_seen,
    last_seen,
    format_mac(src_mac) AS src_mac,
    format_mac(dst_mac) AS dst_mac,
    src_ip,
    dst_ip,
    pkts,
    bytes,
    protos,
    src_ports,
    dst_ports,
    l7_protos
FROM net.connections;
