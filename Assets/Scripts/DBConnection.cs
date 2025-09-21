using System;
using System.Net;
using UnityEngine;
using System.Collections.Generic;
using ClickHouse.Client.ADO;
using ClickHouse.Client.ADO.Parameters;
using TMPro;
using Newtonsoft.Json;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Data.Common;
using System.Threading.Tasks;
using UnityEditor.PackageManager;
using MongoDB.Driver.Core;
using System.Xml.Serialization;
using System.Data;
using static Unity.Burst.Intrinsics.X86.Avx;
using System.Linq;
using UnityEngine.InputSystem.XR;
using Unity.VisualScripting;
using System.Reflection;
using static UnityEditor.PlayerSettings;
using Unity.Profiling;
using System.Net.NetworkInformation;

public class DBConnection : MonoBehaviour
{

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

    // Database Node Object (Get)
    [System.Serializable]
    public struct Node
    {
        public System.Net.NetworkInformation.PhysicalAddress mac;
        public int pkts;
        public int bytes;
        public DateTime first_seen;
        public DateTime last_seen;
        public int degree;
        public IPAddress[] ips;
        public ushort[] src_ports;
        public List<l7_proto> l7_protos;
        public string device_type;
    }

    // Database Connection Object (Get)
    [Serializable]
    public struct Connection
    {
        public IPAddress node_a;
        public IPAddress node_b;
        public int pkts;
        public int bytes;
        public DateTime first_seen;
        public DateTime last_seen;
        public List<l4_proto> protos;
        public PhysicalAddress[] node_a_macs;
        public PhysicalAddress[] node_b_macs;
        public ushort[] node_a_src_ports;
        public ushort[] node_a_dst_ports;
        public List<l7_proto> node_a_l7_protos;
    }

    // Database Packet Object (Get)
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

    public string DB_NAME = "net";
    public string DB_HOST = "10.200.1.13";
    public string DB_PORT = "8123";
    public string DB_USER = "capstone";
    public string DB_PASS = "boogle";
    private ClickHouseConnection _connection;

    NodeSpawnerScript nodeSpawnerScript;
    public GameObject nodeSpawner;

    //public Node[] nodes;
    //public Connection[] connections;

    //public Packet[] packets;

