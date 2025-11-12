CREATE DATABASE IF NOT EXISTS net;

-- Replace any previous nodes / display_nodes objects
DROP VIEW IF EXISTS net.display_nodes;
DROP VIEW IF EXISTS net.nodes;

-- Every MAC from src **or** dst becomes a node; aggregation is done on-the-fly.
CREATE OR REPLACE VIEW net.nodes AS
SELECT
    mac,
    count() AS pkts,
    sum(packet_len) AS bytes,
    min(ts) AS first_seen,
    max(ts) AS last_seen,
    uniqCombined(peers) AS degree,
    arraySort(arrayDistinct(groupUniqArray(ips))) AS ips,
    arraySort(arrayDistinct(groupUniqArrayIf(src_ports, src_ports > 0))) AS src_ports,
    arraySort(arrayDistinct(groupUniqArray(l7_protos))) AS l7_protos,
    CAST(NULL AS Nullable(String)) AS device_type
FROM
    (
        -- src branch: peers = dst_mac, ips = src_ip, src_ports set, l7 as UInt16
        SELECT
            src_mac AS mac,
            dst_mac AS peers,
            src_ip  AS ips,
            assumeNotNull(src_port) AS src_ports,
            toUInt16(l7_proto) AS l7_protos,
            ts,
            toUInt64(packet_len) AS packet_len
        FROM net.packets UNION ALL

        -- dst branch: peers = src_mac, ips = dst_ip, no egress ports (0 sentinel), l7 as UInt16
        SELECT
            dst_mac AS mac,
            src_mac AS peers,
            dst_ip AS ips,
            toUInt16(0) AS src_ports,   -- excluded by the IF in the outer aggregate
            toUInt16(l7_proto) AS l7_protos,
            ts,
            toUInt64(packet_len) AS packet_len
        FROM net.packets
    )
GROUP BY mac;

-- Formatting helper (unchanged)
CREATE FUNCTION IF NOT EXISTS format_mac AS (x) ->
    concat(
        lower(substring(hex(x), 1, 2)),  ':',
        lower(substring(hex(x), 3, 2)),  ':',
        lower(substring(hex(x), 5, 2)),  ':',
        lower(substring(hex(x), 7, 2)),  ':',
        lower(substring(hex(x), 9, 2)),  ':',
        lower(substring(hex(x),11, 2))
    );

-- Readability view
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