CREATE DATABASE IF NOT EXISTS net;

DROP VIEW IF EXISTS net.display_nodes;
DROP VIEW IF EXISTS net.nodes;

CREATE OR REPLACE VIEW net.nodes AS
SELECT
    mac,

    /* Vendor enrichment — OUI as UInt32 */
    dictGetUInt32OrDefault(
            'net.oui_dict', 'vendor_id',
            (
                toUInt32(reinterpretAsUInt8(substring(mac, 1, 1))) * 65536 +
                toUInt32(reinterpretAsUInt8(substring(mac, 2, 1))) * 256 +
                toUInt32(reinterpretAsUInt8(substring(mac, 3, 1)))
                ),
            0
    ) AS vendor_id,

    dictGetStringOrDefault(
            'net.oui_dict', 'vendor',
            (
                toUInt32(reinterpretAsUInt8(substring(mac, 1, 1))) * 65536 +
                toUInt32(reinterpretAsUInt8(substring(mac, 2, 1))) * 256 +
                toUInt32(reinterpretAsUInt8(substring(mac, 3, 1)))
                ),
            'Unknown'
    ) AS vendor,

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
        SELECT
            src_mac AS mac,
            dst_mac AS peers,
            src_ip  AS ips,
            assumeNotNull(src_port) AS src_ports,
            toUInt16(l7_proto) AS l7_protos,
            ts,
            toUInt64(packet_len) AS packet_len
        FROM net.packets

        UNION ALL

        SELECT
            dst_mac AS mac,
            src_mac AS peers,
            dst_ip AS ips,
            toUInt16(0) AS src_ports,
            toUInt16(l7_proto) AS l7_protos,
            ts,
            toUInt64(packet_len) AS packet_len
        FROM net.packets
        )
GROUP BY mac;

/* Readability view (inline formatting; no CREATE FUNCTION) */
CREATE OR REPLACE VIEW net.display_nodes AS
SELECT
    concat(
            lower(substring(hex(mac), 1, 2)), ':',
            lower(substring(hex(mac), 3, 2)), ':',
            lower(substring(hex(mac), 5, 2)), ':',
            lower(substring(hex(mac), 7, 2)), ':',
            lower(substring(hex(mac), 9, 2)), ':',
            lower(substring(hex(mac), 11, 2))
    ) AS mac,

    vendor_id,
    vendor,
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
