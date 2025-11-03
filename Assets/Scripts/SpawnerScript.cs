using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.UI;

public class NodeSpawnerScript : MonoBehaviour
{

    // *****************************************************************************************************************************
    // Class Variables
    // *****************************************************************************************************************************

    // Public objects, must be connected to assets in the main window
    public GameObject NodePrefab;
    public GameObject connectionPrefab;

    public Material ConnectionLight;
    public Material ConnectionMed;
    public Material ConnectionHeavy;

    public GameObject dbInteractor;
    [SerializeField] DBConnection dbConnection;
    public FilterSystem filterSystem;
    int timer;

    public Toggle liveToggle;
    public VendorLookupScript MACVendorMap;

    // Used for spacing of nodes with same vendor
    Dictionary<char, float> zOffset = new();

    // Dictionary of connections and nodes
    public Dictionary<string, GameObject> NodeObjects = new Dictionary<string, GameObject>();
    public Dictionary<string, GameObject> ConnectionObjects = new Dictionary<string, GameObject>();

    // Node ID lookup by IP address
    Dictionary<string, string> NodeIDAddressRel = new Dictionary<string, string>();  ////// this can def be removed

    List<DBConnection.Node> nodeList = new List<DBConnection.Node>();
    List<DBConnection.Connection> ConnectionList = new List<DBConnection.Connection>(); // get rid of this
    List<DBConnection.SubConnection> SubConnectionList = new List<DBConnection.SubConnection>();
    List<DBConnection.SubConnection> SubConnectionIPList = new List<DBConnection.SubConnection>();
    List<GameObject> IpNodeList = new List<GameObject>();
    List<GameObject> spawnedNodes = new List<GameObject>();
    public List<GameObject> spawnedConnections = new List<GameObject>();

    public static void PrintListContents<T>(List<T> listToPrint)
    {
        if (listToPrint == null)
        {
            Debug.Log("The list is null.");
            return;
        }

        Debug.Log($"--- Printing contents of List<{typeof(T).Name}> ---");

        if (listToPrint.Count == 0)
        {
            Debug.Log("The list is empty.");
            return;
        }

        foreach (T item in listToPrint)
        {
            Debug.Log(item);
        }
        Debug.Log("-------------------------------------------");
    }

    void Awake()
    {
        // Get the dbInteractor Script within the spawning script
        //dbConnection = dbInteractor.GetComponent<DBConnection>();

        //foreach (char c in "QWERTYUIOPASDFGHJKLZXCVBNM")   ////// would be better to actually pull the vendor names rather than this approach
        //{
        //   zOffset[c] = 2f;
        //}
    }

    void Start()
    {
        timer = 0;
        if (dbConnection == null)
        {
            Debug.Log("dbConnection is Null");
        }
        else
        {
            nodeList = dbConnection.getNodesAfter(new DateTime(2025, 09, 06, 16, 05, 01));
            ConnectionList = dbConnection.getConnectionsAfter(new DateTime(2025, 09, 06, 16, 05, 01));
            // divide into sub-connections
            foreach (var conn in ConnectionList)
            {
                List<DBConnection.SubConnection> connDivided = null;
                connDivided = dbConnection.subdivideConnectionByProtocol(conn);
                if (connDivided != null)
                {
                    SubConnectionList.AddRange(connDivided);
                }
            }
            Debug.Log($"{ConnectionList.Count} connections");
            Debug.Log($"{SubConnectionList.Count} sub-connections");
        }
        MakeNodes();
        MakeConnections();
        //dbConnection.RequestNodes();
        //dbConnection.RequestConnections();
        //dbConnection.RequestPackets();

        //filterSystem.CreateWhitelistButtons(dbConnection.nodes);
    }

    // Update is called once per frame
    //void Update()
    //{
    //    // If using live data, regularly pull from the DB
    //    if (liveToggle.isOn)
    //    {
    //        timer++;  // Can maybe be optimized using InvokeRepeating()

    //        if (timer >= 100) // ~50 calls to Update per second
    //        {
    //            timer = 0;

    //            dbConnection.RequestNodes();
    //            dbConnection.RequestConnections();
    //            dbConnection.RequestPackets();
    //        }
    //    }

    //    MakeNodes();
    //    MakeConnections();
    //    UpdateConnections();
    //}

    // Getter functions for nodes, connections, and packets (can be called by other classes)
    public Dictionary<string, GameObject> GetNodes()
    {
        return NodeObjects;
    }

