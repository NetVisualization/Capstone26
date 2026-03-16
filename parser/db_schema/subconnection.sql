CREATE DATABASE IF NOT EXISTS net;

/* DROP old undirected artifacts (safe if present) */
DROP VIEW IF EXISTS net.display_connections;
DROP VIEW IF EXISTS net.connections;
DROP VIEW IF EXISTS net.mv_packets_to_connections;
DROP TABLE IF EXISTS net.connections_state;

CREATE TABLE IF NOT EXISTS net.subconnections_state
(
    src_mac FixedString(6),
    dst_mac FixedString(6),
    src_ip IPv6,
    dst_ip IPv6,
    l4_proto UInt8,
    l7_proto UInt16,

    pkts_state AggregateFunction(count),
    bytes_state AggregateFunction(sum, UInt64),
    first_seen_state AggregateFunction(min, DateTime64(6, 'UTC')),
    last_seen_state AggregateFunction(max, DateTime64(6, 'UTC')),

    src_ports_state AggregateFunction(groupUniqArray, UInt16),
    dst_ports_state AggregateFunction(groupUniqArray, UInt16)
)
    ENGINE = AggregatingMergeTree
        ORDER BY (src_mac, dst_mac, src_ip, dst_ip, l4_proto, l7_proto);

CREATE MATERIALIZED VIEW IF NOT EXISTS net.mv_packets_to_subconnections
            TO net.subconnections_state AS
SELECT
    minState(ts) AS first_seen_state,
    maxState(ts) AS last_seen_state,
    src_mac,
    dst_mac,
    src_ip,
    dst_ip,
    toUInt8(l4_proto) AS l4_proto,
    l7_proto AS l7_proto,

    countState() AS pkts_state,
    sumState(toUInt64(packet_len)) AS bytes_state,

    groupUniqArrayStateIf(assumeNotNull(src_port), src_port IS NOT NULL) AS src_ports_state,
    groupUniqArrayStateIf(assumeNotNull(dst_port), dst_port IS NOT NULL) AS dst_ports_state
FROM net.packets
GROUP BY src_mac, dst_mac, src_ip, dst_ip, l4_proto, l7_proto;

CREATE OR REPLACE VIEW net.subconnections AS
SELECT
    minMerge(first_seen_state) AS first_seen,
    maxMerge(last_seen_state) AS last_seen,
    src_mac,
    dst_mac,
    src_ip,
    dst_ip,
    l4_proto,
    l7_proto,
    countMerge(pkts_state) AS pkts,
    sumMerge(bytes_state) AS bytes,
    arraySort(arrayDistinct(groupUniqArrayMerge(src_ports_state))) AS src_ports,
    arraySort(arrayDistinct(groupUniqArrayMerge(dst_ports_state))) AS dst_ports
FROM net.subconnections_state
GROUP BY src_mac, dst_mac, src_ip, dst_ip, l4_proto, l7_proto;

CREATE OR REPLACE VIEW net.display_subconnections AS
SELECT
    first_seen,
    last_seen,
    format_mac(src_mac) AS src_mac,
    format_mac(dst_mac) AS dst_mac,
    src_ip,
    dst_ip,
    l4_proto,
    l7_proto,
    pkts,
    bytes,
    src_ports,
    dst_ports
FROM net.subconnections;