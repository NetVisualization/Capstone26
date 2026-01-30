using System;

public enum l2_proto : byte
{
    ETHERNET = 1,
    ARP = 2,
    PPPoE = 3,
    IEEE802_11 = 4,
    OTHER = 127
}

public enum l3_proto : byte
{
    IPv4 = 1,
    IPv6 = 2,
    MPLS = 3,
    ARP = 4,
    NON_IP = 127
}

public enum l4_proto : ushort
{
    NONE = 0,
    ICMP = 1,
    TCP = 6,
    UDP = 17,
    SCTP = 132
}

public enum l7_proto : ushort
{
    UNKNOWN = 0,
    SSH = 22,
    SMTP = 25,
    DNS = 53,
    DHCP = 67,
    HTTP = 80,
    POP3 = 110,
    NTP = 123,
    IMAP = 143,
    TLS = 443,
    SMB = 445,
    QUIC = 1000,
    SSDP = 1900,
    RDP = 3389,
    MDNS = 5353
}