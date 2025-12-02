use anyhow::Result;
use bytes::Bytes;
use chrono::TimeZone;
use chrono::{DateTime, Utc};
use pcap::{Capture, Device};
use std::sync::{
    Arc,
    atomic::{AtomicBool, Ordering},
};
use tokio::sync::mpsc;
use tokio::time::{Duration, interval};

use crate::db::{DbPacket, DbRawBytes};
use crate::ingest;

// Tune these if you like
const CHANNEL_CAPACITY: usize = 50_000;
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
    pub ch_user: String,
    pub ch_password: String,
}

pub fn list_interfaces() -> Result<()> {
    let devices = Device::list()?;
    println!("Available capture interfaces:");
    for (i, d) in devices.iter().enumerate() {
        println!(
            "  [{}] name={} desc={}",
            i + 1,
            d.name,
            d.desc.as_deref().unwrap_or("")
        );
    }
    Ok(())
}

pub async fn run(opts: LiveOpts) -> Result<()> {
    // Build ClickHouse client (with credentials, like your File path)
    let mut client = clickhouse::Client::default()
        .with_url(&opts.ch_url)
        .with_database(&opts.ch_db);
    if !opts.ch_user.is_empty() {
        client = client.with_user(&opts.ch_user);
    }
    if !opts.ch_password.is_empty() {
        client = client.with_password(&opts.ch_password);
    }

    // Stop flag (q + Enter or Ctrl-C)
    let stop = Arc::new(AtomicBool::new(false));
    // Handle Ctrl-C and 'q' + Enter to stop (async-safe)
    {
        let stop = stop.clone();
        tokio::spawn(async move {
            use tokio::io::{self, AsyncBufReadExt};
            let mut stdin = io::BufReader::new(io::stdin());
            let mut line = String::new();

            tracing::info!("Press 'q' then Enter or Ctrl-C to stop capture...");
            tokio::select! {
                _ = tokio::signal::ctrl_c() => {
                    tracing::info!("Ctrl-C detected, stopping capture...");
                    stop.store(true, Ordering::Relaxed);
                }
                _ = async {
                    loop {
                        line.clear();
                        if stdin.read_line(&mut line).await.is_err() {
                            break;
                        }
                        if line.trim().eq_ignore_ascii_case("q") {
                            tracing::info!("'q' pressed, stopping capture...");
                            stop.store(true, Ordering::Relaxed);
                            break;
                        }
                    }
                } => {}
            }
        });
    }

    // capture → worker channel
    let (tx, rx) = mpsc::channel::<CapturedFrame>(CHANNEL_CAPACITY);
    // startup status
    let (init_tx, init_rx) = std::sync::mpsc::channel::<Result<()>>();

    // Spawn blocking capture thread
    let iface = opts.iface.clone();
    let filter = opts.filter.clone();
    let snaplen = opts.snaplen;
    let stop_for_cap = stop.clone();
    let cap_thread = std::thread::spawn(move || {
        // pass init_tx into capture_loop so it can signal *once it's ready*
        let _ = capture_loop(iface, filter, snaplen, tx, stop_for_cap, Some(init_tx));
    });

    // Wait for capture init
    match init_rx.recv() {
        Ok(Ok(())) => tracing::info!("live: capture thread initialized"),
        Ok(Err(e)) => {
            tracing::error!("live: capture init failed: {e}");
            let _ = cap_thread.join();
            return Err(e);
        }
        Err(_) => {
            tracing::error!("live: capture thread died before init");
            let _ = cap_thread.join();
            return Err(anyhow::anyhow!("capture thread died before init"));
        }
    }

    // Start worker until sender closes; flush on size and on wall clock
    worker_loop(
        client,
        rx,
        opts.ch_head_table,
        opts.ch_raw_table,
        opts.batch_size,
    )
    .await;

    let _ = cap_thread.join();
    Ok(())
}

struct CapturedFrame {
    ts: DateTime<Utc>,
    data: Bytes,
}

