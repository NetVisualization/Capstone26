CREATE DATABASE IF NOT EXISTS net;

DROP TABLE IF EXISTS net.nodes_state;
DROP VIEW IF EXISTS net.mv_packets_to_nodes;
DROP TABLE IF EXISTS net.display_nodes;
DROP TABLE IF EXISTS net.nodes;

CREATE TABLE net.nodes_state
(
    mac FixedString(6),

    num_packets_state AggregateFunction(count),
    num_bytes_state   AggregateFunction(sum, UInt64),
    first_seen_state  AggregateFunction(min, DateTime64(6, 'UTC')),
    last_seen_state   AggregateFunction(max, DateTime64(6, 'UTC')),

    peers_state AggregateFunction(uniqCombined, FixedString(6)),
    ips_state   AggregateFunction(groupUniqArray, IPv6),

    -- egress ports only
    src_ports_state AggregateFunction(groupUniqArray, UInt16),

    -- app protos (CONCRETE Enum16 — same list as above)
    l7_protos_state AggregateFunction(groupUniqArray, Enum16(
        'UNKNOWN' = 0,
        'SSH'     = 22,
        'SMTP'    = 25,
        'DNS'     = 53,
        'DHCP'    = 67,
        'HTTP'    = 80,
        'POP3'    = 110,
        'NTP'     = 123,
        'IMAP'    = 143,
        'TLS'     = 443,
        'SMB'     = 445,
        'QUIC'    = 1000,
        'SSDP'    = 1900,
        'RDP'     = 3389,
        'MDNS'    = 5353
        )),

    device_type LowCardinality(Nullable(String)) DEFAULT NULL
)
    ENGINE = AggregatingMergeTree
        ORDER BY (mac);

CREATE MATERIALIZED VIEW net.mv_packets_to_nodes
            TO net.nodes_state
AS
-- src_mac branch: collect src_port + l7
SELECT
    src_mac AS mac,
    countState()                                       AS num_packets_state,
    sumState(toUInt64(packet_len))                     AS num_bytes_state,
    minState(ts)                                       AS first_seen_state,
    maxState(ts)                                       AS last_seen_state,
    uniqCombinedState(dst_mac)                         AS peers_state,
    groupUniqArrayState(src_ip)                        AS ips_state,
    groupUniqArrayStateIf(assumeNotNull(src_port), src_port IS NOT NULL) AS src_ports_state,
    groupUniqArrayState(l7_proto)                      AS l7_protos_state,
    CAST(NULL AS Nullable(String))                     AS device_type
FROM net.packets
GROUP BY mac

UNION ALL

-- dst_mac branch: collect l7 only
SELECT
    dst_mac AS mac,
    countState(),
    sumState(toUInt64(packet_len)),
    minState(ts),
    maxState(ts),
    uniqCombinedState(src_mac),
    groupUniqArrayState(dst_ip),
    groupUniqArrayStateIf(assumeNotNull(src_port), 0),
    groupUniqArrayState(l7_proto),
    CAST(NULL AS Nullable(String))
FROM net.packets
GROUP BY mac;

CREATE OR REPLACE VIEW net.nodes AS
SELECT
    mac,
    countMerge(num_packets_state)                                   AS pkts,
    sumMerge(num_bytes_state)                                       AS bytes,
    minMerge(first_seen_state)                                      AS first_seen,
    maxMerge(last_seen_state)                                       AS last_seen,
    uniqCombinedMerge(peers_state)                                  AS degree,
    arraySort(arrayDistinct(groupUniqArrayMerge(ips_state)))        AS ips,
    arraySort(arrayDistinct(groupUniqArrayMerge(src_ports_state)))  AS src_ports,
    arraySort(arrayDistinct(groupUniqArrayMerge(l7_protos_state))) AS l7_protos,
    device_type
FROM net.nodes_state
GROUP BY mac, device_type;

/* Ensure helper exists even if other files weren’t run first */
CREATE FUNCTION IF NOT EXISTS format_mac AS (x) ->
    concat(
            lower(substring(hex(x), 1, 2)),  ':',
            lower(substring(hex(x), 3, 2)),  ':',
            lower(substring(hex(x), 5, 2)),  ':',
            lower(substring(hex(x), 7, 2)),  ':',
            lower(substring(hex(x), 9, 2)),  ':',
            lower(substring(hex(x),11, 2))
    );

/* Readability view: formats the node key MAC for display */
CREATE OR REPLACE VIEW net.display_nodes AS
SELECT
    format_mac(mac) AS mac,
    pkts,
    bytes,
    first_seen,
    last_seen,
    degree,
    ips,
    src_ports,
    l7_protos,
    device_type
FROM net.nodes;
