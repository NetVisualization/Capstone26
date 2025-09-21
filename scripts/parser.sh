#!/bin/bash

../NetVis/pcap2ch/target/release/pcap2ch \
  -f ../scans/test-small.pcap \
  --ch-url http://10.200.1.13:8123 \
  --ch-db net \
  --ch-user capstone \
  --ch-password boogle \
  --batch 5000
