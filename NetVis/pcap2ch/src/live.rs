use anyhow::Result;
use bytes::Bytes;
use chrono::{DateTime, Utc};
use pcap::{Capture, Device};
use std::time::{Duration, SystemTime};
use tokio::sync::mpsc;
use tokio::time::Instant;

use crate::db::{DbPacket, DbRawBytes};
use crate::ingest;

const CHANNEL_CAPACITY: usize = 50_000;
const BATCH_TARGET: usize = 5_000;
const BATCH_FLUSH_MS: u64 = 300;

pub struct LiveOpts {
    pub iface: String,
    pub filter: Option<String>,
    pub snaplen: i32,
    pub batch_size: usize,
    pub ch_url: String,
    pub ch_db: String,
    pub ch_head_table: String,
    pub ch_raw_table: String,
}

pub async fn run(opts: LiveOpts) -> Result<()> {
    let client = clickhouse::Client::default()
        .with_url(&opts.ch_url)
        .with_database(&opts.ch_db);

    // 1) Channel from capture thread -> async worker
    let (tx, rx) = mpsc::channel::<CapturedFrame>(CHANNEL_CAPACITY);

    // 2) Spawn blocking capture thread
    let iface = opts.iface.clone();
    let filter = opts.filter.clone();
    let snaplen = opts.snaplen;
    std::thread::spawn(move || {
        if let Err(e) = capture_loop(iface, filter, snaplen, tx) {
            eprintln!("capture error: {e}");
        }
    });

    // 3) Single worker for now (Receiver is single-consumer)
    worker_loop(
        client,
        rx,
        opts.ch_head_table.clone(),
        opts.ch_raw_table.clone(),
        opts.batch_size.max(BATCH_TARGET),
    )
    .await;

    Ok(())
}

struct CapturedFrame {
    ts: DateTime<Utc>,
    data: Bytes,
}

fn lookup_device_by_name(name: &str) -> Option<Device> {
    match Device::list() {
        Ok(list) => list.into_iter().find(|d| d.name == name),
        Err(_) => None,
    }
}

fn capture_loop(
    iface: String,
    filter: Option<String>,
    snaplen: i32,
    tx: mpsc::Sender<CapturedFrame>,
) -> Result<()> {
    // Pick device: explicit iface name wins; otherwise default
    let mut cap = if iface.is_empty() {
        let dev = Device::lookup()?.ok_or_else(|| anyhow::anyhow!("No default capture device"))?;
        Capture::from_device(dev)?
    } else if let Some(dev) = lookup_device_by_name(&iface) {
        Capture::from_device(dev)?
    } else {
        // Fall back to using the string name directly
        Capture::from_device(iface.as_str())?
    }
    .promisc(true)
    .snaplen(snaplen)
    .timeout(10)
    .immediate_mode(true)
    .open()?;

    if let Some(f) = filter {
        let _ = cap.filter(&f, true);
    }

    // Blocking read loop
    while let Ok(pkt) = cap.next_packet() {
        // If you want libpcap timestamps, you can read pkt.header.ts here (platform-dependent).
        let ts: DateTime<Utc> = SystemTime::now().into();
        let data = Bytes::copy_from_slice(pkt.data);

        if tx.blocking_send(CapturedFrame { ts, data }).is_err() {
            break; // receiver closed
        }
    }
    Ok(())
}

async fn worker_loop(
    client: clickhouse::Client,
    mut rx: mpsc::Receiver<CapturedFrame>,
    heads_table: String,
    raw_table: String,
    batch_size: usize,
) {
    let mut heads: Vec<DbPacket> = Vec::with_capacity(batch_size);
    let mut raws: Vec<DbRawBytes> = Vec::with_capacity(batch_size);
    let mut last_flush = Instant::now();

    while let Some(frame) = rx.recv().await {
        // Parse → (DbPacket, DbRawBytes)
        if let Some((p, r)) = crate::parse::parse_frame_to_records(frame.ts, &frame.data) {
            heads.push(p);
            raws.push(r);
        }

        let due_by_size = heads.len() + raws.len() >= batch_size;
        let due_by_time = last_flush.elapsed() >= Duration::from_millis(BATCH_FLUSH_MS);

        if due_by_size || due_by_time {
            let _ = flush(
                &client,
                &heads_table,
                &raw_table,
                &mut heads,
                &mut raws,
                batch_size,
            )
            .await;
            last_flush = Instant::now();
        }
    }

    // Final flush on shutdown
    let _ = flush(
        &client,
        &heads_table,
        &raw_table,
        &mut heads,
        &mut raws,
        batch_size,
    )
    .await;
}

async fn flush(
    client: &clickhouse::Client,
    heads_table: &str,
    raw_table: &str,
    heads: &mut Vec<DbPacket>,
    raws: &mut Vec<DbRawBytes>,
    batch_size: usize,
) -> Result<()> {
    if !heads.is_empty() {
        ingest::run_file_and_insert(client, heads_table, std::mem::take(heads), batch_size).await?;
    }
    if !raws.is_empty() {
        ingest::insert_bytes(client, raw_table, std::mem::take(raws), batch_size).await?;
    }
    Ok(())
}