fn lookup_device_by_name(name: &str) -> Option<Device> {
    if name.trim().is_empty() {
        return None;
    }

    // 1) Treat numeric value as 1-based index into Device::list().
    if let Ok(idx) = name.trim().parse::<usize>() {
        if let Ok(devices) = Device::list() {
            if idx >= 1 && idx <= devices.len() {
                let dev = devices.into_iter().nth(idx - 1)?;
                tracing::info!(
                    index = idx,
                    dev_name = %dev.name,
                    dev_desc = ?dev.desc,
                    "live: resolved interface by index"
                );
                return Some(dev);
            } else {
                tracing::warn!(
                    index = idx,
                    total = devices.len(),
                    "live: interface index out of range"
                );
            }
        } else {
            tracing::error!("live: failed to list pcap devices while resolving index");
        }
    }

    // 2) Exact name match (works on Linux & Windows).
    if let Ok(devices) = Device::list() {
        if let Some(dev) = devices.into_iter().find(|d| d.name == name) {
            tracing::info!(
                iface = name,
                dev_name = %dev.name,
                dev_desc = ?dev.desc,
                "live: resolved interface by exact name"
            );
            return Some(dev);
        }
    }

    let needle = name.to_ascii_lowercase();

    // 3) Windows-specific: match description or partial internal name/GUID.
    #[cfg(target_os = "windows")]
    {
        if let Ok(devices) = Device::list() {
            if let Some(dev) = devices.into_iter().find(|d| {
                d.desc
                    .as_deref()
                    .map(|desc| desc.to_ascii_lowercase().contains(&needle))
                    .unwrap_or(false)
            }) {
                tracing::info!(
                    iface = name,
                    dev_name = %dev.name,
                    dev_desc = ?dev.desc,
                    "live: resolved Windows interface by description"
                );
                return Some(dev);
            }
        }

        if let Ok(devices) = Device::list() {
            if let Some(dev) = devices.into_iter().find(|d| {
                d.name.to_ascii_lowercase().contains(&needle)
            }) {
                tracing::info!(
                    iface = name,
                    dev_name = %dev.name,
                    dev_desc = ?dev.desc,
                    "live: resolved Windows interface by partial name"
                );
                return Some(dev);
            }
        }
    }

    // 4) Fallback fuzzy match on any platform (name or description).
    if let Ok(devices) = Device::list() {
        if let Some(dev) = devices.into_iter().find(|d| {
            d.name.to_ascii_lowercase().contains(&needle)
                || d.desc
                    .as_deref()
                    .map(|desc| desc.to_ascii_lowercase().contains(&needle))
                    .unwrap_or(false)
        }) {
            tracing::info!(
                iface = name,
                dev_name = %dev.name,
                dev_desc = ?dev.desc,
                "live: resolved interface by fuzzy match"
            );
            return Some(dev);
        }
    }

    tracing::warn!(iface = name, "live: failed to resolve interface via Device::list()");
    None
}