    public Dictionary<string, GameObject> GetConnections()
    {
        return ConnectionObjects;
    }

    //public List<DBConnection.Packet> GetPackets()
    //{
    //    //        return dbConnection.packets;
    //}

    void MakeNodes()
    {
        foreach (DBConnection.Node node in nodeList)
        {
            //dbNode.Initialize
            // Create a new GameObject based on the prefab
            GameObject nodeObject = Instantiate(NodePrefab);
            NodeInfo info = nodeObject.GetComponent<NodeInfo>();
            info.Initialize(node);

            //// Set parent and position
            info.transform.position = placeNodev1(nodeObject);

            spawnedNodes.Add(nodeObject);

            //Add a node object to the dictionary with its id as the key
            // NodeObjects[node._id.oid] = nodeObject;

            //Add the node to the lookup table
            //        NodeIDAddressRel.Add(node.IPaddr, node._id.oid);
        }
    }


    void MakeConnections()
    {
        foreach (DBConnection.SubConnection connection in SubConnectionList.ToList())
        {        
            GameObject firstNode = FindNodeByMac(connection.node_a_macs);
            GameObject secondNode = FindNodeByMac(connection.node_b_macs);

            if (firstNode == null || secondNode == null)
            {
                Debug.LogWarning($"Skipping connection between {connection.node_a_macs} and {connection.node_b_macs} because one or both nodes are missing.");
                continue;  // Skip this connection and move on to the next
            }

            GameObject connectionObject = Instantiate(connectionPrefab);
            ConnectionInfo connInfo = connectionObject.GetComponent<ConnectionInfo>();
            connInfo.data.node1 = firstNode;
            connInfo.data.node2 = secondNode;
            connInfo.Initialize(connection);
            spawnedConnections.Add(connectionObject);

            ConnectNodes(connectionObject.transform, firstNode.transform, secondNode.transform);
            //check if there are multiple ip's to a node
            if (CountIP(firstNode) > 1)
            {
                SplitNode(firstNode);
                foreach (var node in IpNodeList)
                {
                    splitConnection(connection, firstNode, node);
                }
            }
            if (CountIP(secondNode) > 1)
            {
                SplitNode(secondNode);
                foreach (var node in IpNodeList)
                {
                    splitConnection(connection, secondNode, node);
                }
            }
        }
    }
    GameObject FindNodeByMac(PhysicalAddress mac)
    {
        foreach (var node in spawnedNodes)
        {
            var info = node.GetComponent<NodeInfo>();
            if (mac.Equals(info.data.mac))
                return node;
        }
        return null;
    }

    //public void SplitNode(GameObject node)
    //{
    //    var info = node.GetComponent<NodeInfo>();
    //        foreach(var ip in info.data.ips)
    //        {
    //        GameObject nodeObject = Instantiate(NodePrefab);
    //        var newNodeInfo = nodeObject.GetComponent<NodeInfo>();
    //        DBConnection.Node newNode = new DBConnection.Node();
    //            newNode.pkts = info.data.pkts;
    //            newNode.bytes = info.data.bytes;
    //            newNode.last_seen = info.data.last_seen;
    //            newNode.first_seen = info.data.first_seen;
    //            newNode.degree = info.data.degree;
    //            newNode.ips.Add(ip);
    //            newNode.src_ports = info.data.src_ports;
    //            newNode.l7_protos = info.data.l7_protos;
    //            newNode.device_type = info.data.device_type;
    //            info.Initialize(newNode);
    //            IpNodeList.Add(nodeObject);
    //        placeNodev1(nodeObject);

    //        }
    //}
    public void SplitNode(GameObject node)
    {
        var info = node.GetComponent<NodeInfo>();

        foreach (var ip in info.data.ips)
        {
            if (NodeExists(info.data.mac, ip))
            {
                Debug.Log($"Node for {info.data.mac} - {ip} already exists. Skipping.");
                continue;
            }
            Debug.Log(ip.ToString());
            GameObject nodeObject = Instantiate(NodePrefab);
            var newNodeInfo = nodeObject.GetComponent<NodeInfo>();

            DBConnection.Node newNode = new DBConnection.Node
            {
                pkts = info.data.pkts,
                bytes = info.data.bytes,
                last_seen = info.data.last_seen,
                first_seen = info.data.first_seen,
                degree = info.data.degree,
                src_ports = info.data.src_ports,
                l7_protos = info.data.l7_protos,
                device_type = info.data.device_type,
                mac = info.data.mac, //  keep same MAC
                ips = new List<IPAddress>()
            };
            newNode.ips.Add(ip);

            newNodeInfo.Initialize(newNode);  //  Initialize the NEW node, not the old one
            IpNodeList.Add(nodeObject);
            spawnedNodes.Add(nodeObject);     //  Track for future lookup
            nodeObject.transform.position = placeNodev1(nodeObject);
        }
    }