    /// <summary>
    /// Connect to the database when the game engine is started
    /// </summary>
    private async void Start()
    {
        try
        {
            await connect(DB_HOST, DB_PORT, DB_NAME, DB_USER, DB_PASS);
            //var version = await ExecuteCommand("SELECT version()");
            //Debug.Log($"Connection to Clickhouse {version}");

            //int result = await getNodeCountAsync();
            //Debug.Log($"There are {result} nodes");

            List<Node> nodes = getNodesAfter(new DateTime(2025, 09, 06, 16, 06, 01));

            foreach (var field in typeof(Node).GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
          //      Debug.Log($"{field.Name} = {field.GetValue(nodes[1])}");
            }

        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }

    /// <summary>
    /// Close the database connection and free any held resources
    /// </summary>
    private void OnDestroy()
    {
        if (_connection != null)
        {
            _connection.Close();
            _connection.Dispose();
            _connection = null;
        }
    }

    /// <summary>
    /// Connect to the Clickhouse Database
    /// </summary>
    /// <param name="host"></param>
    /// <param name="port"></param>
    /// <param name="dbName"></param>
    /// <param name="user"></param>
    /// <param name="pass"></param>
    /// <returns></returns>
    public async Task connect(string host, string port, string dbName, string user, string pass)
    {
        // clickhouse connection info
        string connectionString = $"Host={host};Database={dbName};Port={port};Username={user};Password={pass}";
        _connection = new ClickHouse.Client.ADO.ClickHouseConnection(connectionString);
        await _connection.OpenAsync();
        Debug.Log($"Connected to the {dbName} database.");
    }


    public async Task<object> ExecuteCommand(string text)
    {
        var cmd = _connection.CreateCommand();
        cmd.CommandText = text;
        return await cmd.ExecuteScalarAsync();
    }

    public async Task<object> ExecuteNonQuery(string text)
    {
        var cmd = _connection.CreateCommand();
        cmd.CommandText = text;
        return await cmd.ExecuteNonQueryAsync();
    }

    public DbDataReader ExecuteReader(string text)
    {
        var cmd = _connection.CreateCommand();
        cmd.CommandText = text;
        var reader = cmd.ExecuteReader();
        if (reader != null)
        {
            return reader;
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Get the total number of nodes in the database.
    /// </summary>
    public async Task<int> getNodeCountAsync()
    {
        var result = await ExecuteCommand("SELECT COUNT(*) FROM nodes;");
        return Convert.ToInt32(result);
    }

    /// <summary>
    /// Get the total number of connections in the database.
    /// </summary>
    public async Task<int> getConnectionCountAsync()
    {
        var result = await ExecuteCommand("SELECT COUNT(*) FROM connections;");
        return Convert.ToInt32(result);
    }

    public List<l7_proto> StringArrayToAppLayerList(String[] strings)
    {
        List<l7_proto> myAppLayerValues = new List<l7_proto>();
        foreach (var s in strings) {
            l7_proto currentProto = (l7_proto)Enum.Parse(typeof(l7_proto), s);
            myAppLayerValues.Add(currentProto);
        }
        return myAppLayerValues;
    }

    /// <summary>
    /// Get a list of node structs that were added to the database after a certain time.
    /// </summary>
    public List<Node> getNodesAfter(DateTime time)
    {
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = "SELECT * FROM nodes WHERE nodes.first_seen > toDateTime(@time);";

        cmd.Parameters.Add(new ClickHouseDbParameter
        {
            ParameterName = "time",
            Value = time
        });

        List<Node> nodes = new List<Node>();

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            Node dbRecord = new Node();
            string macFromDb = Convert.ToString(reader.GetValue(reader.GetOrdinal("mac")));
            dbRecord.mac = PhysicalAddress.Parse(macFromDb);
            dbRecord.pkts = Convert.ToInt32(reader.GetValue(reader.GetOrdinal("pkts")));
            dbRecord.bytes = Convert.ToInt32(reader.GetValue(reader.GetOrdinal("bytes")));
            dbRecord.first_seen = reader.GetDateTime(reader.GetOrdinal("first_seen"));
            dbRecord.last_seen = reader.GetDateTime(reader.GetOrdinal("last_seen"));
            dbRecord.degree = Convert.ToInt32(reader.GetValue(reader.GetOrdinal("degree")));
            dbRecord.ips = (IPAddress[])reader.GetValue(reader.GetOrdinal("ips"));
            dbRecord.src_ports = (ushort[])reader.GetValue(reader.GetOrdinal("src_ports"));
            String[] l7s = (String[])reader.GetValue(reader.GetOrdinal("l7_protos"));
            dbRecord.l7_protos = StringArrayToAppLayerList(l7s);
            dbRecord.device_type = Convert.ToString(reader.GetValue(reader.GetOrdinal("device_type")));

            nodes.Add(dbRecord);
        }

        return nodes;
    }

    /// <summary>
    /// Get all connections from the database after a certain time
    /// </summary>
    public List<Connection> getConnectionsAfter(DateTime time)
    {
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = "SELECT * FROM connections WHERE connections.first_seen > toDateTime(@time);";

        cmd.Parameters.Add(new ClickHouseDbParameter
        {
            ParameterName = "time",
            Value = time
        });

        List<Connection> conns = new List<Connection>();

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            Connection dbRecord = new Connection();
            dbRecord.node_a = (IPAddress)reader.GetValue(reader.GetOrdinal("node_a"));
            dbRecord.node_b = (IPAddress)reader.GetValue(reader.GetOrdinal("node_a"));
            dbRecord.pkts = Convert.ToInt32(reader.GetValue(reader.GetOrdinal("pkts")));
            dbRecord.bytes = Convert.ToInt32(reader.GetValue(reader.GetOrdinal("bytes")));
            dbRecord.first_seen = reader.GetDateTime(reader.GetOrdinal("first_seen"));
            dbRecord.last_seen = reader.GetDateTime(reader.GetOrdinal("last_seen"));
            dbRecord.protos = new List<l4_proto>();
            byte[] l4s = (byte[])reader.GetValue(reader.GetOrdinal("protos"));
            foreach (byte l4 in l4s) dbRecord.protos.Add((l4_proto)l4);
            dbRecord.node_a_macs = [];
            string[] macsFromDb = (string[])reader.GetValue(reader.GetOrdinal("node_a_macs"));
            foreach (string mac in macsFromDb) dbRecord.node_a_macs.Append(PhysicalAddress.Parse(mac));
            dbRecord.node_b_macs = [];
            macsFromDb = (string[])reader.GetValue(reader.GetOrdinal("node_b_macs"));
            foreach (string mac in macsFromDb) dbRecord.node_b_macs.Append(PhysicalAddress.Parse(mac));
            dbRecord.node_a_src_ports = (ushort[])reader.GetValue(reader.GetOrdinal("node_a_src_ports"));
            dbRecord.node_a_dst_ports = (ushort[])reader.GetValue(reader.GetOrdinal("node_a_dst_ports"));
            String[] l7s = (String[])reader.GetValue(reader.GetOrdinal("node_a_l7_protos"));
            dbRecord.node_a_l7_protos = StringArrayToAppLayerList(l7s);

            conns.Add(dbRecord);
        }

        return conns;
    }
}