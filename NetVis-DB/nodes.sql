-- MAC-centric nodes

CREATE DATABASE IF NOT EXISTS net;

DROP VIEW  IF EXISTS net.nodes;
DROP TABLE IF EXISTS net.nodes_state;
DROP VIEW  IF EXISTS net.mv_connections_to_nodes;
DROP VIEW  IF EXISTS net.mv_packets_to_nodes;

-- Keep a single ports array: src_ports (egress)
CREATE TABLE net.nodes_state
(
    mac FixedString(12),

    num_packets_state      AggregateFunction(count),
    num_bytes_state        AggregateFunction(sum, UInt64),
    first_seen_state       AggregateFunction(min, DateTime64(6, 'UTC')),
    last_seen_state        AggregateFunction(max, DateTime64(6, 'UTC')),

    peers_state            AggregateFunction(uniqCombined, FixedString(12)),
    ips_state              AggregateFunction(groupUniqArray, IPv6),

    -- egress ports used by this MAC as packet source
    src_ports_state        AggregateFunction(groupUniqArray, UInt16),

    device_type LowCardinality(Nullable(String)) DEFAULT NULL
)
    ENGINE = AggregatingMergeTree
        ORDER BY (mac);

-- Build nodes directly from packets, collecting only src ports
CREATE MATERIALIZED VIEW net.mv_packets_to_nodes
            TO net.nodes_state
AS
-- Branch 1: MAC as packet source (collect src_port)
SELECT
    src_mac                                            AS mac,
    countState()                                       AS num_packets_state,
    sumState(toUInt64(packet_len))                     AS num_bytes_state,
    minState(ts)                                       AS first_seen_state,
    maxState(ts)                                       AS last_seen_state,
    uniqCombinedState(dst_mac)                         AS peers_state,
    groupUniqArrayState(src_ip)                        AS ips_state,
    groupUniqArrayStateIf(assumeNotNull(src_port), src_port IS NOT NULL) AS src_ports_state,
    CAST(NULL AS Nullable(String))                     AS device_type
FROM net.packets
GROUP BY mac

UNION ALL

-- Branch 2: MAC as packet destination (no ports collected here)
SELECT
    dst_mac                                            AS mac,
    countState(),
    sumState(toUInt64(packet_len)),
    minState(ts),
    maxState(ts),
    uniqCombinedState(src_mac),
    groupUniqArrayState(dst_ip),
    groupUniqArrayStateIf(assumeNotNull(src_port), 0), -- no-op
    CAST(NULL AS Nullable(String))
FROM net.packets
GROUP BY mac;

-- Final view
CREATE OR REPLACE VIEW net.nodes AS
SELECT
    mac,
    countMerge(num_packets_state)                                 AS pkts,
    sumMerge(num_bytes_state)                                     AS bytes,
    minMerge(first_seen_state)                                    AS first_seen,
    maxMerge(last_seen_state)                                     AS last_seen,
    uniqCombinedMerge(peers_state)                                AS degree,
    arraySort(arrayDistinct(groupUniqArrayMerge(ips_state)))      AS ips,

    arraySort(arrayDistinct(groupUniqArrayMerge(src_ports_state))) AS src_ports,

    device_type
FROM net.nodes_state
GROUP BY mac, device_type;
