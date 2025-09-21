CREATE DATABASE IF NOT EXISTS net;

CREATE TABLE net.connections_state
(
    -- normalized endpoints (min/max by IP)
    node_a IPv6,
    node_b IPv6,

    -- traffic rollups
    num_packets_state AggregateFunction(count),
    num_bytes_state   AggregateFunction(sum, UInt64),
    first_seen_state  AggregateFunction(min, DateTime64(6, 'UTC')),
    last_seen_state   AggregateFunction(max, DateTime64(6, 'UTC')),
    protos_state      AggregateFunction(groupUniqArray, UInt8),

    -- MACs per side (hex strings)
    node_a_macs_state AggregateFunction(groupUniqArray, FixedString(6)),
    node_b_macs_state AggregateFunction(groupUniqArray, FixedString(6)),

    -- Node-A ports only
    node_a_src_ports_state AggregateFunction(groupUniqArray, UInt16),
    node_a_dst_ports_state AggregateFunction(groupUniqArray, UInt16),

    -- Node-A app protos (CONCRETE Enum16 — matches packets.l7_proto exactly)
    node_a_l7_protos_state AggregateFunction(groupUniqArray, Enum16(
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
        ))
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

    groupUniqArrayState( if(src_ip <= dst_ip, src_mac, dst_mac) ) AS node_a_macs_state,
    groupUniqArrayState( if(src_ip <= dst_ip, dst_mac, src_mac) ) AS node_b_macs_state,

    groupUniqArrayStateIf(assumeNotNull(src_port), (src_port IS NOT NULL) AND (src_ip <= dst_ip)) AS node_a_src_ports_state,
    groupUniqArrayStateIf(assumeNotNull(dst_port), (dst_port IS NOT NULL) AND (src_ip  >  dst_ip)) AS node_a_dst_ports_state,

    -- emit the SAME Enum16 as in the state (no casts!)
    groupUniqArrayStateIf(l7_proto, (src_ip <= dst_ip)) AS node_a_l7_protos_state
FROM net.packets
GROUP BY node_a, node_b;

CREATE OR REPLACE VIEW net.connections AS
SELECT
    node_a,
    node_b,
    countMerge(num_packets_state)                                        AS pkts,
    sumMerge(num_bytes_state)                                            AS bytes,
    minMerge(first_seen_state)                                           AS first_seen,
    maxMerge(last_seen_state)                                            AS last_seen,
    arraySort(groupUniqArrayMerge(protos_state))                         AS protos,
    arraySort(arrayDistinct(groupUniqArrayMerge(node_a_macs_state)))     AS node_a_macs,
    arraySort(arrayDistinct(groupUniqArrayMerge(node_b_macs_state)))     AS node_b_macs,
    arraySort(arrayDistinct(groupUniqArrayMerge(node_a_src_ports_state))) AS node_a_src_ports,
    arraySort(arrayDistinct(groupUniqArrayMerge(node_a_dst_ports_state))) AS node_a_dst_ports,
    arraySort(arrayDistinct(groupUniqArrayMerge(node_a_l7_protos_state))) AS node_a_l7_protos
FROM net.connections_state
GROUP BY node_a, node_b;
