use clap::{Parser, Subcommand};
use tracing_subscriber::{EnvFilter, fmt};

/// Packet parser — supports live capture (later) and offline pcap/pcapng (next milestone).
#[derive(Debug, Parser)]
#[command(author, version, about)]
struct Args {
    /// ClickHouse URL (e.g., http://localhost:8123) — used in Milestone 3
    #[arg(long, env = "CH_URL", default_value = "http://localhost:8123")]
    ch_url: String,

    /// ClickHouse database name
    #[arg(long, env = "CH_DB", default_value = "default")]
    ch_db: String,

    /// ClickHouse table (must be net.packets in your schema)
    #[arg(long, env = "CH_TABLE", default_value = "net.packets")]
    ch_table: String,

    #[command(subcommand)]
    cmd: Cmd,
}

#[derive(Debug, Subcommand)]
enum Cmd {
    /// Parse an offline capture file (.pcap or .pcapng)
    File {
        /// Path to capture file
        path: String,
        /// Optional: limit number of packets (debug/testing)
        #[arg(long)]
        limit: Option<usize>,
    },
    /// (Milestone 4) Live-capture from a network interface
    Live {
        /// Interface name, e.g., eth0
        iface: String,
        /// Optional BPF filter string
        #[arg(long)]
        filter: Option<String>,
        /// Snaplen bytes
        #[arg(long, default_value_t = 262144)]
        snaplen: i32,
        /// Batch size before flush
        #[arg(long, default_value_t = 10_000)]
        batch_size: usize,
    },
}

fn init_tracing() {
    let filter = EnvFilter::try_from_default_env().unwrap_or_else(|_| EnvFilter::new("info"));
    fmt().with_env_filter(filter).init();
}

fn main() -> anyhow::Result<()> {
    init_tracing();
    let args = Args::parse();

    match args.cmd {
        Cmd::File { path, limit } => {
            // Milestone 2 will implement: read magic, choose pcap vs pcapng, parse
            tracing::info!(%path, ?limit, "offline mode (to be implemented next)");
        }
        Cmd::Live {
            iface,
            filter,
            snaplen,
            batch_size,
        } => {
            // Milestone 4 will implement live capture
            tracing::info!(%iface, ?filter, snaplen, batch_size, "live mode (to be implemented later)");
        }
    }
    Ok(())
}
