# Live Packet Capture Ingest
---
## Choosing a node
The current iteration of the parser can only view traffic that is directed through the machine. You should choose a node that interacts with several other nodes in the network.  

## Compiling from source - first time only
1. Clone the repo, or at least place the `parser/` directory on the host that will run the live traffic scanner.  
2. From the root of the repo, acccess, the live ingest and parser tool in the following directory: `cd parser/NetVis/pcap2ch`  
3. Install Rust with the following command: `curl --proto '=https' --tlsv1.2 -sSf https://sh.rustup.rs | sh`
4. Install the C++ build essentials tools:
	- Windows: https://visualstudio.microsoft.com/vs/features/cplusplus/
	- Linux: `sudo apt install build-essential`
5. Ensure the Rust Compiler points to the npcap library, which is distributed with the project for Windows. Or, install the libpcap library for linux:
	- Windows: `$env:LIB = ".\lib\npcap-sdk-1.15\Lib\x64"`
	- Linux: `sudo apt install libpcap-dev`
6. Build with `cargo build --release`
7. Verify the executable file exists in the directory: `cd target/release/ && ls ./pcap2ch`. (You should see `./pcap2ch`)  

## Configure and Run
1. Ensure you are in the directory `parser/Netvis/pcap2ch/target/release` relative to the root of the repo
2. Run the pcap2ch binary with the following options:
    - set the command to `live`
    - set `--ch-url` to point to the database. For more info on database configuration, see `docker-setup.md`
    - set `--ch-db` to `net` unless you changed the default name of the database
    - set `--ch-user` and `--ch-password` to match the `CLICKHOUSE_USER` and `CLICKHOUSE_PASSWORD` variables set in `docker-compose.yml`
    - set `--iface` to the interface you want to capture on
    - Do not set any other options, leave them as default
---
```
Usage: pcap2ch [OPTIONS] <COMMAND>

Commands:
  file    Parse an offline capture file (.pcap or .pcapng)
  live    Live capture
  ifaces  List available capture interfaces
  help    Print this message or the help of the given subcommand(s)

Options:
      --ch-url <CH_URL>                ClickHouse URL (e.g., http://localhost:8123) [env: CH_URL=] [default: http://localhost:8123]
      --ch-db <CH_DB>                  ClickHouse database name [env: CH_DB=] [default: default]
      --ch-head-table <CH_HEAD_TABLE>  ClickHouse table (must be net.packets) [env: CH_HEAD_TABLE=] [default: net.packets]
      --ch-raw-table <CH_RAW_TABLE>    [env: CH_RAW_TABLE=] [default: net.raw_bytes]
      --ch-user <CH_USER>              ClickHouse user [env: CH_USER=] [default: capstone]
      --ch-password <CH_PASSWORD>      ClickHouse password [env: CH_PASSWORD=] [default: ]
  -h, --help                           Print help
  -V, --version                        Print version
```
3. Verify that
