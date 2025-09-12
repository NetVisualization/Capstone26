using System;
using System.Net;
using UnityEngine;
using System.Collections.Generic;
using ClickHouse.Client.ADO;
using TMPro;
using Newtonsoft.Json;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Data.Common;
using System.Threading.Tasks;
using UnityEditor.PackageManager;
using MongoDB.Driver.Core;
using System.Xml.Serialization;

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


    // Database Node Object (Get)
    [Serializable]
    public struct Node
    {
        public int _id;
        public System.Net.NetworkInformation.PhysicalAddress MACaddr;
        public IPAddress IPaddr;
        public string DeviceType;
        public int NumConnections;
        public int NumPackets;
    }

    // Database Connection Object (Get)
    [Serializable]
    public struct Connection
    {
        public int _id;
        public IPAddress NodeA_IP;
        public IPAddress NodeB_IP;
        public int NumPackets;
        public DateTime LastPacketTimestamp;
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

    public Node[] nodes;
    public Connection[] connections;
    public Packet[] packets;

    /// <summary>
    /// Connect to the database when the game engine is started
    /// </summary>
    private async void Start()
    {
        try
        {
            await connect(DB_HOST, DB_PORT, DB_NAME, DB_USER, DB_PASS);
            var version = await ExecuteCommand("SELECT version()");
            Debug.Log($"Connection to Clickhouse {version}");

            var result = await ExecuteCommand("SELECT COUNT(*) FROM nodes;");
            Debug.Log($"There are {result} nodes");
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
    public async void getNodeCount(Action<int> onComplete)
    {
        var result = await ExecuteCommand("SELECT COUNT(*) FROM nodes;");
        int count = Convert.ToInt32(result);
        onComplete?.Invoke(count);
    }

    /// <summary>
    /// Get the total number of connections in the database.
    /// </summary>
    public void getConnectionCount()
    {
        return;
    }

    /// <summary>
    /// Get a node by its index in the DB.
    /// </summary>
    public void GetNode(int nodeId)
    {
        return;
    }

    /// <summary>
    /// Get a connection by its index in the DB.
    /// </summary>
    public void GetConnection(int connectionId)
    {
        return;
    }
}