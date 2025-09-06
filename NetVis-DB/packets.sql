CREATE DATABASE IF NOT EXISTS net;

-- Essential packet facts for nodes/edges
CREATE TABLE net.packets
(
    ts         DateTime64(6, 'UTC'),
    src_ip     IPv6,
    dst_ip     IPv6,
    l4_proto   Enum16('NONE' = 0, 'ICMP' = 1, 'TCP' = 6, 'UDP' = 17, 'SCTP' = 132),
    src_port   Nullable(UInt16),
    dst_port   Nullable(UInt16),
    packet_len UInt32
)
    ENGINE = MergeTree
        PARTITION BY toDate(ts)
        ORDER BY (ts, src_ip, dst_ip)
        SETTINGS index_granularity = 8192;
