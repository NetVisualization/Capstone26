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
        public List<IPAddress> ips;
        public ushort[] src_ports;
        public List<l7_proto> l7_protos;
        public string device_type;
    }

    // Database Connection Object (Get)
    [Serializable]
    public struct Connection
    {
        public List<IPAddress> node_a;
        public List<IPAddress> node_b;
        public int pkts;
        public int bytes;
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
    // Sub-connection where everything is divided by protocol (Get)
    public struct SubConnection
    {
        public List<IPAddress> node_a;
        public List<IPAddress> node_b;
        public PhysicalAddress node_a_macs;
        public PhysicalAddress node_b_macs;
        public l7_proto protocol;
        public int pkts;
        public int bytes;
        public DateTime first_seen;
        public GameObject node1;
        public GameObject node2;
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

    public List<l7_proto> intArrayToAppLayerList(UInt16[] ints)
    {
        List<l7_proto> myAppLayerValues = new List<l7_proto>();
        foreach (var i in ints)
        {
            l7_proto currentProto = (l7_proto)i;
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
        //cmd.CommandText = "SELECT * FROM nodes WHERE nodes.first_seen > toDateTime(@time);";
        cmd.CommandText = @"
        SELECT
            hex(mac) AS mac_hex,
            pkts,
            bytes,
            first_seen,
            last_seen,
            degree,
            ips,
            src_ports,
            l7_protos,
            device_type
        FROM nodes
        WHERE nodes.first_seen > toDateTime(@time);";
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
            string macHex = reader.GetString(reader.GetOrdinal("mac_hex"));
            dbRecord.mac = System.Net.NetworkInformation.PhysicalAddress.Parse(macHex);
            dbRecord.pkts = Convert.ToInt32(reader.GetValue(reader.GetOrdinal("pkts")));
            dbRecord.bytes = Convert.ToInt32(reader.GetValue(reader.GetOrdinal("bytes")));
            dbRecord.first_seen = reader.GetDateTime(reader.GetOrdinal("first_seen"));
            dbRecord.last_seen = reader.GetDateTime(reader.GetOrdinal("last_seen"));
            dbRecord.degree = Convert.ToInt32(reader.GetValue(reader.GetOrdinal("degree")));
            var ips = (System.Net.IPAddress[])reader["ips"];
            dbRecord.ips = ips.ToList();
            dbRecord.src_ports = (ushort[])reader.GetValue(reader.GetOrdinal("src_ports"));
            UInt16[] l7s = (UInt16[])reader.GetValue(reader.GetOrdinal("l7_protos"));
            dbRecord.l7_protos = intArrayToAppLayerList(l7s);
            dbRecord.device_type = Convert.ToString(reader.GetValue(reader.GetOrdinal("device_type")));

            nodes.Add(dbRecord);
            Debug.Log(dbRecord.mac);
        }

        return nodes;
    }

    /// <summary>
    /// Get all connections from the database after a certain time
    /// </summary>
    public List<Connection> getConnectionsAfter(DateTime time)
    {
        using var cmd = _connection.CreateCommand();
        // cmd.CommandText = "SELECT * FROM connections WHERE connections.first_seen > toDateTime(@time);";

        cmd.CommandText = @"SELECT
                LEAST(hex(src_mac),hex(dst_mac)) AS mac_a,
                groupUniqArray(if(src_mac <= dst_mac, src_ip, dst_ip)) AS ip_a,
                GREATEST(hex(src_mac), hex(dst_mac)) AS mac_b,
                groupUniqArray(if(src_mac <= dst_mac, dst_ip, src_ip)) AS ip_b,
                MIN(first_seen) AS first_seen,
                MAX(last_seen) AS last_seen,
                SUM(pkts) AS pkts,
                SUM(bytes) AS bytes,
            
                -- Aggregate ports for each node
                arrayDistinct(arrayFlatten(groupArrayArray(if(src_mac <= dst_mac, src_ports, dst_ports)))) AS ports_a,
                arrayDistinct(arrayFlatten(groupArrayArray(if(src_mac <= dst_mac, dst_ports, src_ports)))) AS ports_b,
                -- Get L7 protocols for each side of the connection
                arrayDistinct(arrayFlatten(groupArrayArray(if(src_mac <= dst_mac, l7_protos, l7_protos)))) AS l7_protos_a,
                -- Conditionally aggregate L7 protocols for mac_b
                arrayDistinct(arrayFlatten(groupArrayArray(if(src_mac <= dst_mac, l7_protos, l7_protos)))) AS l7_protos_b,

                -- Aggregate distinct L3 protocols (connection-wide)
                arrayDistinct(arrayFlatten(groupArrayArray(protos))) AS protos
            FROM
                connections
            WHERE 
                connections.first_seen > toDateTime(@time)
            GROUP BY
                mac_a,
                mac_b;";

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
            var ips = (System.Net.IPAddress[])reader["ip_a"];
            dbRecord.node_a = ips.ToList();
            ips = (System.Net.IPAddress[])reader["ip_b"];
            dbRecord.node_b = ips.ToList();
            //dbRecord.node_a = (List<IPAddress>)reader.GetValue(reader.GetOrdinal("ip_a"));
            //dbRecord.node_b = (List<IPAddress>)reader.GetValue(reader.GetOrdinal("ip_b"));
            dbRecord.pkts = Convert.ToInt32(reader.GetValue(reader.GetOrdinal("pkts")));
            dbRecord.bytes = Convert.ToInt32(reader.GetValue(reader.GetOrdinal("bytes")));
            dbRecord.first_seen = reader.GetDateTime(reader.GetOrdinal("first_seen"));
            dbRecord.last_seen = reader.GetDateTime(reader.GetOrdinal("last_seen"));
            dbRecord.protos = new List<l4_proto>();

            string macHex = reader.GetString(reader.GetOrdinal("mac_a"));
            dbRecord.node_a_macs = System.Net.NetworkInformation.PhysicalAddress.Parse(macHex);
            macHex = reader.GetString(reader.GetOrdinal("mac_b"));
            dbRecord.node_b_macs = System.Net.NetworkInformation.PhysicalAddress.Parse(macHex);

            dbRecord.node_a_src_ports = (ushort[])reader.GetValue(reader.GetOrdinal("ports_a"));
            dbRecord.node_a_dst_ports = (ushort[])reader.GetValue(reader.GetOrdinal("ports_b"));

            // combine all app layer protocols into one list for now
            var l7s = (ushort[])reader.GetValue(reader.GetOrdinal("l7_protos_a"));
            dbRecord.node_a_l7_protos = intArrayToAppLayerList(l7s);
            l7s = (ushort[]) reader.GetValue(reader.GetOrdinal("l7_protos_b"));
            dbRecord.node_a_l7_protos.Union(intArrayToAppLayerList(l7s));

            conns.Add(dbRecord);
        }

        return conns;
    }

    public List<SubConnection> subdivideConnectionByProtocol(Connection conn)
    {
        List<SubConnection> subConns = new List<SubConnection>();
        try
        {
            foreach (var proto in conn.node_a_l7_protos)
            {
                SubConnection subConn = new SubConnection();
                subConn.node_a = conn.node_a;
                subConn.node_b = conn.node_b;
                subConn.node_a_macs = conn.node_a_macs;
                subConn.node_b_macs = conn.node_b_macs;
                subConn.protocol = proto;
                // set other fields to zero/empty
                subConn.pkts = 0;
                subConn.bytes = 0;
                subConn.first_seen = new DateTime(1970, 01, 01);

                subConns.Add(subConn);
            }
            return subConns;
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            return null;
        }


        
    }
}