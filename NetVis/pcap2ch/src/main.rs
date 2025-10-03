mod cli;
mod db; // keep your existing db.rs
mod ingest;
mod parse;

mod summary;
mod util; // keep your existing lib.rs types

use clap::Parser;
use clickhouse::Client;

use crate::db::DbRawBytes;

#[tokio::main]
async fn main() -> anyhow::Result<()> {
    util::init_tracing();
    let args = cli::Cli::parse();

    match args.cmd {
        cli::Cmd::File {
            path,
            limit,
            insert,
            batch_size,
        } => {
            let mut raw_rows: Vec<DbRawBytes> = Vec::new();
            let rows = parse::run_file(&path, limit, &mut raw_rows)?;

            if insert {
                let mut client = Client::default()
                    .with_url(&args.ch_url)
                    .with_database(&args.ch_db);
                if !args.ch_user.is_empty() {
                    client = client.with_user(&args.ch_user);
                }
                if !args.ch_password.is_empty() {
                    client = client.with_password(&args.ch_password);
                }

                ingest::run_file_and_insert(&client, &args.ch_head_table, rows, batch_size).await?;
                ingest::insert_bytes(&client, &args.ch_raw_table, raw_rows, batch_size).await?;
            } else {
                tracing::info!(count = rows.len(), "parsed rows (no insert)");
            }
        }
        cli::Cmd::Live {
            iface,
            filter,
            snaplen,
            batch_size,
        } => {
            tracing::info!(%iface, ?filter, snaplen, batch_size, "live mode (todo)");
        }
    }

    Ok(())
}
