CREATE DATABASE IF NOT EXISTS net;

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


DROP TABLE IF EXISTS net.notice;

CREATE TABLE net.notice
(
    ts DateTime64(6, 'UTC'),

    uid       Nullable(String),

    id_orig_h Nullable(IPv6),
    id_resp_h Nullable(IPv6),

    -- Zeek notice type, now allowed to be NULL
    note      Nullable(String),

    msg       Nullable(String),
    sub       Nullable(String),

    src       Nullable(IPv6),

    -- Actions taken (log, email, etc.)
    actions   Array(String),

    day Date MATERIALIZED toDate(ts)
)
ENGINE = MergeTree
PARTITION BY day
ORDER BY
(
    day,
    ts,
    ifNull(note, ''),
    ifNull(uid, ''),
    ifNull(id_orig_h, toIPv6('::')),
    ifNull(id_resp_h, toIPv6('::')),
    ifNull(src,       toIPv6('::'))
);
