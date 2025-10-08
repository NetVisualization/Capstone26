use anyhow::Result;
use bytes::Bytes;
use chrono::TimeZone;
use chrono::{DateTime, Utc};
use pcap::{Capture, Device, Linktype};
use std::io::Read;
use std::sync::{
    Arc,
    atomic::{AtomicBool, Ordering},
};
use std::time::SystemTime;
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
    {
        let stop = stop.clone();
        std::thread::spawn(move || {
            eprintln!("Press 'q' then Enter to stop…");
            for b in std::io::stdin().bytes() {
                if matches!(b, Ok(b'q') | Ok(b'Q')) {
                    stop.store(true, Ordering::Relaxed);
                    break;
                }
            }
        });
    }
    {
        let stop = stop.clone();
        tokio::spawn(async move {
            let _ = tokio::signal::ctrl_c().await;
            stop.store(true, Ordering::Relaxed);
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
        let res = capture_loop(iface, filter, snaplen, tx, stop_for_cap);
        let _ = init_tx.send(res.map(|_| ()));
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
    Device::list().ok()?.into_iter().find(|d| d.name == name)
}

fn capture_loop(
    iface: String,
    filter: Option<String>,
    snaplen: i32,
    tx: mpsc::Sender<CapturedFrame>,
    stop: Arc<AtomicBool>,
) -> Result<()> {
    // Build and open the capture handle
    let mut cap = {
        let inactive = if iface.is_empty() {
            let dev =
                Device::lookup()?.ok_or_else(|| anyhow::anyhow!("No default capture device"))?;
            Capture::from_device(dev)?
        } else if let Some(dev) = lookup_device_by_name(&iface) {
            Capture::from_device(dev)?
        } else {
            Capture::from_device(iface.as_str())?
        };

        inactive
            .promisc(true)
            .snaplen(snaplen)
            .timeout(10)
            .immediate_mode(true)
            .open()?
    };

    // Log datalink to catch loopback/non-ethernet mistakes
    let dl = cap.get_datalink();
    tracing::info!("live: datalink = {:?}", dl);
    if dl != Linktype(1) {
        tracing::warn!(
            "live: non-Ethernet datalink {:?}; parse(from_ethernet) will drop frames on this iface",
            dl
        );
    }

    if let Some(f) = filter {
        cap.filter(&f, true)?;
        tracing::info!(filter=%f, "live: BPF filter set");
    } else {
        tracing::info!("live: no BPF filter");
    }

    tracing::info!("live: capture started (press 'q' + Enter or Ctrl-C to stop)");

    let mut cap_count: u64 = 0;
    loop {
        if stop.load(Ordering::Relaxed) {
            tracing::info!("live: stop flag set, exiting capture loop");
            break;
        }

        match cap.next_packet() {
            Ok(pkt) => {
                // Use libpcap timestamp if present, else fallback to now()
                #[allow(deprecated)]
                let ts: DateTime<Utc> = {
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
            Err(pcap::Error::TimeoutExpired) => continue,
            Err(e) => return Err(e.into()),
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
