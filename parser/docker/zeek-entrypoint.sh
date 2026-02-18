#!/bin/bash
set -e

# set up secret from envvar
SALT="${ZEEK_DIGEST_SALT:-default-insecure-salt}"
echo "redef digest_salt = \"$SALT\";" > /usr/local/zeek/share/zeek/site/secret.zeek
# pass execution to the main container command (zeek)
exec "$@"