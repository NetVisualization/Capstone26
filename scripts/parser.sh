#!/bin/bash

CH_DSN=http://100.111.112.111:8123 \
CH_USER=capstone \
CH_PASSWORD=boogle \
../NetVis/pcap2ch/target/release/pcap2ch \
  --database net \
  --table packets \
  --batch 10000 \
  ../scans/test.pcap
