#!/bin/bash

# Start tshark with sudo privileges;
# scan on all interfaces with JSON output and save to packets.jsonl
sudo tshark -i any -l -T jsonraw > ../scans/packets.jsonl
