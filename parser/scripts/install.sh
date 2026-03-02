#!/bin/bash

# ensure sudo
if [[ $EUID -ne 0 ]]; then
    echo "Error: This script must be run as root." >&2
    exit 1
fi

# helper func
get_default_nic() {
  case "$OSTYPE" in
    darwin*)
      route -n get default 2>/dev/null | grep 'interface:' | awk '{print $2}' || echo "en0" ;;
    linux*)
      ip route | grep default | awk '{print $5}' | head -n1 || echo "eth0" ;;
    msys* | cygwin* | mingw*)
      netstat -rn | grep '0.0.0.0' | awk '{print $4}' | head -n1 || echo "eth0" ;;
    *) echo "eth0" ;;
  esac
}

# explain and confirm installation steps, verify linux host
echo "This script will set up the clickhouse (database) and zeek (IDS) backend for netvis."
echo "This automates the default configuration, where both backend containers will be run on this host."
echo "If you want customize the backend deployment, please follow the manual instructions in documentation directory."
read -p "Please ensure you are running an up-to-date linux server. Do you wish to continue? (y/n) " CONTINUE
if [[ $CONTINUE != "y" ]]; then
    echo "Installation cancelled."
    exit 1
fi

# prompt for credentials and config info
read -p "Enter ClickHouse username: " CLICKHOUSE_USER
read -s -p "Enter ClickHouse password: " CLICKHOUSE_PASSWORD
echo ""
read -s -p "Confirm ClickHouse password: " CLICKHOUSE_PASSWORD_CONFIRM
echo ""
if [[ "$CLICKHOUSE_PASSWORD" != "$CLICKHOUSE_PASSWORD_CONFIRM" ]]; then
    echo "Error: Passwords do not match." >&2
    exit 1
fi
read -p "Enter Clickhouse database name (default: net): " CLICKHOUSE_DB
if [[ -z "$CLICKHOUSE_DB" ]]; then
    CLICKHOUSE_DB="net"
fi
read -p "Enter ClickHouse external port (default: 8123): " CLICKHOUSE_PORT
if [[ -z "$CLICKHOUSE_PORT" ]]; then
    CLICKHOUSE_PORT="8123"
fi

# save to .env file for docker-compose
cat > ../docker/.env <<EOL
CLICKHOUSE_USER=$CLICKHOUSE_USER
CLICKHOUSE_PASSWORD=$CLICKHOUSE_PASSWORD
CLICKHOUSE_DB=$CLICKHOUSE_DB
CLICKHOUSE_PORT=$CLICKHOUSE_PORT
EOL

# install dependencies (docker, docker-compose, rust, cargo, etc.)
echo "Installing docker"
# https://docs.docker.com/compose/install/linux/#install-using-the-repository
if [[ -x "$(which apt)" ]]; then
    # Add Docker's official GPG key:
    apt update
    apt install -y ca-certificates curl
    install -m 0755 -d /etc/apt/keyrings
    curl -fsSL https://download.docker.com/linux/ubuntu/gpg -o /etc/apt/keyrings/docker.asc
    chmod a+r /etc/apt/keyrings/docker.asc

    # Add the repository to Apt sources:
    tee /etc/apt/sources.list.d/docker.sources <<EOF
Types: deb
URIs: https://download.docker.com/linux/ubuntu
Suites: $(. /etc/os-release && echo "${UBUNTU_CODENAME:-$VERSION_CODENAME}")
Components: stable
Signed-By: /etc/apt/keyrings/docker.asc
EOF

    # install from new repo
    apt update
    apt install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin
else
    # https://docs.docker.com/engine/install/ubuntu/#install-using-the-convenience-script
    curl -fsSL https://get.docker.com -o get-docker.sh
    sh ./get-docker.sh
    rm ./get-docker.sh 2>/dev/null
    DOCKER_CONFIG=${DOCKER_CONFIG:/usr/local/lib/docker/cli-plugins}
    mkdir -p $DOCKER_CONFIG/cli-plugins
    curl -SL https://github.com/docker/compose/releases/download/v5.0.1/docker-compose-linux-x86_64 -o $DOCKER_CONFIG/cli-plugins/docker-compose
    chmod +x /usr/local/lib/docker/cli-plugins/docker-compose
fi
#verify
if [[ -x "$(docker compose version)" ]]; then
    echo "Error: Docker Compose installation failed." >&2
    exit 1
fi

echo "Installing Rust"
curl --proto '=https' --tlsv1.2 -sSf https://sh.rustup.rs | sh -s -- -y
source $HOME/.cargo/env

echo "Installing C++ build tools"
sudo apt install -y build-essential 2>/dev/null

echo "Installing libpcap"
sudo apt install -y libpcap-dev 2>/dev/null

# start containers and build custom zeek image
IFACE=$(get_default_nic)
sed -i "s/command: zeek -C -i ens160 local/command: zeek -C -i $IFACE local/" ../docker/docker-compose.yml
docker compose -f ../docker/docker-compose.yml up -d

# compile the parser binary
echo "Compiling parser binary with cargo"
cargo build --release --manifest-path ../bin/pcap2ch/Cargo.toml

# direct user to run the parsers via the parser.sh script
echo "Installation complete! You can now run the parsers using the parser.sh script in this directory."
echo "You will also need to save the information below to connect the frontend visualizer to the database:"
echo "ClickHouse Host: $(ip route get 1.1.1.1 | grep -oP 'src \K\S+')"
echo "ClickHouse Port: $CLICKHOUSE_PORT"
echo "ClickHouse Database: $CLICKHOUSE_DB"
echo "ClickHouse User: $CLICKHOUSE_USER"
