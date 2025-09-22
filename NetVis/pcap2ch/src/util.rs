use std::{fs::File, io::Read, path::Path};
use tracing_subscriber::{EnvFilter, fmt};

#[derive(Debug, Clone, Copy)]
pub enum CaptureFormat {
    Pcap,
    PcapNg,
}

pub fn init_tracing() {
    let filter = EnvFilter::try_from_default_env().unwrap_or_else(|_| EnvFilter::new("info"));
    fmt().with_env_filter(filter).init();
}

pub fn read_all<P: AsRef<Path>>(path: P) -> anyhow::Result<Vec<u8>> {
    let mut v = Vec::new();
    File::open(path.as_ref())?.read_to_end(&mut v)?;
    Ok(v)
}

pub fn sniff_format(buf: &[u8]) -> anyhow::Result<CaptureFormat> {
    if buf.len() < 4 {
        anyhow::bail!("file too small to detect format");
    }
    let m_be = u32::from_be_bytes([buf[0], buf[1], buf[2], buf[3]]);
    match m_be {
        0x0A0D0D0A => Ok(CaptureFormat::PcapNg),
        0xA1B2C3D4 | 0xD4C3B2A1 | 0xA1B23C4D | 0x4D3CB2A1 => Ok(CaptureFormat::Pcap),
        _ => anyhow::bail!("unrecognized capture magic: 0x{m_be:08x}"),
    }
}

pub fn escape_sql_single_quotes(s: &str) -> String {
    s.replace('\'', "''")
}
