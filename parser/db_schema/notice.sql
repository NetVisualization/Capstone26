CREATE TABLE net.notice
(
    ts DateTime64(6, 'UTC'),

    uid Nullable(String),

    id_orig_h Nullable(IPv6),
    id_resp_h Nullable(IPv6),

    note Enum256(   -- Expand or modify as needed
        'Scan::Port_Scan' = 1,
        'Scan::Address_Scan' = 2,
        'HTTP::SQL_Injection_Attack' = 3,
        'SSH::Password_Guessing' = 4,
        'FTP::Bruteforce' = 5,
        'SSL::Invalid_Certificate' = 6,
        'Weird::Activity' = 7,
        'Notice::Unknown' = 255
    ),

    msg Nullable(String),
    sub Nullable(String),

    src Nullable(IPv6),

    actions Array(Enum8(
        'Notice::ACTION_LOG' = 1,
        'Notice::ACTION_EMAIL' = 2,
        'Notice::ACTION_ALARM' = 3,
        'Notice::ACTION_DROP' = 4
    )),

    -- Derived partition
    day Date MATERIALIZED toDate(ts)
)
ENGINE = MergeTree
PARTITION BY day
ORDER BY
(
    day,
    ts,
    note,
    ifNull(uid, ''),
    ifNull(id_orig_h,  toIPv6('::')),
    ifNull(id_resp_h,  toIPv6('::')),
    ifNull(src,        toIPv6('::'))
);
