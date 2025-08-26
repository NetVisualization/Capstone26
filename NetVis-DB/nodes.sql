CREATE TABLE net.nodes
(
    ip IPv6, -- IP address of the node
    macs_state AggregateFunction(groupUniqArray, FixedString(6)), -- known MAC addresses related to IP

    -- packet/time rollups
    num_packets_state  AggregateFunction(count), -- total packets seen with IP
    first_seen_state   AggregateFunction(min, DateTime64(6, 'UTC')), -- time of first packet seen with IP
    last_seen_state    AggregateFunction(max, DateTime64(6, 'UTC')), -- time of last packet seen with IP

    -- unique peers (exact cardinality is expensive; uniqCombined is efficient)
    num_connections_state AggregateFunction(uniqCombined, IPv6), -- number of unique IPs node communicated with

    -- which L4 protocols this node used across all connections
    l4_protos_state    AggregateFunction(groupUniqArray, UInt8), -- all L4 protocols seen with IP

    -- per-protocol port sets (all ports this node used, regardless of side)
    tcp_ports_state    AggregateFunction(groupUniqArray, UInt16), -- all TCP ports seen with IP
    udp_ports_state    AggregateFunction(groupUniqArray, UInt16), -- all UDP ports seen with IP

    -- optional label you can backfill/update later
    device_type        LowCardinality(String) DEFAULT '' -- e.g. 'desktop', 'mobile', 'server', etc. (if known)
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

CREATE MATERIALIZED VIEW IF NOT EXISTS net.mv_packets_to_nodes_macs
            TO net.nodes (ip, macs_state)
AS
-- source side
SELECT
    src_ip AS ip,
    groupUniqArrayState(src_mac) AS macs_state
FROM net.packets
GROUP BY ip

UNION ALL
-- dest side
SELECT
    dst_ip AS ip,
    groupUniqArrayState(dst_mac) AS macs_state
FROM net.packets
GROUP BY ip;
