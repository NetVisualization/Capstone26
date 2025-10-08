mod cli;
mod db;
mod ingest;
mod live;
mod parse;

mod summary;
mod util;

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
            tracing::info!(%iface, ?filter, snaplen, batch_size, "starting live capture");

            let opts = live::LiveOpts {
                iface,
                filter,
                snaplen,
                batch_size,
                ch_url: args.ch_url,
                ch_db: args.ch_db,
                ch_head_table: args.ch_head_table,
                ch_raw_table: args.ch_raw_table,
                ch_user: args.ch_user,
                ch_password: args.ch_password,
            };
            live::run(opts).await?;
        }
    }

    Ok(())
}
