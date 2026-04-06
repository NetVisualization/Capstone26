using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using UnityEngine;

namespace models
{
    // Database Connection Object (Get)
    [Serializable]
    /// <summary> WARNING: this model is currently deprecated and replaced
    /// with subconnectins, please don't add any features using it </summary>
    public struct Connection
    {
        public List<IPAddress> node_a;
        public List<IPAddress> node_b;
        public UInt64 pkts;
        public UInt64 bytes;
        public DateTime first_seen;
        public DateTime last_seen;
        public List<l4_proto> protos;
        public PhysicalAddress node_a_macs;
        public PhysicalAddress node_b_macs;
        public ushort[] node_a_src_ports;
        public ushort[] node_a_dst_ports;
        public List<l7_proto> node_a_l7_protos;
        public GameObject node1;
        public GameObject node2;
    }
}