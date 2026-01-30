// Database Packet Object (Get)
using System;
using System.Net;

namespace models
{
    [Serializable]
    public struct Packet
    {
        public int _id;
        public DateTime Timestamp;
        public System.Net.NetworkInformation.PhysicalAddress SourceMAC;
        public System.Net.NetworkInformation.PhysicalAddress DestinationMAC;
        public IPAddress SourceIP;
        public IPAddress DestinationIP;
        public short SourcePort;
        public short DestinationPort;
        public string Protocol;
        public int Length;
        public byte[] PayloadHex;
    }
}
