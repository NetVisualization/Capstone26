using System;
using System.Collections.Generic;
using System.Net;

namespace models
{
    // Database Node Object (Get)
    [System.Serializable]
    public struct Node
    {
        public System.Net.NetworkInformation.PhysicalAddress mac;
        public UInt64 pkts;
        public UInt64 bytes;
        public DateTime first_seen;
        public DateTime last_seen;
        public int degree;
        public List<IPAddress> ips;
        public ushort[] src_ports;
        public List<l7_proto> l7_protos;
        public string device_type;
    }
}