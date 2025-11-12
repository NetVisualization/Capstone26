#!/bin/bash

# Live Capture
RUST_LOG=info \
../NetVis/pcap2ch/target/release/pcap2ch \
 --ch-url http://10.200.1.13:8123 \
 --ch-db net \
 --ch-user capstone \
 --ch-password 'boogle' \
 --ch-head-table packets \
 --ch-raw-table raw_bytes \
 live \
 --iface en0 \
