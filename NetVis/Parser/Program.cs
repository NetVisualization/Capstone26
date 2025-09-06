using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;

using ClickHouse.Client.ADO;
using ClickHouse.Client.Copy;

using PacketDotNet;
using SharpPcap;
using SharpPcap.LibPcap; // CaptureFileReaderDevice + PacketCapture

enum L2Proto : sbyte { ETHERNET = 1, ARP = 2, PPPoE = 3, IEEE802_11 = 4, OTHER = 127 }
enum L3Proto : sbyte { IPv4 = 1, IPv6 = 2, MPLS = 3, ARP = 4, NON_IP = 127 }
enum L4Proto : short { NONE = 0, ICMP = 1, TCP = 6, UDP = 17, SCTP = 132 }

class Program
{
    const string CH = "Host=100.111.112.111;Username=capstone;Password=boogle;Compression=true;";
    const int BATCH = 5000;

    static readonly string Table = "net.packets";
    static readonly string[] Cols = {
        "dst_ip","dst_mac","dst_port","iface","ip_ttl",
        "l2_proto","l3_proto","l4_proto","l7_proto",
        "packet_len","payload","payload_ref","payload_sha256",
        "src_ip","src_mac","src_port","tcp_flags","ts"
    };

    static int Main(string[] args)
    {
        if (args.Length < 1) { Console.WriteLine("Usage: dotnet run -- <pcap(.ng)> [iface]"); return 2; }
        var pcap = args[0];
        var iface = args.Length > 1 ? args[1] : Path.GetFileNameWithoutExtension(pcap);
        if (!File.Exists(pcap)) { Console.Error.WriteLine($"Not found: {pcap}"); return 1; }

        using var reader = new CaptureFileReaderDevice(pcap);
        reader.Open();

        using var conn = new ClickHouseConnection(CH);
        conn.Open();

        using var bulk = new ClickHouseBulkCopy(conn) { DestinationTableName = Table, ColumnNames = Cols };
        using var sha256 = SHA256.Create();

        var batch = new List<object[]>(BATCH);
        int total = 0, inserted = 0;

        // SharpPcap 6.x loop: use out PacketCapture, convert immediately to RawCapture
        while (reader.GetNextPacket(out PacketCapture pc) == GetPacketStatus.PacketRead)
        {
            var cap = pc.GetPacket(); // RawCapture; safe beyond this point
            total++;

            var row = BuildRow(cap, iface, sha256);
            if (row != null) batch.Add(row);

            // Flush synchronously (no await) to avoid ref-struct across async boundary
            if (batch.Count >= BATCH)
            {
                bulk.WriteToServerAsync(batch).GetAwaiter().GetResult();
                inserted += batch.Count;
                batch.Clear();
                Console.WriteLine($"Inserted {inserted:n0} rows…");
            }
        }

        if (batch.Count > 0)
        {
            bulk.WriteToServerAsync(batch).GetAwaiter().GetResult();
            inserted += batch.Count;
        }

        Console.WriteLine($"Done. Parsed: {total:n0}, Inserted: {inserted:n0}");
        return 0;
    }

    static object[]? BuildRow(RawCapture cap, string iface, SHA256 sha256)
    {
        var tsUtc = cap.Timeval.Date.ToUniversalTime();

        var pkt = Packet.ParsePacket(cap.LinkLayerType, cap.Data);
        var eth = pkt.Extract<EthernetPacket>();

        var l2 = L2Proto.OTHER;
        var srcMac = new byte[6];
        var dstMac = new byte[6];
        if (eth != null)
        {
            l2 = L2Proto.ETHERNET;
            var s = eth.SourceHardwareAddress?.GetAddressBytes();
            var d = eth.DestinationHardwareAddress?.GetAddressBytes();
            if (s is { Length: 6 }) srcMac = s;
            if (d is { Length: 6 }) dstMac = d;
        }

        var ip4 = pkt.Extract<IPv4Packet>();
        var ip6 = pkt.Extract<IPv6Packet>();
        var l3 = L3Proto.NON_IP;