    public void splitConnection(DBConnection.SubConnection connection, GameObject Original, GameObject NewNode)
    {
        NodeInfo originalInfo = Original.GetComponent<NodeInfo>();
        NodeInfo newInfo = NewNode.GetComponent<NodeInfo>();

        if (originalInfo == null || newInfo == null)
        {
            Debug.LogError("splitConnection: One or both provided GameObjects are missing NodeInfo!");
            return;
        }

        DBConnection.SubConnection newConnection = new DBConnection.SubConnection();

        // --- Copy over connection attributes ---
        newConnection.protocol = connection.protocol;
        newConnection.pkts = connection.pkts / 2;
        newConnection.bytes = connection.bytes / 2;
        newConnection.first_seen = connection.first_seen;

        // --- Assign MACs ---
        newConnection.node_a_macs = originalInfo.data.mac;
        newConnection.node_b_macs = newInfo.data.mac;

        // --- Assign IP lists ---
        newConnection.node_a = new List<IPAddress>(originalInfo.data.ips);
        newConnection.node_b = new List<IPAddress>(newInfo.data.ips);

        // --- Assign GameObject references ---
        newConnection.node1 = Original;
        newConnection.node2 = NewNode;

        // --- Add to tracking lists ---
        SubConnectionList.Add(newConnection);

        // --- Spawn visual connection object ---
        GameObject connectionObj = Instantiate(connectionPrefab);
        spawnedConnections.Add(connectionObj);

        // --- Initialize connection data ---
        ConnectionInfo connInfo = connectionObj.GetComponent<ConnectionInfo>();
        connInfo.Initialize(newConnection);
        connInfo.data.node1 = Original;
        connInfo.data.node2 = NewNode;

        // --- Visually connect them ---
        ConnectNodes(connectionObj.transform, Original.transform, NewNode.transform);

        // --- Change appearance so split connections stand out ---
        var renderer = connectionObj.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            // Example 1: choose color based on protocol
                    // If it’s a “split” connection, tint the color slightly blue for distinction
            renderer.material = ConnectionLight;
            renderer.material.color = Color.cyan; // distinct from the main connections

        }

        Debug.Log($"Created split sub-connection between {originalInfo.data.mac} and {newInfo.data.mac} with protocol {newConnection.protocol}");
    }




    bool NodeExists(PhysicalAddress mac, IPAddress ip)
    {
        foreach (var node in IpNodeList)
        {
            var info = node.GetComponent<NodeInfo>();
            if (mac.Equals(info.data.mac) && info.data.ips.Contains(ip))
            {
                return true; // Already spawned
            }
        }
        return false;
    }

    public int CountIP(GameObject node)
    {
        var info = node.GetComponent<NodeInfo>();
        int count = info.data.ips.Count;
        return count;
    }

    Vector3 placeNodev1(GameObject node)
    {
        Vector3 v = new Vector3();

        // Wider horizontal spread
        v.x = UnityEngine.Random.Range(-10f, 10f);
        v.y = UnityEngine.Random.Range(5f, 10f);   // vertical height
        v.z = UnityEngine.Random.Range(-10f, 10f); // deeper spread

        return v;
    }
    //Vector3 placeNodev1(GameObject node)
    //{
    //    float radius = 25f;  // increase for wider spread
    //    Vector3 randomDir = UnityEngine.Random.onUnitSphere;  // random direction
    //    float randomRadius = radius * UnityEngine.Random.Range(0.5f, 1f);

    //    return randomDir * randomRadius;
    //}
    public void ConnectNodes(Transform connection, Transform a, Transform b)
    {
        if (a == null || b == null)
        {
            Debug.LogWarning("One or both transforms are null!");
            return;
        }
        float thickness = 0.05f;
        Vector3 posA = a.position;
        Vector3 posB = b.position;

        Vector3 dir = posB - posA;
        float length = dir.magnitude;

        connection.position = (posA + posB) * 0.5f;

        connection.rotation = Quaternion.LookRotation(dir);

        connection.localScale = new Vector3(thickness, thickness, length);
        Debug.DrawLine(posA, posB, Color.green, 100f);
    }
}