fn capture_loop(
    iface: String,
    filter: Option<String>,
    snaplen: i32,
    tx: tokio::sync::mpsc::Sender<CapturedFrame>,
    stop: Arc<AtomicBool>,
    init_tx: Option<std::sync::mpsc::Sender<anyhow::Result<()>>>, // NEW
) -> anyhow::Result<()> {
    // Build & open
    let mut cap = {
        let inactive = if iface.trim().is_empty() {
            let dev = Device::lookup()?
                .ok_or_else(|| anyhow::anyhow!("No default capture device"))?;
            tracing::info!(
                dev_name = %dev.name,
                dev_desc = ?dev.desc,
                "live: using default capture device"
            );
            Capture::from_device(dev)?
        } else if let Some(dev) = lookup_device_by_name(&iface) {
            tracing::info!(
                iface = %iface,
                dev_name = %dev.name,
                dev_desc = ?dev.desc,
                "live: using resolved capture device"
            );
            Capture::from_device(dev)?
        } else {
            tracing::warn!(
                iface = %iface,
                "live: could not resolve interface, falling back to raw name for pcap"
            );
            Capture::from_device(iface.as_str())?
        };

        inactive
            .promisc(true)
            .snaplen(snaplen)
            .timeout(10)
            .immediate_mode(true)
            .open()?
    };


    cap = cap.setnonblock()?;
    // Datalink log (optional)
    let dl = cap.get_datalink();
    tracing::info!("live: datalink = {:?}", dl);

    // Apply BPF (propagate errors to main)
    if let Some(f) = filter {
        cap.filter(&f, true)?;
        tracing::info!(filter=%f, "live: BPF filter set");
    } else {
        tracing::info!("live: no BPF filter");
    }

    // >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
    // Tell main thread we are READY so it can start the worker immediately.
    if let Some(tx_ready) = &init_tx {
        let _ = tx_ready.send(Ok(()));
    }
    tracing::info!("live: capture started (press 'q' + Enter or Ctrl-C to stop)");
    // <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<

    // Read loop
    let mut cap_count: u64 = 0;
    while !stop.load(Ordering::Relaxed) {
        match cap.next_packet() {
            Ok(pkt) => {
                // ... your timestamp & send code ...
                let ts: DateTime<Utc> = {
                    #[allow(deprecated)]
                    let secs = pkt.header.ts.tv_sec as i64;
                    let usec = pkt.header.ts.tv_usec as u32;
                    let nanos = (usec as u64) * 1000;
                    chrono::Utc
                        .timestamp_opt(secs, nanos as u32)
                        .single()
                        .unwrap_or_else(|| chrono::Utc::now())
                };
                let data = Bytes::copy_from_slice(pkt.data);
                if tx.blocking_send(CapturedFrame { ts, data }).is_err() {
                    tracing::warn!("live: worker receiver closed; stopping capture");
                    break;
                }
                cap_count += 1;
                if cap_count % 50 == 0 {
                    tracing::info!(cap_count, "live: captured frames");
                }
            }

            // Non-blocking: no packet ready right now; yield briefly.
            Err(pcap::Error::NoMorePackets) | Err(pcap::Error::TimeoutExpired) => {
                std::thread::sleep(Duration::from_millis(10));
                continue;
            }

            Err(e) => {
                // If error occurs after init, propagate/log and exit
                if let Some(tx_ready) = &init_tx {
                    let _ = tx_ready.send(Err(anyhow::anyhow!(e.to_string())));
                }
                return Err(e.into());
            }
        }
    }

    tracing::info!("live: exiting capture loop");
    drop(tx); // close channel so worker_loop sees EOF and flushes
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

    // Wall-clock ticker: flush even with no new packets
    let mut tick = interval(Duration::from_millis(BATCH_FLUSH_MS));
    tick.tick().await; // skip immediate first tick

    let mut seen: u64 = 0;

    loop {
        tokio::select! {
            // Packet path
            maybe_frame = rx.recv() => {
                match maybe_frame {
                    Some(frame) => {
                        if let Some((p, r)) = crate::parse::parse_frame_to_records(frame.ts, &frame.data) {
                            heads.push(p);
                            raws.push(r);
                        }
                        seen += 1;

                        // size-based flush
                        if heads.len() >= batch_size || raws.len() >= batch_size {
                            match flush(&client, &heads_table, &raw_table, &mut heads, &mut raws, batch_size).await {
                                Ok((nh, nr)) => tracing::info!(seen, nh, nr, "live: size-based flush"),
                                Err(e) => tracing::error!("live: size-based flush error: {e}"),
                            }
                        }
                    }
                    None => {
                        // sender closed -> final flush, then exit
                        match flush(&client, &heads_table, &raw_table, &mut heads, &mut raws, batch_size).await {
                            Ok((nh, nr)) => tracing::info!(seen, nh, nr, "live: final flush"),
                            Err(e) => tracing::error!("live: final flush error: {e}"),
                        }
                        break;
                    }
                }
            }

            // Time-based path
            _ = tick.tick() => {
                if !(heads.is_empty() && raws.is_empty()) {
                    match flush(&client, &heads_table, &raw_table, &mut heads, &mut raws, batch_size).await {
                        Ok((nh, nr)) => tracing::info!(seen, nh, nr, "live: tick flush"),
                        Err(e) => tracing::error!("live: tick flush error: {e}"),
                    }
                }
            }
        }
    }
}

async fn flush(
    client: &clickhouse::Client,
    heads_table: &str,
    raw_table: &str,
    heads: &mut Vec<DbPacket>,
    raws: &mut Vec<DbRawBytes>,
    batch_size: usize,
) -> Result<(usize, usize)> {
    let nh = heads.len();
    let nr = raws.len();

    if nh > 0 {
        let moved = std::mem::take(heads);
        ingest::run_file_and_insert(client, heads_table, moved, batch_size).await?;
    }
    if nr > 0 {
        let moved = std::mem::take(raws);
        ingest::insert_bytes(client, raw_table, moved, batch_size).await?;
    }

    Ok((nh, nr))
}