        byte[] srcIp = IPAddress.IPv6None.GetAddressBytes();
        byte[] dstIp = IPAddress.IPv6None.GetAddressBytes();
        byte? ttl = null;

        if (ip6 != null)
        {
            l3 = L3Proto.IPv6;
            srcIp = ip6.SourceAddress.GetAddressBytes();
            dstIp = ip6.DestinationAddress.GetAddressBytes();
            ttl = (byte)ip6.HopLimit;
        }
        else if (ip4 != null)
        {
            l3 = L3Proto.IPv4;
            srcIp = ip4.SourceAddress.MapToIPv6().GetAddressBytes();
            dstIp = ip4.DestinationAddress.MapToIPv6().GetAddressBytes();
            ttl = (byte)ip4.TimeToLive;
        }
        else if (eth?.Type == EthernetType.Arp)
        {
            l3 = L3Proto.ARP;
        }

        var tcp = pkt.Extract<TcpPacket>();
        var udp = pkt.Extract<UdpPacket>();
        var icmp4 = pkt.Extract<IcmpV4Packet>();
        var icmp6 = pkt.Extract<IcmpV6Packet>();

        var l4 = L4Proto.NONE;
        ushort? sport = null, dport = null, flags = null;

        if (tcp != null)
        {
            l4 = L4Proto.TCP;
            sport = (ushort)tcp.SourcePort;
            dport = (ushort)tcp.DestinationPort;
            flags = TcpFlagsMask(tcp); // version-agnostic
        }
        else if (udp != null)
        {
            l4 = L4Proto.UDP;
            sport = (ushort)udp.SourcePort;
            dport = (ushort)udp.DestinationPort;
        }
        else if (icmp4 != null || icmp6 != null)
        {
            l4 = L4Proto.ICMP;
        }

        var l7 = "";

        byte[] payload = Array.Empty<byte>();
        if (tcp?.PayloadData is { Length: > 0 }) payload = tcp.PayloadData;
        else if (udp?.PayloadData is { Length: > 0 }) payload = udp.PayloadData;

        var payloadSha = sha256.ComputeHash(payload);
        ushort pktLen = (ushort)Math.Clamp(cap.Data.Length, 0, ushort.MaxValue);

        if (srcIp.Length != 16 || dstIp.Length != 16 || srcMac.Length != 6 || dstMac.Length != 6 || payloadSha.Length != 32)
            return null;

        return new object[] {
            dstIp,
            dstMac,
            dport is ushort dp ? dp : (object?)null,
            iface,
            ttl is byte t ? t : (object?)null,
            (sbyte)l2,
            (sbyte)l3,
            (short)l4,
            l7,
            pktLen,
            payload,
            (object?)null,        // payload_ref
            payloadSha,
            srcIp,
            srcMac,
            sport is ushort sp ? sp : (object?)null,
            flags is ushort f ? f : (object?)null,
            tsUtc
        };
    }

    // Works across PacketDotNet versions (property names vary)
    static ushort TcpFlagsMask(TcpPacket tcp)
    {
        int f = 0;
        if (HasFlag(tcp, "Fin", "Finish", "Finished")) f |= 0x01;
        if (HasFlag(tcp, "Syn", "Synchronize"))        f |= 0x02;
        if (HasFlag(tcp, "Rst", "Reset"))              f |= 0x04;
        if (HasFlag(tcp, "Psh", "Push"))               f |= 0x08;
        if (HasFlag(tcp, "Ack", "Acknowledgment"))     f |= 0x10;
        if (HasFlag(tcp, "Urg", "Urgent"))             f |= 0x20;
        if (HasFlag(tcp, "ECN", "Ecn", "Ece"))         f |= 0x40;
        if (HasFlag(tcp, "CWR"))                       f |= 0x80;
        return (ushort)f;
    }

    static bool HasFlag(TcpPacket tcp, params string[] names)
    {
        var t = tcp.GetType();
        foreach (var n in names)
        {
            var p = t.GetProperty(n);
            if (p != null && p.PropertyType == typeof(bool))
                return (bool)(p.GetValue(tcp) ?? false);
        }
        return false;
    }
}
