CREATE DATABASE IF NOT EXISTS net;
-- packets.sql

DROP TABLE IF EXISTS net.packets;

CREATE TABLE net.packets
(
    packet_id UUID DEFAULT generateUUIDv4(),
    ts DateTime64(6, 'UTC'),
    src_ip IPv6,
    dst_ip IPv6,
    src_mac FixedString(6),
    dst_mac FixedString(6),

    l4_proto Enum16('NONE' = 0, 'ICMP' = 1, 'TCP' = 6, 'UDP' = 17, 'SCTP' = 132),

    l7_proto UInt16 DEFAULT 0,

    src_vendor LowCardinality(String) DEFAULT 'Unknown',
    dst_vendor LowCardinality(String) DEFAULT 'Unknown',

    src_port Nullable(UInt16),
    dst_port Nullable(UInt16),
    packet_len UInt32,
    info String DEFAULT '' CODEC(ZSTD(6))

)
    ENGINE = MergeTree
        PARTITION BY toDate(ts)
        ORDER BY (ts, src_ip, dst_ip, packet_id)
        SETTINGS index_granularity = 8192;

/* Helper to render FixedString(6) -> "aa:bb:cc:dd:ee:ff" */
CREATE FUNCTION IF NOT EXISTS format_mac AS (x) ->
    concat(
            lower(substring(hex(x), 1, 2)), ':',
            lower(substring(hex(x), 3, 2)), ':',
            lower(substring(hex(x), 5, 2)), ':',
            lower(substring(hex(x), 7, 2)), ':',
            lower(substring(hex(x), 9, 2)), ':',
            lower(substring(hex(x),11, 2))
    );

/* Readability view for dashboards & ad-hoc queries */
CREATE OR REPLACE VIEW net.display_packets AS
SELECT
    packet_id,
    ts,
    src_ip,
    dst_ip,
    format_mac(src_mac) AS src_mac,
    format_mac(dst_mac) AS dst_mac,
    l4_proto,
    l7_proto,
    src_vendor,
    dst_vendor,
    src_port,
    dst_port,
    packet_len,
    info
FROM net.packets;


-- connections.sql
/* DROP old undirected artifacts (safe if present) */
DROP VIEW IF EXISTS net.display_connections;
DROP VIEW IF EXISTS net.connections;
DROP VIEW IF EXISTS net.mv_packets_to_connections;
DROP TABLE IF EXISTS net.connections_state;

/* Directed state, keyed by src/dst MAC + IP */
CREATE TABLE net.connections_state
(
    /* key (directed) */
    src_mac FixedString(6),
    dst_mac FixedString(6),
    src_ip IPv6,
    dst_ip IPv6,

    /* rollups */
    pkts_state AggregateFunction(count),
    bytes_state AggregateFunction(sum, UInt64),
    first_seen_state AggregateFunction(min, DateTime64(6, 'UTC')),
    last_seen_state AggregateFunction(max, DateTime64(6, 'UTC')),
    l4_state AggregateFunction(groupUniqArray, UInt8),

    /* ports and L7 from the true source/dest perspective */
    src_ports_state AggregateFunction(groupUniqArray, UInt16),
    dst_ports_state AggregateFunction(groupUniqArray, UInt16),

    l7_state AggregateFunction(groupUniqArray, UInt16)
)
    ENGINE = AggregatingMergeTree
        ORDER BY (src_mac, dst_mac, src_ip, dst_ip);

/* Materialized view: map each packet into its directed bucket */
CREATE MATERIALIZED VIEW net.mv_packets_to_connections
            TO net.connections_state AS
SELECT
    minState(ts) AS first_seen_state,
    maxState(ts) AS last_seen_state,
    src_mac,
    dst_mac,
    src_ip,
    dst_ip,

    countState() AS pkts_state,
    sumState(toUInt64(packet_len)) AS bytes_state,
    groupUniqArrayState(toUInt8(l4_proto)) AS l4_state,

    /* collect both sides explicitly */
    groupUniqArrayStateIf(assumeNotNull(src_port), src_port IS NOT NULL) AS src_ports_state,
    groupUniqArrayStateIf(assumeNotNull(dst_port), dst_port IS NOT NULL) AS dst_ports_state,

    /* keep the exact Enum16 from packets.l7_proto */
    groupUniqArrayState(l7_proto) AS l7_state
