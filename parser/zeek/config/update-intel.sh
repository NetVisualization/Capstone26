#!/bin/bash

# Define the target directory for Zeek site configurations
INTEL_DIR="/usr/local/zeek/share/zeek/site"
mkdir -p "$INTEL_DIR"

# 1. Download and format the Abuse.ch JA3 feed
wget -qO- https://sslbl.abuse.ch/blacklist/ja3_fingerprints.csv | \
grep -v "^#" | \
awk -F, '{printf "%s\tIntel::JA3\tAbuse.ch\t%s\tT\n", $1, $2}' > "$INTEL_DIR/intel_abuse.tmp"

# 2. Prepend the strictly required Zeek Intelligence header
printf "#fields\tindicator\tindicator_type\tmeta.source\tmeta.desc\tmeta.do_notice\n" > "$INTEL_DIR/intel.dat"

# 3. Clean up temporary files
rm "$INTEL_DIR/intel_abuse.tmp"