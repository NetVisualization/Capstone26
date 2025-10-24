CREATE DATABASE IF NOT EXISTS net;

-- Replace any previous nodes / display_nodes objects
DROP VIEW IF EXISTS net.display_nodes;
DROP VIEW IF EXISTS net.nodes;

-- Every MAC from src **or** dst becomes a node; aggregation is done on-the-fly.
CREATE OR REPLACE VIEW net.nodes AS
SELECT
    ip,
    count() AS pkts,
    sum(packet_len) AS bytes,
    min(ts) AS first_seen,
    max(ts) AS last_seen,
    uniqCombined(peers) AS degree,
    arraySort(arrayDistinct(groupUniqArray(macs))) AS macs,
    arraySort(arrayDistinct(groupUniqArray(src_ports))) AS src_ports,
    arraySort(arrayDistinct(groupUniqArray(l7_protos))) AS l7_protos,
    CAST(NULL AS Nullable(String)) AS device_type
FROM
    (
        -- src branch: peers = dst_ip, macs = src_mac, etc.
        SELECT
            src_ip AS ip,
            dst_ip AS peers,
            src_mac AS macs,
            assumeNotNull(src_port) AS src_ports,
            toUInt16(l7_proto) AS l7_protos,
            ts,
            toUInt64(packet_len) AS packet_len
        FROM net.packets
        UNION ALL
        -- dst branch: peers = src_ip, macs = dst_mac
        SELECT
            dst_ip AS ip,
            src_ip AS peers,
            dst_mac AS macs,
            toUInt16(0) AS src_ports,
            toUInt16(l7_proto) AS l7_protos,
            ts,
            toUInt64(packet_len) AS packet_len
        FROM net.packets
        )
GROUP BY ip;

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
    ip,
    pkts,
    bytes,
    first_seen,
    last_seen,
    degree,
    arrayMap(x -> format_mac(x), macs) AS macs,
    src_ports,
    l7_protos,
    device_type
FROM net.nodes;