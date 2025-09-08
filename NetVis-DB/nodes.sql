-- MAC-centric nodes

CREATE DATABASE IF NOT EXISTS net;

-- Drop old IP-centric nodes pipeline
DROP VIEW  IF EXISTS net.nodes;
DROP TABLE IF EXISTS net.nodes_state;
DROP VIEW  IF EXISTS net.mv_connections_to_nodes;
DROP VIEW  IF EXISTS net.mv_packets_to_nodes;

-- New MAC-centric state
CREATE TABLE net.nodes_state
(
    mac FixedString(12),

    num_packets_state      AggregateFunction(count),
    num_bytes_state        AggregateFunction(sum, UInt64),
    first_seen_state       AggregateFunction(min, DateTime64(6, 'UTC')),
    last_seen_state        AggregateFunction(max, DateTime64(6, 'UTC')),

    -- unique peer MACs for degree
    peers_state            AggregateFunction(uniqCombined, FixedString(12)),

    -- all IPs observed on this MAC (IPv6, IPv4 is ::ffff:a.b.c.d)
    ips_state              AggregateFunction(groupUniqArray, IPv6),

    device_type LowCardinality(Nullable(String)) DEFAULT NULL
)
    ENGINE = AggregatingMergeTree
        ORDER BY (mac);

-- Build MAC nodes directly from packets (two branches, src and dst)
CREATE MATERIALIZED VIEW net.mv_packets_to_nodes
            TO net.nodes_state
AS
SELECT
    src_mac                                       AS mac,
    countState()                                  AS num_packets_state,
    sumState(toUInt64(packet_len))                AS num_bytes_state,
    minState(ts)                                  AS first_seen_state,
    maxState(ts)                                  AS last_seen_state,
    uniqCombinedState(dst_mac)                    AS peers_state,
    groupUniqArrayState(src_ip)                   AS ips_state,
    CAST(NULL AS Nullable(String))                AS device_type
FROM net.packets
GROUP BY mac

UNION ALL
SELECT
    dst_mac                                       AS mac,
    countState(),
    sumState(toUInt64(packet_len)),
    minState(ts),
    maxState(ts),
    uniqCombinedState(src_mac),
    groupUniqArrayState(dst_ip),
    CAST(NULL AS Nullable(String))                AS device_type
FROM net.packets
GROUP BY mac;

-- Final MAC-centric nodes view
CREATE OR REPLACE VIEW net.nodes AS
SELECT
    mac,
    countMerge(num_packets_state)                              AS pkts,
    sumMerge(num_bytes_state)                                  AS bytes,
    minMerge(first_seen_state)                                 AS first_seen,
    maxMerge(last_seen_state)                                  AS last_seen,
    uniqCombinedMerge(peers_state)                             AS degree,
    arraySort(arrayDistinct(groupUniqArrayMerge(ips_state)))   AS ips,
    device_type
FROM net.nodes_state
GROUP BY mac, device_type;
