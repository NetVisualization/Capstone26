// Sub-connection where everything is divided by protocol (Get)
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using UnityEngine;

namespace models
{
    public struct SubConnection
    {
        public List<IPAddress> node_a;
        public List<IPAddress> node_b;
        public PhysicalAddress node_a_macs;
        public PhysicalAddress node_b_macs;
        public l7_proto protocol;
        public UInt64 pkts;
        public UInt64 bytes;
        public DateTime first_seen;
        public GameObject node1;
        public GameObject node2;
    }
}