FROM net.packets
GROUP BY src_mac, dst_mac, src_ip, dst_ip;

/* User-facing directed view */
CREATE OR REPLACE VIEW net.connections AS
SELECT
    minMerge(first_seen_state) AS first_seen,
    maxMerge(last_seen_state) AS last_seen,
    src_mac,
    dst_mac,
    src_ip,
    dst_ip,
    countMerge(pkts_state) AS pkts,
    sumMerge(bytes_state) AS bytes,
    arraySort(groupUniqArrayMerge(l4_state)) AS protos,
    arraySort(arrayDistinct(groupUniqArrayMerge(src_ports_state))) AS src_ports,
    arraySort(arrayDistinct(groupUniqArrayMerge(dst_ports_state))) AS dst_ports,
    arraySort(arrayDistinct(groupUniqArrayMerge(l7_state))) AS l7_protos
FROM net.connections_state
GROUP BY src_mac, dst_mac, src_ip, dst_ip;

/* Pretty formatter for MACs (same helper as before) */
CREATE FUNCTION IF NOT EXISTS format_mac AS (x) ->
    concat(
            lower(substring(hex(x), 1, 2)),  ':',
            lower(substring(hex(x), 3, 2)),  ':',
            lower(substring(hex(x), 5, 2)),  ':',
            lower(substring(hex(x), 7, 2)),  ':',
            lower(substring(hex(x), 9, 2)),  ':',
            lower(substring(hex(x),11, 2))
    );

/* Readable version with colon MACs */
CREATE OR REPLACE VIEW net.display_connections AS
SELECT
    first_seen,
    last_seen,
    format_mac(src_mac) AS src_mac,
    format_mac(dst_mac) AS dst_mac,
    src_ip,
    dst_ip,
    pkts,
    bytes,
    protos,
    src_ports,
    dst_ports,
    l7_protos
FROM net.connections;


-- nodes.sql
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
    anyHeavy(vendor) AS device_type
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
            toUInt64(packet_len) AS packet_len,
            src_vendor AS vendor
        FROM net.packets UNION ALL

        -- dst branch: peers = src_mac, ips = dst_ip, no egress ports (0 sentinel), l7 as UInt16
        SELECT
            dst_mac AS mac,
            src_mac AS peers,
            dst_ip AS ips,
            toUInt16(0) AS src_ports,   -- excluded by the IF in the outer aggregate
            toUInt16(l7_proto) AS l7_protos,
            ts,
            toUInt64(packet_len) AS packet_len,
            dst_vendor AS vendor
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


-- raw_bytes.sql

DROP TABLE IF EXISTS net.raw_bytes;

-- Stores the full captured bytes for each packet, keyed by packet_id
CREATE TABLE net.raw_bytes
(
    packet_id UUID,                               -- must match net.packets.packet_id
    ts DateTime64(6, 'UTC'),               -- keeps partitioning/time scans efficient
    bytes String CODEC(ZSTD(6))               -- raw binary blob (full frame including payload)
)
    ENGINE = MergeTree
        PARTITION BY toDate(ts)
        ORDER BY (ts, packet_id)
        SETTINGS index_granularity = 8192;

-- Convenience view for inspection (hex output, don’t use in heavy queries)
CREATE OR REPLACE VIEW net.display_raw_bytes AS
SELECT
    ts,
    packet_id,
    hex(bytes) AS bytes_hex
FROM net.raw_bytes;

-- zeek.sql
DROP TABLE IF EXISTS net.weird;

CREATE TABLE net.weird
(
    ts DateTime64(6, 'UTC'),

    uid Nullable(String),

    orig_h Nullable(IPv6),
    orig_p Nullable(UInt16),
    resp_h Nullable(IPv6),
    resp_p Nullable(UInt16),

    name LowCardinality(String),
    addl Nullable(String),

    notice UInt8,
    peer LowCardinality(String),

    source LowCardinality(Nullable(String)),
    identifier Nullable(String),

    day Date MATERIALIZED toDate(ts)
)
    ENGINE = MergeTree
        PARTITION BY day
        ORDER BY
            (
             day,
             ts,
             name,
             ifNull(uid, ''),
             ifNull(orig_h, toIPv6('::')),
             ifNull(orig_p, 0),
             ifNull(resp_h, toIPv6('::')),
             ifNull(resp_p, 0)
                );
