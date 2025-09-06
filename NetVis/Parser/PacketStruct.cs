namespace Parser;

// PacketStruct.cs
using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;

// Enums mirror your SQL enums exactly
public enum L2Proto : sbyte
{
    ETHERNET = 1, ARP = 2, PPPoE = 3, IEEE802_11 = 4, OTHER = 127
}

public enum L3Proto : sbyte
{
    IPv4 = 1, IPv6 = 2, MPLS = 3, ARP = 4, NON_IP = 127
}

public enum L4Proto : short
{
    NONE = 0, ICMP = 1, TCP = 6, UDP = 17, SCTP = 132
}

// Strongly-typed row that matches net.packets
public readonly record struct PacketRow(
    DateTime Ts,                 // DateTime64(6,'UTC')
    string Iface,                // LowCardinality(String)
    L2Proto L2_Proto,            // Enum8
    L3Proto L3_Proto,            // Enum8
    L4Proto L4_Proto,            // Enum16
    string L7_Proto,             // LowCardinality(String)
    PhysicalAddress SrcMac,      // FixedString(6)
    PhysicalAddress DstMac,      // FixedString(6)
    IPAddress SrcIp,             // IPv6 (use v4-mapped for IPv4)
    IPAddress DstIp,             // IPv6
    ushort? SrcPort,             // Nullable(UInt16)
    ushort? DstPort,             // Nullable(UInt16)
    byte? IpTtl,                 // Nullable(UInt8)
    ushort? TcpFlags,            // Nullable(UInt16)
    ushort PacketLen,            // UInt16
    byte[] Payload,              // String (binary; ClickHouse will LZ4 it per table)
    byte[] PayloadSha256,        // FixedString(32)
    string? PayloadRef           // Nullable(String)
)
{
    // Validation helpers (optional but helpful)
    public void Validate()
    {
        if (SrcMac.GetAddressBytes().Length != 6) throw new ArgumentException("SrcMac must be 6 bytes");
        if (DstMac.GetAddressBytes().Length != 6) throw new ArgumentException("DstMac must be 6 bytes");
        if (PayloadSha256.Length != 32) throw new ArgumentException("PayloadSha256 must be 32 bytes");
        if (SrcIp.GetAddressBytes().Length != 16) throw new ArgumentException("SrcIp must be 16 bytes (IPv6)");
        if (DstIp.GetAddressBytes().Length != 16) throw new ArgumentException("DstIp must be 16 bytes (IPv6)");
        if (Ts.Kind != DateTimeKind.Utc) throw new ArgumentException("Ts must be UTC");
    }
}

// Tiny conversion utilities for parameter binding
static class ChTypes
{
    public static byte[] Ip(IPAddress ip) => ip.GetAddressBytes();         // 16 bytes for IPv6
    public static IPAddress Ip(byte[] raw) => new IPAddress(raw);
    public static byte[] Mac(PhysicalAddress mac) => mac.GetAddressBytes(); // 6 bytes
    public static PhysicalAddress Mac(byte[] raw) => new PhysicalAddress(raw.Select(b => b).ToArray());
}
