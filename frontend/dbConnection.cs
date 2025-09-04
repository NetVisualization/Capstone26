using System;
using System.Net;
using UnityEngine;
using System.Collections.Generic;
using ClickHouse.Ado;
using ClickHouse.Client.ADO;
using ClickHouse.Client.ADO.Parameters;
using ClickHouse.Client.Utility;
using ClickHouse.Client.Copy; 
using TMPro;
using Newtonsoft.Json;

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
    struct Node
    {
        public int _id;
        public NetworkInformation.PhysicalAddress MACaddr;
        public IPAddress IPaddr;
        public string DeviceType;
        public int NumConnections;
        public int NumPackets;
    }

    // Database Connection Object (Get)
    [Serializable]
    struct Connection
    {
        public int _id;
        public IPAddress NodeA_IP;
        public IPAddress NodeB_IP;
        public int NumPackets;
        public DateTime LastPacketTimestamp;
    }

    // Database Packet Object (Get)
    [Serializable]
    struct Packet
    {
        public int _id;
        public DateTime Timestamp;
        public NetworkInformation.PhysicalAddress SourceMAC;
        public NetworkInformation.PhysicalAddress DestinationMAC;
        public IPAddress SourceIP;
        public IPAddress DestinationIP;
        public short SourcePort;
        public short DestinationPort;
        public string Protocol;
        public int Length;
        public byte[] PayloadHex;
    }

    private ClickHouse.Client.ADO.ClickHouseConnection? _connection;

    NodeSpawnerScript nodeSpawnerScript;
    public GameObject nodeSpawner;

    public Node[] nodes;
    public Connection[] connections;
    public Packet[] packets;

    public void connect(string host, string port, string dbName, string user, string pass)
    {
        // clickhouse connection info
        string connectionString = $"Host={host};Database={dbName};port={port};Username={user};Password={pass}";
        _connection = new ClickHouse.Client.ADO.ClickHouseConnection(connectionString);
        _connection.Open();
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
        public int getNodeCount()
        {
            return 0;
        }

        /// <summary>
        /// Get the total number of connections in the database.
        /// </summary>
        public int getConnectionCount()
        {
            return 0;
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

    void Awake()
    {
        nodeSpawnerScript = nodeSpawner.GetComponent<NodeSpawnerScript>();

        var settings = MongoClientSettings.FromConnectionString(connectionUri);

        // Set the ServerApi field of the settings object to set the version of the Stable API on the client
        settings.ServerApi = new ServerApi(ServerApiVersion.V1);
        
        // Create a new client and connect to the server
        client = new MongoClient(settings);
    }

    public void RequestNodes()
    {
        IMongoCollection<BsonDocument> collection = client.GetDatabase("NetworkVisualization").GetCollection<BsonDocument>("Nodes");
        var filter = Builders<BsonDocument>.Filter.Empty;
        IFindFluent<BsonDocument, BsonDocument> document = collection.Find(filter);

        foreach(BsonDocument doc in document.ToList())
        {
            string json = doc.ToJson();

            nodes.Add(JsonConvert.DeserializeObject<Node>(json));
        }
    }

    public void RequestConnections()
    {
        IMongoCollection<BsonDocument> collection = client.GetDatabase("NetworkVisualization").GetCollection<BsonDocument>("Connections");
        var filter = Builders<BsonDocument>.Filter.Empty;
        IFindFluent<BsonDocument, BsonDocument> document = collection.Find(filter);

        foreach (BsonDocument doc in document.ToList())
        {
            string json = doc.ToJson();

            connections.Add(JsonConvert.DeserializeObject<Connection>(json));
        }
    }

    public void RequestPackets()
    {
        IMongoCollection<BsonDocument> collection = client.GetDatabase("NetworkVisualization").GetCollection<BsonDocument>("Packets");
        var filter = Builders<BsonDocument>.Filter.Empty;
        IFindFluent<BsonDocument, BsonDocument> document = collection.Find(filter);

        foreach (BsonDocument doc in document.ToList())
        {
            string json = doc.ToJson();

            packets.Add(JsonConvert.DeserializeObject<Packet>(json));
        }
    }
}