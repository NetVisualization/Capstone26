#!/usr/bin/env bash
set -euo pipefail

export DEBIAN_FRONTEND=noninteractive

echo "[netvis-entry] Updating apt and installing system dependencies..."
apt-get update
apt-get install -y \
  build-essential \
  curl \
  pkg-config \
  libssl-dev \
  libpcap-dev \
  iproute2 \
  iputils-ping \
  tcpdump \
  ca-certificates

# Install Rust toolchain if not installed
if ! command -v cargo >/dev/null 2>&1; then
  echo "[netvis-entry] Installing Rust toolchain via rustup..."
  curl https://sh.rustup.rs -sSf | sh -s -- -y
fi

# Ensure cargo is in PATH
export PATH="$HOME/.cargo/bin:$PATH"

echo "[netvis-entry] Building pcap2ch (release)..."
cd /netvis/pcap2ch
cargo build --release

echo "[netvis-entry] Setup complete. Launching shell in /netvis."
cd /netvis
exec bash
