using System;
using System.Collections.Generic;
using System.Linq;
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
    List<DBConnection.SubConnection> SubConnectionList = new List<DBConnection.SubConnection>();// replace with subconnections
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
        foreach (DBConnection.SubConnection connection in SubConnectionList)
        {         
            //GameObject firstNode = null;
            //GameObject secondNode = null;

            //int Index = 0;
            //while ((firstNode == null || secondNode == null) && Index < spawnedNodes.Count)
            //{
            //    NodeInfo info = spawnedNodes[Index].GetComponent<NodeInfo>();
            //    Debug.Log(info.data.mac.ToString());
            //    if (connection.node_a_macs.Equals(info.data.mac))
            //    {
            //        firstNode = spawnedNodes[Index];
            //    }
            //    if (connection.node_b_macs.Equals(info.data.mac))
            //    {
            //        secondNode = spawnedNodes[Index];
            //    }
            //    Index++;
            //}
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
            // check if there are multiple ip's to a node
            //if (CountIP(firstNode) > 1)
            //{
            //    SplitNode(firstNode);
            //    foreach (var node in IpNodeList)
            //    {
            //        ConnectNodes(connectionObject.transform, firstNode.transform, node.transform);
            //    }
            //}
            //if (CountIP(secondNode) > 1)
            //{
            //    foreach (var node in IpNodeList)
            //    {
            //        ConnectNodes(connectionObject.transform, node.transform, secondNode.transform);
            //    }
            //}
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

    public void SplitNode(GameObject node)
    {
        var info = node.GetComponent<NodeInfo>();
            foreach(var ip in info.data.ips)
            {
            GameObject nodeObject = Instantiate(NodePrefab);
            var newNodeInfo = nodeObject.GetComponent<NodeInfo>();
            DBConnection.Node newNode = new DBConnection.Node();
                newNode.pkts = info.data.pkts;
                newNode.bytes = info.data.bytes;
                newNode.last_seen = info.data.last_seen;
                newNode.first_seen = info.data.first_seen;
                newNode.degree = info.data.degree;
                newNode.ips.Add(ip);
                newNode.src_ports = info.data.src_ports;
                newNode.l7_protos = info.data.l7_protos;
                newNode.device_type = info.data.device_type;
                info.Initialize(newNode);
                IpNodeList.Add(nodeObject);

            }
    }
    public int CountIP(GameObject node)
    {
        var info = node.GetComponent<NodeInfo>();
        int count = info.data.ips.Count;
        return count;
    }


    void UpdateConnections()
    {
        //    // Calculate split points for connection coloring
        //    int numPMed;
        //    int numPHeavy;

        //    List<int> numPackets = new();

        //    if (ConnectionObjects.Keys.Count > 0)
        //    {
        //        foreach (GameObject connectionObject in ConnectionObjects.Values)
        //        {
        //            numPackets.Add(connectionObject.GetComponent<ConnectionData>().NumPackets);
        //        }
        //    }
        //    else
        //    {
        //        return;
        //    }

        //    numPackets.Sort();
        //    numPMed = numPackets[numPackets.Count / 3];
        //    numPHeavy = numPackets[2 * (numPackets.Count / 3)];

        //    foreach (GameObject connectionObject in ConnectionObjects.Values)
        //    {
        //        GameObject firstNode = connectionObject.GetComponent<ConnectionData>().NodeA;
        //        GameObject secondNode = connectionObject.GetComponent<ConnectionData>().NodeB;

        //        // Check if either node is hidden
        //        connectionObject.SetActive(!(firstNode.GetComponent<NodeData>().isHidden ||
        //                                    secondNode.GetComponent<NodeData>().isHidden));

        //        // Ensure both nodes are active
        //        connectionObject.SetActive((firstNode.activeSelf && secondNode.activeSelf));

        //        // Set position to parent nodes
        //        connectionObject.transform.position = (firstNode.transform.position + secondNode.transform.position) / 2;
        //        connectionObject.transform.rotation = Quaternion.FromToRotation(Vector3.up, secondNode.transform.position - firstNode.transform.position);
        //        Vector3 delta = secondNode.transform.position - firstNode.transform.position;
        //        float connectionLength = delta.magnitude / 2;

        //        // The size of the visible object
        //        //connectionObject.transform.localScale = new Vector3(0.08f, connectionLength, 0.08f);
        //        connectionObject.transform.localScale.Set(connectionObject.transform.localScale.x, connectionLength, connectionObject.transform.localScale.z);

        //        // The size of the hitbox
        //        connectionObject.GetComponent<CapsuleCollider>().transform.localScale = new Vector3(0.08f, connectionLength, 0.08f);
        //        //connectionObject.GetComponent<CapsuleCollider>().transform.localScale.Set(connectionObject.transform.localScale.x, connectionLength, connectionObject.transform.localScale.z);

        //        // Update color of connection
        //        if (connectionObject.GetComponent<ConnectionData>().NumPackets >= numPHeavy)
        //        {
        //            connectionObject.GetComponent<MeshRenderer>().material = ConnectionHeavy;
        //        }
        //        else if (connectionObject.GetComponent<ConnectionData>().NumPackets >= numPMed)
        //        {
        //            connectionObject.GetComponent<MeshRenderer>().material = ConnectionMed;
        //        }
        //        else
        //        {
        //            connectionObject.GetComponent<MeshRenderer>().material = ConnectionLight;
        //        }
        //    }
    }

    // Generate a random node position vector
    //Vector3 randomPosVector()
    //{
    //    System.Random rnd = new System.Random();

    //    float x = (float)((rnd.NextDouble() * 10) - 5); // -1 to 1
    //    float y = (float)((rnd.NextDouble() * 5) + 0.1); // .1 - 2.1
    //    float z = (float)((rnd.NextDouble() * 10) - 5); // -1 to 1

    //    return new Vector3(x, y, z);
    //}

    Vector3 placeNodev1(GameObject node)
    {
        Vector3 v = new Vector3();

        //char vChar = Char.ToUpper(node.GetComponent<NodeData>().Vendor[0]);

        // assign x based on vendor
        v.x = UnityEngine.Random.Range(0, 5f);

        // assign heights based on number of connections
        v.y = UnityEngine.Random.Range(0, 5f);

        // just have them all start in a line
        v.z = UnityEngine.Random.Range(0, 2f);

        return v;
    }
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

