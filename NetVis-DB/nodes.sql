-- === net.nodes as a finalized VIEW over a state table ===

CREATE DATABASE IF NOT EXISTS net;

-- Clean up any previous objects
DROP VIEW IF EXISTS net.nodes;
DROP TABLE IF EXISTS net.nodes;
DROP TABLE IF EXISTS net.nodes_state;
DROP VIEW IF EXISTS net.mv_connections_to_nodes;
DROP VIEW IF EXISTS net.mv_packets_to_nodes;

-- Raw state table (per-IP rollups)
CREATE TABLE net.nodes_state
(
    ip IPv6,

    num_packets_state      AggregateFunction(count),
    num_bytes_state        AggregateFunction(sum, UInt64),
    first_seen_state       AggregateFunction(min, DateTime64(6, 'UTC')),
    last_seen_state        AggregateFunction(max, DateTime64(6, 'UTC')),
    num_unique_peers_state AggregateFunction(uniqCombined, IPv6),

    device_type LowCardinality(Nullable(String)) DEFAULT NULL
)
    ENGINE = AggregatingMergeTree
        ORDER BY (ip);

-- MV: connections_state -> nodes_state  (degree/time window/volumes)
CREATE MATERIALIZED VIEW net.mv_connections_to_nodes
            TO net.nodes_state
AS
-- A side
SELECT
    node_a AS ip,
    countMergeState(num_packets_state)  AS num_packets_state,
    sumMergeState(num_bytes_state)      AS num_bytes_state,
    minMergeState(first_seen_state)     AS first_seen_state,
    maxMergeState(last_seen_state)      AS last_seen_state,
    uniqCombinedState(node_b)           AS num_unique_peers_state,
    CAST(NULL AS Nullable(String))      AS device_type
FROM net.connections_state
GROUP BY ip

UNION ALL

-- B side
SELECT
    node_b AS ip,
    countMergeState(num_packets_state),
    sumMergeState(num_bytes_state),
    minMergeState(first_seen_state),
    maxMergeState(last_seen_state),
    uniqCombinedState(node_a),
    CAST(NULL AS Nullable(String))      AS device_type
FROM net.connections_state
GROUP BY ip;

-- Optional safety net: packets -> nodes_state (helps if connections lag)
CREATE MATERIALIZED VIEW net.mv_packets_to_nodes
            TO net.nodes_state
AS
SELECT
    src_ip AS ip,
    countState()                    AS num_packets_state,
    sumState(toUInt64(packet_len))  AS num_bytes_state,
    minState(ts)                    AS first_seen_state,
    maxState(ts)                    AS last_seen_state,
    uniqCombinedState(dst_ip)       AS num_unique_peers_state,
    CAST(NULL AS Nullable(String))  AS device_type
FROM net.packets
GROUP BY ip

UNION ALL
SELECT
    dst_ip AS ip,
    countState(),
    sumState(toUInt64(packet_len)),
    minState(ts),
    maxState(ts),
    uniqCombinedState(src_ip),
    CAST(NULL AS Nullable(String))  AS device_type
FROM net.packets
GROUP BY ip;

-- Finalized view with human-readable columns
CREATE OR REPLACE VIEW net.nodes AS
SELECT
    ip,
    countMerge(num_packets_state)                AS pkts,
    sumMerge(num_bytes_state)                    AS bytes,
    minMerge(first_seen_state)                   AS first_seen,
    maxMerge(last_seen_state)                    AS last_seen,
    uniqCombinedMerge(num_unique_peers_state)    AS degree,
    device_type
FROM net.nodes_state
GROUP BY ip, device_type;
