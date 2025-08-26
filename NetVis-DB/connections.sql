CREATE TABLE net.connections
(
    node_a IPv6, -- IP for one side of the connection (the lower IP numerically)
    node_b IPv6, -- IP for the other side of the connection (the higher IP numerically)

    num_packets_state   AggregateFunction(count),
    first_seen_state    AggregateFunction(min, DateTime64(6, 'UTC')), -- time of first packet seen on this connection
    last_seen_state     AggregateFunction(max, DateTime64(6, 'UTC')), -- time of last packet seen on this connection

    protos_state        AggregateFunction(groupUniqArray, UInt8), -- all L4 protocols seen on this connection

    tcp_src_ports_state AggregateFunction(groupUniqArray, UInt16), -- all TCP ports seen on src side
    tcp_dst_ports_state AggregateFunction(groupUniqArray, UInt16), -- all TCP ports seen on dst side
    udp_src_ports_state AggregateFunction(groupUniqArray, UInt16), -- all UDP ports seen on src side
    udp_dst_ports_state AggregateFunction(groupUniqArray, UInt16) -- all UDP ports seen on dst side
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
