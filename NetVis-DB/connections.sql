-- Unordered IP pair + basic stats for edge thickness/time window
CREATE TABLE net.connections
(
    node_a IPv6,  -- min(src_ip, dst_ip)
    node_b IPv6,  -- max(src_ip, dst_ip)

    num_packets_state AggregateFunction(count),
    num_bytes_state   AggregateFunction(sum, UInt64),
    first_seen_state  AggregateFunction(min, DateTime64(6, 'UTC')),
    last_seen_state   AggregateFunction(max, DateTime64(6, 'UTC')),
    protos_state      AggregateFunction(groupUniqArray, UInt8)  -- toUInt8(l4_proto)
)
    ENGINE = AggregatingMergeTree
        ORDER BY (node_a, node_b);

CREATE MATERIALIZED VIEW net.mv_packets_to_connections
            TO net.connections
AS
SELECT
    if(src_ip <= dst_ip, src_ip, dst_ip) AS node_a,
    if(src_ip <= dst_ip, dst_ip, src_ip) AS node_b,
    countState()                                            AS num_packets_state,
    sumState(toUInt64(packet_len))                          AS num_bytes_state,
    minState(ts)                                            AS first_seen_state,
    maxState(ts)                                            AS last_seen_state,
    groupUniqArrayState(toUInt8(l4_proto))                  AS protos_state
FROM net.packets
GROUP BY node_a, node_b;
