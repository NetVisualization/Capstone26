CREATE TABLE net.connections
(
    node_a IPv6,
    node_b IPv6,

    num_packets_state   AggregateFunction(count),
    first_seen_state    AggregateFunction(min, DateTime64(6, 'UTC')),
    last_seen_state     AggregateFunction(max, DateTime64(6, 'UTC')),

    protos_state        AggregateFunction(groupUniqArray, UInt8),

    tcp_src_ports_state AggregateFunction(groupUniqArray, UInt16),
    tcp_dst_ports_state AggregateFunction(groupUniqArray, UInt16),
    udp_src_ports_state AggregateFunction(groupUniqArray, UInt16),
    udp_dst_ports_state AggregateFunction(groupUniqArray, UInt16)
)
    ENGINE = AggregatingMergeTree
        ORDER BY (node_a, node_b);

CREATE MATERIALIZED VIEW net.mv_packets_to_connections
            TO net.connections
AS
SELECT
    if(src_ip <= dst_ip, src_ip, dst_ip) AS node_a,
    if(src_ip <= dst_ip, dst_ip, src_ip) AS node_b,

    countState()                                    AS num_packets_state,
    minState(ts)                                    AS first_seen_state,
    maxState(ts)                                    AS last_seen_state,

    groupUniqArrayState(toUInt8(l4_proto))          AS protos_state,

    groupUniqArrayStateIf(assumeNotNull(src_port), l4_proto='TCP' AND isNotNull(src_port)) AS tcp_src_ports_state,
    groupUniqArrayStateIf(assumeNotNull(dst_port), l4_proto='TCP' AND isNotNull(dst_port)) AS tcp_dst_ports_state,
    groupUniqArrayStateIf(assumeNotNull(src_port), l4_proto='UDP' AND isNotNull(src_port)) AS udp_src_ports_state,
    groupUniqArrayStateIf(assumeNotNull(dst_port), l4_proto='UDP' AND isNotNull(dst_port)) AS udp_dst_ports_state
FROM net.packets
GROUP BY node_a, node_b;
