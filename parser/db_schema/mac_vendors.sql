CREATE DATABASE IF NOT EXISTS net;

DROP DICTIONARY IF EXISTS net.oui_dict;
DROP TABLE IF EXISTS net.oui_vendors;

-- OUI stored as 24-bit integer in a UInt32:
-- oui_u32 = (b1<<16) | (b2<<8) | b3
CREATE TABLE net.oui_vendors
(
    oui_u32 UInt32,
    vendor_id UInt32,
    vendor String
)
ENGINE = MergeTree
ORDER BY (oui_u32);

CREATE DICTIONARY net.oui_dict
(
    oui_u32 UInt32,
    vendor_id UInt32,
    vendor String
)
PRIMARY KEY oui_u32
SOURCE(CLICKHOUSE(DB 'net' TABLE 'oui_vendors'))
LAYOUT(HASHED())
LIFETIME(MIN 300 MAX 3600);
