-- Per-IP totals and window; optional label stays for later enrichment
CREATE TABLE net.nodes
(
    ip IPv6,

    num_packets_state     AggregateFunction(count),
    num_bytes_state       AggregateFunction(sum, UInt64),
    first_seen_state      AggregateFunction(min, DateTime64(6, 'UTC')),
    last_seen_state       AggregateFunction(max, DateTime64(6, 'UTC')),
    num_unique_peers_state AggregateFunction(uniqCombined, IPv6),

    device_type LowCardinality(Nullable(String)) DEFAULT NULL
)
    ENGINE = AggregatingMergeTree
        ORDER BY (ip);

-- Build nodes from connections (fast degree + time window + counts/bytes)
CREATE MATERIALIZED VIEW net.mv_connections_to_nodes
            TO net.nodes
AS
-- A side
SELECT
    node_a AS ip,
    countMergeState(num_packets_state)               AS num_packets_state,
    sumMergeState(num_bytes_state)                   AS num_bytes_state,
    minMergeState(first_seen_state)                  AS first_seen_state,
    maxMergeState(last_seen_state)                   AS last_seen_state,
    uniqCombinedState(node_b)                        AS num_unique_peers_state,
    CAST(NULL AS Nullable(String))                   AS device_type
FROM net.connections
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
    CAST(NULL AS Nullable(String))                   AS device_type
FROM net.connections
GROUP BY ip;

-- Optional: direct safety net from packets; helps when connections lag
CREATE MATERIALIZED VIEW IF NOT EXISTS net.mv_packets_to_nodes
            TO net.nodes
AS
SELECT
    src_ip AS ip,
    countState()                     AS num_packets_state,
    sumState(toUInt64(packet_len))   AS num_bytes_state,
    minState(ts)                     AS first_seen_state,
    maxState(ts)                     AS last_seen_state,
    uniqCombinedState(dst_ip)        AS num_unique_peers_state,
    CAST(NULL AS Nullable(String))   AS device_type
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
    CAST(NULL AS Nullable(String))   AS device_type
FROM net.packets
GROUP BY ip;
