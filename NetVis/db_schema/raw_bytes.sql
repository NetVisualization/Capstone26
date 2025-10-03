CREATE DATABASE IF NOT EXISTS net;

DROP TABLE IF EXISTS net.raw_bytes;

-- Stores the full captured bytes for each packet, keyed by packet_id
CREATE TABLE net.raw_bytes
(
    packet_id UUID,                               -- must match net.packets.packet_id
    ts        DateTime64(6, 'UTC'),               -- keeps partitioning/time scans efficient
    bytes     String CODEC(ZSTD(6))               -- raw binary blob (full frame including payload)
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
