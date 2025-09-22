use clap::{Parser, Subcommand};

/// Packet parser/ingestor — offline pcap/pcapng; live capture next.
#[derive(Debug, Parser)]
#[command(author, version, about)]
pub struct Cli {
    /// ClickHouse URL (e.g., http://localhost:8123)
    #[arg(long, env = "CH_URL", default_value = "http://localhost:8123")]
    pub ch_url: String,

    /// ClickHouse database name
    #[arg(long, env = "CH_DB", default_value = "default")]
    pub ch_db: String,

    /// ClickHouse table (must be net.packets)
    #[arg(long, env = "CH_TABLE", default_value = "net.packets")]
    pub ch_table: String,

    /// ClickHouse user
    #[arg(long, env = "CH_USER", default_value = "default")]
    pub ch_user: String,

    /// ClickHouse password
    #[arg(long, env = "CH_PASSWORD", default_value = "")]
    pub ch_password: String,

    #[command(subcommand)]
    pub cmd: Cmd,
}

#[derive(Debug, Subcommand)]
pub enum Cmd {
    /// Parse an offline capture file (.pcap or .pcapng)
    File {
        /// Path to capture file (positional)
        #[arg(long)]
        path: String,
        /// Optional: limit number of packets (debug/testing)
        #[arg(long)]
        limit: Option<usize>,
        /// Insert into ClickHouse
        #[arg(long, default_value_t = false)]
        insert: bool,
        /// Batch size for inserts
        #[arg(long, default_value_t = 5_000)]
        batch_size: usize,
    },
    /// Live capture (to be implemented later)
    Live {
        iface: String,
        #[arg(long)]
        filter: Option<String>,
        #[arg(long, default_value_t = 262_144)]
        snaplen: i32,
        #[arg(long, default_value_t = 10_000)]
        batch_size: usize,
    },
}
