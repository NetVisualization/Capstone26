CREATE TABLE net.nodes
(
    ip IPv6,

    -- packet/time rollups
    num_packets_state  AggregateFunction(count),
    first_seen_state   AggregateFunction(min, DateTime64(6, 'UTC')),
    last_seen_state    AggregateFunction(max, DateTime64(6, 'UTC')),

    -- unique peers (exact cardinality is expensive; uniqCombined is efficient)
    num_connections_state AggregateFunction(uniqCombined, IPv6),

    -- which L4 protocols this node used across all connections
    l4_protos_state    AggregateFunction(groupUniqArray, UInt8),

    -- per-protocol port sets (all ports this node used, regardless of side)
    tcp_ports_state    AggregateFunction(groupUniqArray, UInt16),
    udp_ports_state    AggregateFunction(groupUniqArray, UInt16),

    -- optional label you can backfill/update later
    device_type        LowCardinality(String) DEFAULT ''
)
    ENGINE = AggregatingMergeTree
        ORDER BY (ip);


CREATE MATERIALIZED VIEW net.mv_connections_to_nodes
            TO net.nodes
AS
-- From A side
SELECT
    node_a AS ip,

    countMergeState(num_packets_state)           AS num_packets_state,   -- state → state
    minMergeState(first_seen_state)              AS first_seen_state,
    maxMergeState(last_seen_state)               AS last_seen_state,

    uniqCombinedState(node_b)                    AS num_connections_state, -- new state

    groupUniqArrayMergeState(protos_state)       AS l4_protos_state,

    groupUniqArrayMergeState(tcp_src_ports_state) AS tcp_ports_state,
    groupUniqArrayMergeState(udp_src_ports_state) AS udp_ports_state,

    '' AS device_type
FROM net.connections
GROUP BY ip

UNION ALL

-- From B side
SELECT
    node_b AS ip,

    countMergeState(num_packets_state)           AS num_packets_state,
    minMergeState(first_seen_state)              AS first_seen_state,
    maxMergeState(last_seen_state)               AS last_seen_state,

    uniqCombinedState(node_a)                    AS num_connections_state,

    groupUniqArrayMergeState(protos_state)       AS l4_protos_state,

    groupUniqArrayMergeState(tcp_dst_ports_state) AS tcp_ports_state,
    groupUniqArrayMergeState(udp_dst_ports_state) AS udp_ports_state,

    '' AS device_type
FROM net.connections
GROUP BY ip;
