-- Finalized connections view over a state table (MACs stored as hex strings)

CREATE DATABASE IF NOT EXISTS net;

DROP VIEW  IF EXISTS net.connections;
DROP TABLE IF EXISTS net.connections_state;
DROP VIEW  IF EXISTS net.mv_packets_to_connections;

CREATE TABLE net.connections_state
(
    node_a IPv6,  -- min(src_ip, dst_ip)
    node_b IPv6,  -- max(src_ip, dst_ip)

    num_packets_state AggregateFunction(count),
    num_bytes_state   AggregateFunction(sum, UInt64),
    first_seen_state  AggregateFunction(min, DateTime64(6, 'UTC')),
    last_seen_state   AggregateFunction(max, DateTime64(6, 'UTC')),
    protos_state      AggregateFunction(groupUniqArray, UInt8),

    -- MACs are hex strings now
    node_a_macs_state AggregateFunction(groupUniqArray, FixedString(12)),
    node_b_macs_state AggregateFunction(groupUniqArray, FixedString(12))
)
    ENGINE = AggregatingMergeTree
        ORDER BY (node_a, node_b);

CREATE MATERIALIZED VIEW net.mv_packets_to_connections
            TO net.connections_state
AS
SELECT
    if(src_ip <= dst_ip, src_ip, dst_ip) AS node_a,
    if(src_ip <= dst_ip, dst_ip, src_ip) AS node_b,

    countState()                           AS num_packets_state,
    sumState(toUInt64(packet_len))         AS num_bytes_state,
    minState(ts)                           AS first_seen_state,
    maxState(ts)                           AS last_seen_state,
    groupUniqArrayState(toUInt8(l4_proto)) AS protos_state,

    -- carry hex strings to the correct side
    groupUniqArrayState( if(src_ip <= dst_ip, src_mac, dst_mac) ) AS node_a_macs_state,
    groupUniqArrayState( if(src_ip <= dst_ip, dst_mac, src_mac) ) AS node_b_macs_state
FROM net.packets
GROUP BY node_a, node_b;

CREATE OR REPLACE VIEW net.connections AS
SELECT
    node_a,
    node_b,
    countMerge(num_packets_state)                 AS pkts,
    sumMerge(num_bytes_state)                     AS bytes,
    minMerge(first_seen_state)                    AS first_seen,
    maxMerge(last_seen_state)                     AS last_seen,
    arraySort(groupUniqArrayMerge(protos_state))  AS protos,
    arraySort(arrayDistinct(groupUniqArrayMerge(node_a_macs_state))) AS node_a_macs,
    arraySort(arrayDistinct(groupUniqArrayMerge(node_b_macs_state))) AS node_b_macs
FROM net.connections_state
GROUP BY node_a, node_b;
