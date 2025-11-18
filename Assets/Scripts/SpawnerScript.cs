////using System;
////using System.Collections.Generic;
////using System.Linq;
////using System.Net;
////using System.Net.NetworkInformation;
////using UnityEngine;
////using UnityEngine.UI;

////public class NodeSpawnerScript : MonoBehaviour
////{

////    // *****************************************************************************************************************************
////    // Class Variables
////    // *****************************************************************************************************************************

////    // Public objects, must be connected to assets in the main window
////    public GameObject NodePrefab;
////    public GameObject connectionPrefab;

////    public Material ConnectionLight;
////    public Material ConnectionMed;
////    public Material ConnectionHeavy;

////    public GameObject dbInteractor;
////    [SerializeField] DBConnection dbConnection;
////    public FilterSystem filterSystem;
////    int timer;

////    public Toggle liveToggle;
////    public VendorLookupScript MACVendorMap;

////    // Used for spacing of nodes with same vendor
////    Dictionary<char, float> zOffset = new();

////    // Dictionary of connections and nodes
////    public Dictionary<string, GameObject> NodeObjects = new Dictionary<string, GameObject>();
////    public Dictionary<string, GameObject> ConnectionObjects = new Dictionary<string, GameObject>();

////    // Node ID lookup by IP address
////    Dictionary<string, string> NodeIDAddressRel = new Dictionary<string, string>();  ////// this can def be removed

////    List<DBConnection.Node> nodeList = new List<DBConnection.Node>();
////    List<DBConnection.Connection> ConnectionList = new List<DBConnection.Connection>(); // get rid of this
////    List<DBConnection.SubConnection> SubConnectionList = new List<DBConnection.SubConnection>();
////    List<DBConnection.SubConnection> SubConnectionIPList = new List<DBConnection.SubConnection>();
////    List<GameObject> IpNodeList = new List<GameObject>();
////    List<GameObject> spawnedNodes = new List<GameObject>();
////    public List<GameObject> spawnedConnections = new List<GameObject>();
////    // --- Add near your other fields ---
////    public float MacPlaneY = 0f;
////    public float IpPlaneY = 10f;  // height of IP layer

////    Dictionary<IPAddress, GameObject> IpNodes = new();
////    Dictionary<string, GameObject> SubnetHubs = new();  // optional: /24 hub to de-spaghetti


////    public static void PrintListContents<T>(List<T> listToPrint)
////    {
////        if (listToPrint == null)
////        {
////            Debug.Log("The list is null.");
////            return;
////        }

////        Debug.Log($"--- Printing contents of List<{typeof(T).Name}> ---");

////        if (listToPrint.Count == 0)
////        {
////            Debug.Log("The list is empty.");
////            return;
////        }

////        foreach (T item in listToPrint)
////        {
////            Debug.Log(item);
////        }
////        Debug.Log("-------------------------------------------");
////    }

////    void Awake()
////    {
////        // Get the dbInteractor Script within the spawning script
////        //dbConnection = dbInteractor.GetComponent<DBConnection>();

////        //foreach (char c in "QWERTYUIOPASDFGHJKLZXCVBNM")   ////// would be better to actually pull the vendor names rather than this approach
////        //{
////        //   zOffset[c] = 2f;
////        //}
////    }

////    void Start()
////    {
////        timer = 0;
////        if (dbConnection == null)
////        {
////            Debug.Log("dbConnection is Null");
////        }
////        else
////        {
////            nodeList = dbConnection.getNodesAfter(new DateTime(2025, 09, 06, 16, 05, 01));
////            ConnectionList = dbConnection.getConnectionsAfter(new DateTime(2025, 09, 06, 16, 05, 01));
////            // divide into sub-connections
////            foreach (var conn in ConnectionList)
////            {
////                List<DBConnection.SubConnection> connDivided = null;
////                connDivided = dbConnection.subdivideConnectionByProtocol(conn);
////                if (connDivided != null)
////                {
////                    SubConnectionList.AddRange(connDivided);
////                }
////            }
////            Debug.Log($"{ConnectionList.Count} connections");
////            Debug.Log($"{SubConnectionList.Count} sub-connections");
////        }
////        MakeNodes();
////        MakeConnections();
////        BuildIpLayerFromMacs();
////        //dbConnection.RequestNodes();
////        //dbConnection.RequestConnections();
////        //dbConnection.RequestPackets();

////        //filterSystem.CreateWhitelistButtons(dbConnection.nodes);
////    }

////    // Update is called once per frame
////    //void Update()
////    //{
////    //    // If using live data, regularly pull from the DB
////    //    if (liveToggle.isOn)
////    //    {
////    //        timer++;  // Can maybe be optimized using InvokeRepeating()

////    //        if (timer >= 100) // ~50 calls to Update per second
////    //        {
////    //            timer = 0;

////    //            dbConnection.RequestNodes();
////    //            dbConnection.RequestConnections();
////    //            dbConnection.RequestPackets();
////    //        }
////    //    }

////    //    MakeNodes();
////    //    MakeConnections();
////    //    UpdateConnections();
////    //}

////    // Getter functions for nodes, connections, and packets (can be called by other classes)
////    public Dictionary<string, GameObject> GetNodes()
////    {
////        return NodeObjects;
////    }

////    public Dictionary<string, GameObject> GetConnections()
////    {
////        return ConnectionObjects;
////    }

////    //public List<DBConnection.Packet> GetPackets()
////    //{
////    //    //        return dbConnection.packets;
////    //}

////    //void MakeNodes()
////    //{
////    //    foreach (DBConnection.Node node in nodeList)
////    //    {
////    //        //dbNode.Initialize
////    //        // Create a new GameObject based on the prefab
////    //        GameObject nodeObject = Instantiate(NodePrefab);
////    //        NodeInfo info = nodeObject.GetComponent<NodeInfo>();
////    //        info.Initialize(node);

////    //        //// Set parent and position
////    //        info.transform.position = placeNodev1(nodeObject);

////    //        spawnedNodes.Add(nodeObject);

////    //        //Add a node object to the dictionary with its id as the key
////    //        // NodeObjects[node._id.oid] = nodeObject;

////    //        //Add the node to the lookup table
////    //        //        NodeIDAddressRel.Add(node.IPaddr, node._id.oid);
////    //    }
////    //}
////    void MakeNodes()
////    {
////        foreach (DBConnection.Node node in nodeList)
////        {
////            GameObject nodeObject = Instantiate(NodePrefab);
////            NodeInfo info = nodeObject.GetComponent<NodeInfo>();
////            info.Initialize(node);

////            // Seed on a ring (or random XZ), but pin Y to MacPlaneY
////            Vector3 p = placeNodev1(nodeObject);
////            p.y = MacPlaneY;
////            nodeObject.transform.position = p;

////            spawnedNodes.Add(nodeObject);
////        }
////    }

////    void BuildIpLayerFromMacs()
////    {
////        foreach (var macGO in spawnedNodes)
////        {
////            var ni = macGO.GetComponent<NodeInfo>();
////            if (ni == null || ni.data.ips == null) continue;


////            foreach (var ip in ni.data.ips)
////            {
////                // 1) Create or reuse the IP node on the upper plane
////                GameObject ipGO = CreateOrGetIpNode(ip);

////                // 2) Create a vertical mapping edge (MAC <-> IP)
////                GameObject mapEdge = Instantiate(connectionPrefab);
////                spawnedConnections.Add(mapEdge);
////                ConnectNodes(mapEdge.transform, macGO.transform, ipGO.transform); // reuses your cylinder linker
////            }
////        }
////    }

////    GameObject CreateOrGetIpNode(IPAddress ip)
////    {
////        if (IpNodes.TryGetValue(ip, out var existing)) return existing;

////        // Optional: group IPs by /24 hub; place hub, then place IPs around it
////        string cidr = SubnetCidrv4(ip, 24); // e.g., "10.12.34.0/24"
////        GameObject hub = CreateOrGetSubnetHub(cidr);

////        // Spawn IP node near its hub, on the IP plane
////        GameObject ipGO = Instantiate(NodePrefab);
////        var info = ipGO.GetComponent<NodeInfo>();

////        // Minimal NodeInfo payload for an IP-only node, if needed
////        // (or keep as-is if your prefab expects a full DBConnection.Node)
////        // info.Initialize(...)

////        Vector3 offset = UnityEngine.Random.insideUnitSphere * 1.5f;
////        Vector3 p = hub.transform.position + new Vector3(offset.x, 0f, offset.z);
////        p.y = IpPlaneY;
////        ipGO.transform.position = p;

////        IpNodes[ip] = ipGO;
////        return ipGO;
////    }

////    GameObject CreateOrGetSubnetHub(string cidr)
////    {
////        if (SubnetHubs.TryGetValue(cidr, out var hub)) return hub;

////        hub = new GameObject($"SubnetHub_{cidr}");
////        // Place hubs on a ring on the IP plane
////        int idx = SubnetHubs.Count;
////        float R = 15f; // IP ring radius
////        float ang = (idx / Mathf.Max(1f, (float)(SubnetHubs.Count + 1))) * Mathf.PI * 2f;
////        hub.transform.position = new Vector3(Mathf.Cos(ang) * R, IpPlaneY, Mathf.Sin(ang) * R);

////        SubnetHubs[cidr] = hub;
////        return hub;
////    }

////    string SubnetCidrv4(IPAddress ip, int prefix)
////    {
////        byte[] b = ip.GetAddressBytes();
////        if (b.Length != 4) return ip.ToString(); // naive fallback for IPv6
////        int mask = unchecked((int)(uint.MaxValue << (32 - prefix)));
////        int val = (b[0] << 24) | (b[1] << 16) | (b[2] << 8) | b[3];
////        int net = val & mask;
////        byte n0 = (byte)((net >> 24) & 0xFF);
////        byte n1 = (byte)((net >> 16) & 0xFF);
////        byte n2 = (byte)((net >> 8) & 0xFF);
////        byte n3 = (byte)(net & 0xFF);
////        return $"{n0}.{n1}.{n2}.{n3}/{prefix}";
////    }



////    void MakeConnections()
////    {
////        foreach (DBConnection.SubConnection connection in SubConnectionList.ToList())
////        {        
////            GameObject firstNode = FindNodeByMac(connection.node_a_macs);
////            GameObject secondNode = FindNodeByMac(connection.node_b_macs);

////            if (firstNode == null || secondNode == null)
////            {
////                Debug.LogWarning($"Skipping connection between {connection.node_a_macs} and {connection.node_b_macs} because one or both nodes are missing.");
////                continue;  // Skip this connection and move on to the next
////            }

////            GameObject connectionObject = Instantiate(connectionPrefab);
////            ConnectionInfo connInfo = connectionObject.GetComponent<ConnectionInfo>();
////            connInfo.data.node1 = firstNode;
////            connInfo.data.node2 = secondNode;
////            connInfo.Initialize(connection);
////            spawnedConnections.Add(connectionObject);

////            ConnectNodes(connectionObject.transform, firstNode.transform, secondNode.transform);
////            //check if there are multiple ip's to a node
////            //if (CountIP(firstNode) > 1)
////            //{
////            //    SplitNode(firstNode);
////            //    foreach (var node in IpNodeList)
////            //    {
////            //        splitConnection(connection, firstNode, node);
////            //    }
////            //}
////            //if (CountIP(secondNode) > 1)
////            //{
////            //    SplitNode(secondNode);
////            //    foreach (var node in IpNodeList)
////            //    {
////            //        splitConnection(connection, secondNode, node);
////            //    }
////            //}
////        }
////    }
////    GameObject FindNodeByMac(PhysicalAddress mac)
////    {
////        foreach (var node in spawnedNodes)
////        {
////            var info = node.GetComponent<NodeInfo>();
////            if (mac.Equals(info.data.mac))
////                return node;
////        }
////        return null;
////    }

////    //public void SplitNode(GameObject node)
////    //{
////    //    var info = node.GetComponent<NodeInfo>();
////    //        foreach(var ip in info.data.ips)
////    //        {
////    //        GameObject nodeObject = Instantiate(NodePrefab);
////    //        var newNodeInfo = nodeObject.GetComponent<NodeInfo>();
////    //        DBConnection.Node newNode = new DBConnection.Node();
////    //            newNode.pkts = info.data.pkts;
////    //            newNode.bytes = info.data.bytes;
////    //            newNode.last_seen = info.data.last_seen;
////    //            newNode.first_seen = info.data.first_seen;
////    //            newNode.degree = info.data.degree;
////    //            newNode.ips.Add(ip);
////    //            newNode.src_ports = info.data.src_ports;
////    //            newNode.l7_protos = info.data.l7_protos;
////    //            newNode.device_type = info.data.device_type;
////    //            info.Initialize(newNode);
////    //            IpNodeList.Add(nodeObject);
////    //        placeNodev1(nodeObject);

////    //        }
////    //}
////    public void SplitNode(GameObject node)
////    {
////        var info = node.GetComponent<NodeInfo>();

////        foreach (var ip in info.data.ips)
////        {
////            if (NodeExists(info.data.mac, ip))
////            {
////                Debug.Log($"Node for {info.data.mac} - {ip} already exists. Skipping.");
////                continue;
////            }
////            Debug.Log(ip.ToString());
////            GameObject nodeObject = Instantiate(NodePrefab);
////            var newNodeInfo = nodeObject.GetComponent<NodeInfo>();

////            DBConnection.Node newNode = new DBConnection.Node
////            {
////                pkts = info.data.pkts,
////                bytes = info.data.bytes,
////                last_seen = info.data.last_seen,
////                first_seen = info.data.first_seen,
////                degree = info.data.degree,
////                src_ports = info.data.src_ports,
////                l7_protos = info.data.l7_protos,
////                device_type = info.data.device_type,
////                mac = info.data.mac, //  keep same MAC
////                ips = new List<IPAddress>()
////            };
////            newNode.ips.Add(ip);

////            newNodeInfo.Initialize(newNode);  //  Initialize the NEW node, not the old one
////            IpNodeList.Add(nodeObject);
////            spawnedNodes.Add(nodeObject);     //  Track for future lookup
////            nodeObject.transform.position = placeNodev1(nodeObject);
////        }
////    }

////    public void splitConnection(DBConnection.SubConnection connection, GameObject Original, GameObject NewNode)
////    {
////        NodeInfo originalInfo = Original.GetComponent<NodeInfo>();
////        NodeInfo newInfo = NewNode.GetComponent<NodeInfo>();

////        if (originalInfo == null || newInfo == null)
////        {
////            Debug.LogError("splitConnection: One or both provided GameObjects are missing NodeInfo!");
////            return;
////        }

////        DBConnection.SubConnection newConnection = new DBConnection.SubConnection();

////        // --- Copy over connection attributes ---
////        newConnection.protocol = connection.protocol;
////        newConnection.pkts = connection.pkts / 2;
////        newConnection.bytes = connection.bytes / 2;
////        newConnection.first_seen = connection.first_seen;

////        // --- Assign MACs ---
////        newConnection.node_a_macs = originalInfo.data.mac;
////        newConnection.node_b_macs = newInfo.data.mac;

////        // --- Assign IP lists ---
////        newConnection.node_a = new List<IPAddress>(originalInfo.data.ips);
////        newConnection.node_b = new List<IPAddress>(newInfo.data.ips);

////        // --- Assign GameObject references ---
////        newConnection.node1 = Original;
////        newConnection.node2 = NewNode;

////        // --- Add to tracking lists ---
////        SubConnectionList.Add(newConnection);

////        // --- Spawn visual connection object ---
////        GameObject connectionObj = Instantiate(connectionPrefab);
////        spawnedConnections.Add(connectionObj);

////        // --- Initialize connection data ---
////        ConnectionInfo connInfo = connectionObj.GetComponent<ConnectionInfo>();
////        connInfo.Initialize(newConnection);
////        connInfo.data.node1 = Original;
////        connInfo.data.node2 = NewNode;

////        // --- Visually connect them ---
////        ConnectNodes(connectionObj.transform, Original.transform, NewNode.transform);

////        // --- Change appearance so split connections stand out ---
////        var renderer = connectionObj.GetComponent<MeshRenderer>();
////        if (renderer != null)
////        {
////            // Example 1: choose color based on protocol
////                    // If it�s a �split� connection, tint the color slightly blue for distinction
////            renderer.material = ConnectionLight;
////            renderer.material.color = Color.cyan; // distinct from the main connections

////        }

////        Debug.Log($"Created split sub-connection between {originalInfo.data.mac} and {newInfo.data.mac} with protocol {newConnection.protocol}");
////    }




////    bool NodeExists(PhysicalAddress mac, IPAddress ip)
////    {
////        foreach (var node in IpNodeList)
////        {
////            var info = node.GetComponent<NodeInfo>();
////            if (mac.Equals(info.data.mac) && info.data.ips.Contains(ip))
////            {
////                return true; // Already spawned
////            }
////        }
////        return false;
////    }

////    public int CountIP(GameObject node)
////    {
////        var info = node.GetComponent<NodeInfo>();
////        int count = info.data.ips.Count;
////        return count;
////    }

////    //Vector3 placeNodev1(GameObject node)
////    //{
////    //    Vector3 v = new Vector3();

////    //    // Wider horizontal spread
////    //    v.x = UnityEngine.Random.Range(-10f, 10f);
////    //    v.y = UnityEngine.Random.Range(5f, 10f);   // vertical height
////    //    v.z = UnityEngine.Random.Range(-10f, 10f); // deeper spread

////    //    return v;
////    //}
////    Vector3 placeNodev1(GameObject node)
////    {
////        float radius = 25f;  // increase for wider spread
////        Vector3 randomDir = UnityEngine.Random.onUnitSphere;  // random direction
////        float randomRadius = radius * UnityEngine.Random.Range(0.5f, 1f);

////        return randomDir * randomRadius;
////    }
////    public void ConnectNodes(Transform connection, Transform a, Transform b)
////    {
////        if (a == null || b == null)
////        {
////            Debug.LogWarning("One or both transforms are null!");
////            return;
////        }
////        float thickness = 0.05f;
////        Vector3 posA = a.position;
////        Vector3 posB = b.position;

////        Vector3 dir = posB - posA;
////        float length = dir.magnitude;

////        connection.position = (posA + posB) * 0.5f;

////        connection.rotation = Quaternion.LookRotation(dir);

////        connection.localScale = new Vector3(thickness, thickness, length);
////        Debug.DrawLine(posA, posB, Color.green, 100f);
////    }
////}
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net;
//using System.Net.NetworkInformation;
//using UnityEngine;
//using UnityEngine.UI;

//public class NodeSpawnerScript : MonoBehaviour
//{
//    // ---------------------------------------------------------------------
//    // Prefabs / Materials
//    // ---------------------------------------------------------------------
//    [Header("Prefabs")]
//    public GameObject NodePrefab;
//    public GameObject connectionPrefab;   // straight cylinder edge (MAC<->MAC)
//    public GameObject curvedEdgePrefab;   // LineRenderer edge (MAC<->IP), optional

//    [Header("Connection Materials")]
//    public Material ConnectionLight; // MAC<->MAC
//    public Material ConnectionMed;   // MAC<->IP
//    public Material ConnectionHeavy; // (unused here, reserved)

//    // ---------------------------------------------------------------------
//    // DB / External
//    // ---------------------------------------------------------------------
//    public GameObject dbInteractor;
//    [SerializeField] DBConnection dbConnection;
//    public FilterSystem filterSystem;
//    public Toggle liveToggle;

//    // ---------------------------------------------------------------------
//    // Layout parameters
//    // ---------------------------------------------------------------------
//    [Header("Layer Heights")]
//    public float MacPlaneY = 0f;
//    public float IpPlaneY = 10f;

//    [Header("MAC Plane (group by primary IP subnet)")]
//    public float MacSubnetRingRadius = 22f; // big ring for MAC subnet hubs (y=0)
//    public float MacLocalRadiusMin = 1.4f;
//    public float MacLocalRadiusMax = 4.0f;
//    public int MacRelaxIterations = 1;   // small clean-up relax
//    public float MacRepel = 6f;
//    public float MacMaxStep = 0.25f;

//    [Header("IP Plane (group by subnet)")]
//    public float IpSubnetRingRadius = 22f; // big ring for IP subnet hubs (y=IpPlaneY) � same as MAC to align vertically
//    public float IpLocalRadiusMin = 1.5f;
//    public float IpLocalRadiusMax = 4.5f;
//    public int IpRelaxIterations = 2;
//    public float IpRepel = 4f;
//    public float IpMaxStep = 0.2f;

//    // ---------------------------------------------------------------------
//    // Internal state
//    // ---------------------------------------------------------------------
//    public Dictionary<string, GameObject> NodeObjects = new();            // MAC string -> GO
//    public Dictionary<string, GameObject> ConnectionObjects = new();      // reserved

//    List<DBConnection.Node> nodeList = new();
//    List<DBConnection.Connection> ConnectionList = new();
//    List<DBConnection.SubConnection> SubConnectionList = new();

//    List<GameObject> spawnedMacNodes = new(); // MAC layer nodes
//    public List<GameObject> spawnedConnections = new();

//    Dictionary<IPAddress, GameObject> IpNodes = new();         // IP -> GO (upper plane)
//    Dictionary<string, GameObject> MacSubnetHubs = new();      // subnet key -> hub GO (y=0)
//    Dictionary<string, GameObject> IpSubnetHubs = new();      // subnet key -> hub GO (y=IpPlaneY)

//    // Maintains a shared, deterministic order of subnets so MAC & IP hubs align
//    List<string> subnetOrder = new();

//    // ---------------------------------------------------------------------
//    // Unity lifecycle
//    // ---------------------------------------------------------------------
//    void Start()
//    {
//        // --- DB pulls ---
//        if (dbConnection == null)
//        {
//            Debug.Log("dbConnection is Null");
//        }
//        else
//        {
//            nodeList = dbConnection.getNodesAfter(new DateTime(2025, 09, 06, 16, 05, 01));
//            ConnectionList = dbConnection.getConnectionsAfter(new DateTime(2025, 09, 06, 16, 05, 01));

//            foreach (var conn in ConnectionList)
//            {
//                var parts = dbConnection.subdivideConnectionByProtocol(conn);
//                if (parts != null) SubConnectionList.AddRange(parts);
//            }
//            Debug.Log($"{ConnectionList.Count} connections");
//            Debug.Log($"{SubConnectionList.Count} sub-connections");
//        }

//        // 0) Build shared subnet order from ALL IPs we see (for alignment)
//        subnetOrder = ComputeSubnetOrder();

//        // 1) MAC plane: spawn + layout by MAC primary-IP subnet
//        MakeMacNodes();
//        LayoutMacPlaneBySubnet();
//        RelaxPlane(spawnedMacNodes.Select(go => go.transform).ToList(), MacPlaneY, MacRelaxIterations, MacRepel, MacMaxStep);

//        // 2) MAC<->MAC traffic (straight cylinder)
//        MakeMacTrafficConnections();

//        // 3) IP plane: build by subnet (aligned order), spread IPs, relax
//        BuildIpLayerFromMacsSpreadAligned();
//        RelaxPlane(IpNodes.Values.Select(go => go.transform).ToList(), IpPlaneY, IpRelaxIterations, IpRepel, IpMaxStep);

//        // 4) MAC<->IP mapping (curved, different color)
//        MakeMacIpMappingEdgesCurved();
//    }

//    // ---------------------------------------------------------------------
//    //  MAC layer
//    // ---------------------------------------------------------------------
//    void MakeMacNodes()
//    {
//        foreach (var node in nodeList)
//        {
//            GameObject go = Instantiate(NodePrefab);
//            var info = go.GetComponent<NodeInfo>();
//            info.Initialize(node);

//            var p = go.transform.position; p.y = MacPlaneY; go.transform.position = p;

//            spawnedMacNodes.Add(go);

//            string macKey = node.mac?.ToString();
//            if (!string.IsNullOrEmpty(macKey) && !NodeObjects.ContainsKey(macKey))
//                NodeObjects.Add(macKey, go);
//        }
//    }

//    // Lay out MAC nodes on y=0, grouped by the subnet of each MAC's primary IP.
//    void LayoutMacPlaneBySubnet()
//    {
//        // 1) bucket MAC nodes by primary-IP subnet
//        var macsBySubnet = new Dictionary<string, List<GameObject>>();
//        foreach (var go in spawnedMacNodes)
//        {
//            var ni = go.GetComponent<NodeInfo>();
//            var primary = PrimaryIp(ni);
//            string key = (primary != null) ? SubnetKey(primary) : "unknown";
//            if (!macsBySubnet.TryGetValue(key, out var list))
//            {
//                list = new List<GameObject>();
//                macsBySubnet[key] = list;
//            }
//            list.Add(go);
//        }

//        // 2) place MAC subnet hubs around a ring at y=0 using the shared order
//        int count = subnetOrder.Count;
//        for (int i = 0; i < count; i++)
//        {
//            string subnet = subnetOrder[i];
//            var hub = CreateOrGetMacSubnetHub(subnet);

//            float ang = (i / Mathf.Max(1f, (float)count)) * Mathf.PI * 2f;
//            hub.transform.position = new Vector3(Mathf.Cos(ang) * MacSubnetRingRadius, MacPlaneY, Mathf.Sin(ang) * MacSubnetRingRadius);
//        }

//        // 3) place MACs around their subnet hub on small rings
//        foreach (var kv in macsBySubnet)
//        {
//            var hub = CreateOrGetMacSubnetHub(kv.Key);
//            var list = kv.Value;
//            int n = list.Count;
//            float localR = Mathf.Lerp(MacLocalRadiusMin, MacLocalRadiusMax, Mathf.Clamp01((n - 1) / 14f));

//            for (int i = 0; i < n; i++)
//            {
//                float t = (i / (float)n) * Mathf.PI * 2f;
//                Vector3 local = new Vector3(Mathf.Cos(t) * localR, 0f, Mathf.Sin(t) * localR);
//                Vector3 p = hub.transform.position + local; p.y = MacPlaneY;
//                list[i].transform.position = p;
//            }
//        }
//    }

//    // ---------------------------------------------------------------------
//    //  MAC<->MAC traffic (straight)
//    // ---------------------------------------------------------------------
//    void MakeMacTrafficConnections()
//    {
//        foreach (var sc in SubConnectionList)
//        {
//            GameObject a = FindNodeByMac(sc.node_a_macs);
//            GameObject b = FindNodeByMac(sc.node_b_macs);
//            if (a == null || b == null) continue;

//            GameObject edge = Instantiate(connectionPrefab);
//            spawnedConnections.Add(edge);

//            // color/material for MAC<->MAC
//            ConnectStraight(edge.transform, a.transform, b.transform, ConnectionLight);
//        }
//    }

//    // ---------------------------------------------------------------------
//    //  IP layer (subnet hubs + IP ring per subnet), aligned with MAC subnet order
//    // ---------------------------------------------------------------------
//    public void BuildIpLayerFromMacsSpreadAligned()
//    {
//        // 1) Collect IPs per subnet from all MACs
//        var ipsBySubnet = new Dictionary<string, List<IPAddress>>();
//        foreach (var macGO in spawnedMacNodes)
//        {
//            var ni = macGO.GetComponent<NodeInfo>();
//            if (ni == null || ni.data.ips == null) continue;

//            foreach (var ip in ni.data.ips)
//            {
//                string key = SubnetKey(ip);
//                if (!ipsBySubnet.TryGetValue(key, out var list))
//                {
//                    list = new List<IPAddress>();
//                    ipsBySubnet[key] = list;
//                }
//                if (!list.Contains(ip)) list.Add(ip);
//            }
//        }

//        // 2) Place IP subnet hubs using the SAME angular order as MAC hubs
//        int count = subnetOrder.Count;
//        for (int i = 0; i < count; i++)
//        {
//            string subnet = subnetOrder[i];
//            var hub = CreateOrGetIpSubnetHub(subnet);

//            float ang = (i / Mathf.Max(1f, (float)count)) * Mathf.PI * 2f;
//            hub.transform.position = new Vector3(Mathf.Cos(ang) * IpSubnetRingRadius, IpPlaneY, Mathf.Sin(ang) * IpSubnetRingRadius);
//        }

//        // 3) Place IPs around their subnet hub on a small ring
//        foreach (var kv in ipsBySubnet)
//        {
//            var hub = CreateOrGetIpSubnetHub(kv.Key);
//            var ips = kv.Value;
//            int m = ips.Count;
//            float localR = Mathf.Lerp(IpLocalRadiusMin, IpLocalRadiusMax, Mathf.Clamp01((m - 1) / 12f));

//            for (int i = 0; i < m; i++)
//            {
//                var ip = ips[i];
//                var ipGO = CreateOrGetIpNode(ip);
//                float t = (i / (float)m) * Mathf.PI * 2f;
//                Vector3 local = new Vector3(Mathf.Cos(t) * localR, 0f, Mathf.Sin(t) * localR);
//                Vector3 p = hub.transform.position + local; p.y = IpPlaneY;
//                ipGO.transform.position = p;
//            }
//        }
//    }

//    // ---------------------------------------------------------------------
//    //  MAC<->IP mapping edges (curved)
//    // ---------------------------------------------------------------------
//    void MakeMacIpMappingEdgesCurved()
//    {
//        foreach (var macGO in spawnedMacNodes)
//        {
//            var ni = macGO.GetComponent<NodeInfo>();
//            if (ni == null || ni.data.ips == null) continue;

//            foreach (var ip in ni.data.ips)
//            {
//                var ipGO = CreateOrGetIpNode(ip);

//                if (curvedEdgePrefab != null && curvedEdgePrefab.GetComponent<LineRenderer>() != null)
//                {
//                    var edge = Instantiate(curvedEdgePrefab);
//                    spawnedConnections.Add(edge);
//                    var lr = edge.GetComponent<LineRenderer>();
//                    ApplyEdgeMaterial(lr, ConnectionMed);
//                    SetQuadraticCurve(lr, macGO.transform.position, ipGO.transform.position, bulge: 0.35f, segments: 20);
//                }
//                else
//                {
//                    var edge = Instantiate(connectionPrefab);
//                    spawnedConnections.Add(edge);
//                    ConnectStraight(edge.transform, macGO.transform, ipGO.transform, ConnectionMed);
//                }
//            }
//        }
//    }

//    // ---------------------------------------------------------------------
//    //  Helpers (nodes, hubs, keys)
//    // ---------------------------------------------------------------------
//    GameObject CreateOrGetMacSubnetHub(string cidr)
//    {
//        if (MacSubnetHubs.TryGetValue(cidr, out var hub)) return hub;
//        hub = new GameObject($"MAC_SubnetHub_{cidr}");
//        var p = hub.transform.position; p.y = MacPlaneY; hub.transform.position = p;
//        MacSubnetHubs[cidr] = hub;
//        return hub;
//    }

//    GameObject CreateOrGetIpSubnetHub(string cidr)
//    {
//        if (IpSubnetHubs.TryGetValue(cidr, out var hub)) return hub;
//        hub = new GameObject($"IP_SubnetHub_{cidr}");
//        var p = hub.transform.position; p.y = IpPlaneY; hub.transform.position = p;
//        IpSubnetHubs[cidr] = hub;
//        return hub;
//    }

//    GameObject CreateOrGetIpNode(IPAddress ip)
//    {
//        if (IpNodes.TryGetValue(ip, out var existing)) return existing;

//        GameObject go = Instantiate(NodePrefab);
//        var p = go.transform.position; p.y = IpPlaneY; go.transform.position = p;
//        EnsureSphereCollider(go, 0.25f);
//        IpNodes[ip] = go;
//        return go;
//    }

//    // Pick a "primary" IP for a MAC (first in the list; swap if you prefer most-recent)
//    IPAddress PrimaryIp(NodeInfo ni)
//    {
//        if (ni == null || ni.data.ips == null || ni.data.ips.Count == 0) return null;
//        return ni.data.ips[0];
//    }

//    // Build a stable, shared order of subnets across all IPs we see (for vertical alignment)
//    List<string> ComputeSubnetOrder()
//    {
//        var set = new HashSet<string>();

//        // From each MAC node�s primary IP (if any)
//        foreach (var node in nodeList)
//        {
//            if (node.ips != null && node.ips.Count > 0)
//                set.Add(SubnetKey(node.ips[0]));
//            else
//                set.Add("unknown");
//        }

//        // From all sub-connections: add every IP in node_a / node_b lists
//        foreach (var sc in SubConnectionList)
//        {
//            if (sc.node_a != null)
//                foreach (var ip in sc.node_a)
//                    if (ip != null) set.Add(SubnetKey(ip));

//            if (sc.node_b != null)
//                foreach (var ip in sc.node_b)
//                    if (ip != null) set.Add(SubnetKey(ip));
//        }

//        // Also include any extra IPs on nodes
//        foreach (var node in nodeList)
//        {
//            if (node.ips == null) continue;
//            foreach (var ip in node.ips)
//                if (ip != null) set.Add(SubnetKey(ip));
//        }

//        // Keep "unknown" last for tidiness
//        var order = set.ToList();
//        order.Sort(StringComparer.Ordinal);
//        if (order.Remove("unknown")) order.Add("unknown");
//        return order;
//    }


//    string SubnetKey(IPAddress ip)
//    {
//        if (ip == null) return "unknown";
//        var b = ip.GetAddressBytes();
//        if (b.Length == 4) return $"{b[0]}.{b[1]}.{b[2]}.0/24";
//        return $"v6:{b[0]:X2}{b[1]:X2}:{b[2]:X2}{b[3]:X2}:{b[4]:X2}{b[5]:X2}:{b[6]:X2}{b[7]:X2}/64";
//    }

//    // ---------------------------------------------------------------------
//    //  Geometry / drawing
//    // ---------------------------------------------------------------------
//    void ConnectStraight(Transform edge, Transform a, Transform b, Material mat = null)
//    {
//        if (a == null || b == null) return;

//        float thickness = 0.05f;
//        Vector3 pa = a.position, pb = b.position, dir = pb - pa;
//        float len = dir.magnitude;

//        edge.position = (pa + pb) * 0.5f;
//        edge.rotation = Quaternion.LookRotation(dir);
//        edge.localScale = new Vector3(thickness, thickness, len);

//        var rend = edge.GetComponentInChildren<Renderer>();
//        if (rend != null)
//        {
//            if (mat != null) rend.material = mat;
//            else rend.material.color = Color.white;
//        }
//    }

//    void ApplyEdgeMaterial(LineRenderer lr, Material mat)
//    {
//        if (lr == null) return;
//        if (mat != null) lr.material = mat;
//        else { lr.startColor = lr.endColor = Color.white; }
//        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
//        lr.receiveShadows = false;
//        lr.alignment = LineAlignment.View;
//        lr.numCornerVertices = 2;
//        lr.widthMultiplier = 0.03f;
//    }

//    public static void SetQuadraticCurve(LineRenderer lr, Vector3 a, Vector3 b, float bulge = 0.35f, int segments = 20)
//    {
//        if (lr == null) return;

//        Vector3 mid = (a + b) * 0.5f;
//        Vector3 dir = (b - a);
//        Vector3 n = Vector3.Cross(dir.normalized, Vector3.up).normalized;
//        Vector3 c = mid + n * Mathf.Min(2f, dir.magnitude * 0.25f) * bulge;

//        lr.positionCount = segments + 1;
//        for (int i = 0; i <= segments; i++)
//        {
//            float t = i / (float)segments;
//            Vector3 p = (1 - t) * (1 - t) * a + 2 * (1 - t) * t * c + t * t * b;
//            lr.SetPosition(i, p);
//        }
//    }

//    // flat-plane relax to reduce overlaps
//    void RelaxPlane(List<Transform> list, float yPlane, int iters, float repel, float maxStep)
//    {
//        if (iters <= 0 || list == null || list.Count <= 1) return;

//        for (int it = 0; it < iters; it++)
//        {
//            for (int i = 0; i < list.Count; i++)
//            {
//                Vector3 f = Vector3.zero;
//                var a = list[i];
//                for (int j = 0; j < list.Count; j++)
//                {
//                    if (i == j) continue;
//                    var b = list[j];
//                    Vector3 d = a.position - b.position;
//                    d.y = 0f;
//                    float dist2 = d.sqrMagnitude + 0.0001f;
//                    f += d.normalized * (repel / dist2);
//                }
//                Vector3 step = Vector3.ClampMagnitude(new Vector3(f.x, 0f, f.z), maxStep);
//                var p = a.position + step; p.y = yPlane; a.position = p;
//            }
//        }
//    }

//    // ---------------------------------------------------------------------
//    //  Utilities
//    // ---------------------------------------------------------------------
//    GameObject FindNodeByMac(PhysicalAddress mac)
//    {
//        if (mac == null) return null;
//        string key = mac.ToString();
//        if (NodeObjects.TryGetValue(key, out var go) && go != null) return go;

//        foreach (var go2 in spawnedMacNodes)
//        {
//            var ni = go2.GetComponent<NodeInfo>();
//            if (ni != null && ni.data.mac != null && ni.data.mac.Equals(mac))
//                return go2;
//        }
//        return null;
//    }

//    void EnsureSphereCollider(GameObject go, float radius)
//    {
//        var col = go.GetComponent<SphereCollider>();
//        if (!col) col = go.AddComponent<SphereCollider>();
//        col.isTrigger = true;
//        col.radius = radius;
//    }
//}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Timers;
using UnityEngine;
using UnityEngine.UI;

public class NodeSpawnerScript : MonoBehaviour
{
    // ---------------------------------------------------------------------
    // Prefabs / Materials
    // ---------------------------------------------------------------------
    [Header("Prefabs")]
    public GameObject NodePrefab;
    public GameObject connectionPrefab;   // straight cylinder edge (MAC<->MAC)
    public GameObject curvedEdgePrefab;   // LineRenderer edge (MAC<->IP), optional

    [Header("Connection Materials")]
    public Material ConnectionLight; // MAC<->MAC
    public Material ConnectionMed;   // MAC<->IP
    public Material ConnectionHeavy; // (unused here)

    // ---------------------------------------------------------------------
    // DB / External
    // ---------------------------------------------------------------------
    public GameObject dbInteractor;
    [SerializeField] DBConnection dbConnection;
    public FilterSystem filterSystem;
    public Toggle liveToggle;

    // ---------------------------------------------------------------------
    // Layout parameters
    // ---------------------------------------------------------------------
    [Header("Layer Heights")]
    public float MacPlaneY = 0f;
    public float IpPlaneY = 10f;

    [Header("MAC Plane (group by primary IP subnet)")]
    public float MacSubnetRingRadius = 22f; // big ring for MAC subnet hubs (y=0)
    public float MacLocalRadiusMin = 1.4f;
    public float MacLocalRadiusMax = 4.0f;
    public int MacRelaxIterations = 1;   // small clean-up relax
    public float MacRepel = 6f;
    public float MacMaxStep = 0.25f;

    [Header("IP Plane (group by subnet)")]
    public float IpSubnetRingRadius = 22f; // big ring for IP subnet hubs (y=IpPlaneY)
    public float IpLocalRadiusMin = 1.5f;
    public float IpLocalRadiusMax = 4.5f;
    public int IpRelaxIterations = 2;
    public float IpRepel = 4f;
    public float IpMaxStep = 0.2f;

    [Header("Render Timer")]
    private float renderTimer = 0.0f;
    private float waitTime = 5.0f;
    private DateTime lastRender;

    // ---------------------------------------------------------------------
    // Internal state
    // ---------------------------------------------------------------------
    public Dictionary<string, GameObject> NodeObjects = new();            // MAC string -> GO
    public Dictionary<string, GameObject> ConnectionObjects = new();      // reserved

    List<DBConnection.Node> nodeList = new();
    List<DBConnection.Connection> ConnectionList = new();
    List<DBConnection.SubConnection> SubConnectionList = new();

    List<GameObject> spawnedMacNodes = new(); // MAC layer nodes
    public List<GameObject> spawnedConnections = new();

    Dictionary<IPAddress, GameObject> IpNodes = new();         // IP -> GO (upper plane)
    Dictionary<string, GameObject> MacSubnetHubs = new();      // subnet key -> hub GO (y=0)
    Dictionary<string, GameObject> IpSubnetHubs = new();      // subnet key -> hub GO (y=IpPlaneY)

    // Shared order of subnets (so MAC & IP hubs are angle-aligned)
    List<string> subnetOrder = new();

    // The MAC with the most IPs (router) pinned at center
    GameObject routerMacGO = null;

    // One string per connection, e.g. "MACA|MACB|PROTO"
    private HashSet<string> spawnedConnectionKeys = new();

    // For polling
    public float refreshInterval = 2f;  // seconds between DB polls
    private float refreshTimer = 0f;

    private DateTime lastFetchTime;


    // ---------------------------------------------------------------------
    // Unity lifecycle
    // ---------------------------------------------------------------------
    void Start()
    {
        // --- DB pulls ---
        if (dbConnection == null)
        {
            Debug.Log("dbConnection is Null");
        }
        else
        {
            DateTime initTime = new DateTime(1970, 01, 01, 00, 00, 00);     // ensure the initial frame renders all nodes
            nodeList = dbConnection.getNodesAfter(initTime);
            ConnectionList = dbConnection.getConnectionsAfter(initTime);

            foreach (var conn in ConnectionList)
            {
                var parts = dbConnection.subdivideConnectionByProtocol(conn);
                if (parts != null) SubConnectionList.AddRange(parts);
            }
            lastRender = DateTime.Now;

            Debug.Log($"{ConnectionList.Count} connections");
            Debug.Log($"{SubConnectionList.Count} sub-connections");
        }

        // 0) Build shared subnet order from ALL IPs we see (for alignment)
        subnetOrder = ComputeSubnetOrder();

        // 1) MAC plane: spawn nodes
        MakeMacNodes();

        // 1.1) Choose router as MAC with most IPs & pin it center
        routerMacGO = FindRouterByMostIPs();

        // 1.2) Layout MACs by primary-IP subnet, keeping router pinned at center
        LayoutMacPlaneBySubnet();

        // 1.3) Relax MAC plane but keep router pinned
        var macTransforms = spawnedMacNodes.Select(go => go.transform).ToList();
        var pinned = new HashSet<Transform>();
        if (routerMacGO != null) pinned.Add(routerMacGO.transform);
        RelaxPlane(macTransforms, MacPlaneY, MacRelaxIterations, MacRepel, MacMaxStep, pinned);

        // 2) MAC<->MAC traffic (straight cylinder)
        MakeMacTrafficConnections();

        // 3) IP plane: build by subnet (aligned order), spread IPs, relax
        BuildIpLayerFromMacsSpreadAligned();
        RelaxPlane(IpNodes.Values.Select(go => go.transform).ToList(), IpPlaneY, IpRelaxIterations, IpRepel, IpMaxStep, null);

        // 4) MAC<->IP mapping (curved, different color)
        MakeMacIpMappingEdgesCurved();

        lastFetchTime = DateTime.UtcNow.AddSeconds(-5);
    }
    // ---------------------------------------------------------------------
    //  Update only for live traffic
    // ---------------------------------------------------------------------

    void Update()
    {
        if (liveToggle == null || !liveToggle.isOn)
            return;

        refreshTimer += Time.deltaTime;
        if (refreshTimer >= refreshInterval)
        {
            refreshTimer = 0f;
            PollForNewTraffic();
        }
    }


    // ---------------------------------------------------------------------
    //  MAC layer
    // ---------------------------------------------------------------------
    void MakeMacNodes()
    {
        foreach (var node in nodeList)
        {
            GameObject go = Instantiate(NodePrefab);
            var info = go.GetComponent<NodeInfo>();
            info.Initialize(node);

            var p = go.transform.position; p.y = MacPlaneY; go.transform.position = p;

            spawnedMacNodes.Add(go);

            string macKey = node.mac?.ToString();
            if (!string.IsNullOrEmpty(macKey) && !NodeObjects.ContainsKey(macKey))
                NodeObjects.Add(macKey, go);
        }
    }

    GameObject FindRouterByMostIPs()
    {
        GameObject best = null;
        int bestCount = -1;

        foreach (var go in spawnedMacNodes)
        {
            var ni = go.GetComponent<NodeInfo>();
            int c = (ni != null && ni.data.ips != null) ? ni.data.ips.Count : 0;
            if (c > bestCount)
            {
                bestCount = c;
                best = go;
            }
        }

        if (best != null)
        {
            var p = new Vector3(0f, MacPlaneY, 0f);
            best.transform.position = p;
        }
        return best;
    }

    // Lay out MAC nodes on y=0, grouped by the subnet of each MAC's primary IP.
    // Router (if any) is excluded from the ring and pinned at center.
    void LayoutMacPlaneBySubnet()
    {
        // 1) bucket MAC nodes by primary-IP subnet
        var macsBySubnet = new Dictionary<string, List<GameObject>>();
        foreach (var go in spawnedMacNodes)
        {
            if (go == routerMacGO) continue; // skip router from subnet ring placement

            var ni = go.GetComponent<NodeInfo>();
            var primary = PrimaryIp(ni);
            string key = (primary != null) ? SubnetKey(primary) : "unknown";
            if (!macsBySubnet.TryGetValue(key, out var list))
            {
                list = new List<GameObject>();
                macsBySubnet[key] = list;
            }
            list.Add(go);
        }

        // 2) place MAC subnet hubs around a ring at y=0 using the shared order
        int count = subnetOrder.Count;
        for (int i = 0; i < count; i++)
        {
            string subnet = subnetOrder[i];
            var hub = CreateOrGetMacSubnetHub(subnet);

            float ang = (i / Mathf.Max(1f, (float)count)) * Mathf.PI * 2f;
            hub.transform.position = new Vector3(Mathf.Cos(ang) * MacSubnetRingRadius, MacPlaneY, Mathf.Sin(ang) * MacSubnetRingRadius);
        }

        // 3) place MACs around their subnet hub on small rings
        foreach (var kv in macsBySubnet)
        {
            var hub = CreateOrGetMacSubnetHub(kv.Key);
            var list = kv.Value;
            int n = list.Count;
            float localR = Mathf.Lerp(MacLocalRadiusMin, MacLocalRadiusMax, Mathf.Clamp01((n - 1) / 14f));

            for (int i = 0; i < n; i++)
            {
                float t = (i / (float)n) * Mathf.PI * 2f;
                Vector3 local = new Vector3(Mathf.Cos(t) * localR, 0f, Mathf.Sin(t) * localR);
                Vector3 p = hub.transform.position + local; p.y = MacPlaneY;
                list[i].transform.position = p;
            }
        }

        // 4) ensure router is exactly centered (in case something nudged it)
        if (routerMacGO != null)
            routerMacGO.transform.position = new Vector3(0f, MacPlaneY, 0f);
    }

    // ---------------------------------------------------------------------
    //  MAC<->MAC traffic (straight)
    // ---------------------------------------------------------------------
    void MakeMacTrafficConnections()
    {
        // Group all sub-connections by unordered MAC pair
        var groups = SubConnectionList
            .GroupBy(sc => EdgeKey(sc.node_a_macs, sc.node_b_macs));

        foreach (var g in groups)
        {
            var list = g.ToList();
            int count = list.Count;

            for (int i = 0; i < count; i++)
            {
                var sc = list[i];

                GameObject a = FindNodeByMac(sc.node_a_macs);
                GameObject b = FindNodeByMac(sc.node_b_macs);
                if (a == null || b == null) continue;

                GameObject edge = Instantiate(connectionPrefab);
                spawnedConnections.Add(edge);

                // Pass index + total to spread parallel edges left/right of the line
                // (ConnectStraight already does the perpendicular offset on y=0)
                ConnectStraight(edge.transform, a.transform, b.transform, ConnectionLight, i, count);
            }
        }
    }

    string EdgeKey(PhysicalAddress a, PhysicalAddress b)
    {
        string sa = a?.ToString() ?? "";
        string sb = b?.ToString() ?? "";
        // Unordered pair key: smaller first
        return (string.CompareOrdinal(sa, sb) <= 0) ? $"{sa}|{sb}" : $"{sb}|{sa}";
    }

    // ---------------------------------------------------------------------
    //  IP layer (subnet hubs + IP ring per subnet), aligned with MAC subnet order
    // ---------------------------------------------------------------------
    public void BuildIpLayerFromMacsSpreadAligned()
    {
        // 1) Collect IPs per subnet from all MACs
        var ipsBySubnet = new Dictionary<string, List<IPAddress>>();
        foreach (var macGO in spawnedMacNodes)
        {
            var ni = macGO.GetComponent<NodeInfo>();
            if (ni == null || ni.data.ips == null) continue;

            foreach (var ip in ni.data.ips)
            {
                string key = SubnetKey(ip);
                if (!ipsBySubnet.TryGetValue(key, out var list))
                {
                    list = new List<IPAddress>();
                    ipsBySubnet[key] = list;
                }
                if (!list.Contains(ip)) list.Add(ip);
            }
        }

        // 2) Place IP subnet hubs using the SAME angular order as MAC hubs
        int count = subnetOrder.Count;
        for (int i = 0; i < count; i++)
        {
            string subnet = subnetOrder[i];
            var hub = CreateOrGetIpSubnetHub(subnet);

            float ang = (i / Mathf.Max(1f, (float)count)) * Mathf.PI * 2f;
            hub.transform.position = new Vector3(Mathf.Cos(ang) * IpSubnetRingRadius, IpPlaneY, Mathf.Sin(ang) * IpSubnetRingRadius);
        }

        // 3) Place IPs around their subnet hub on a small ring
        foreach (var kv in ipsBySubnet)
        {
            var hub = CreateOrGetIpSubnetHub(kv.Key);
            var ips = kv.Value;
            int m = ips.Count;
            float localR = Mathf.Lerp(IpLocalRadiusMin, IpLocalRadiusMax, Mathf.Clamp01((m - 1) / 12f));

            for (int i = 0; i < m; i++)
            {
                var ip = ips[i];
                var ipGO = CreateOrGetIpNode(ip);
                float t = (i / (float)m) * Mathf.PI * 2f;
                Vector3 local = new Vector3(Mathf.Cos(t) * localR, 0f, Mathf.Sin(t) * localR);
                Vector3 p = hub.transform.position + local; p.y = IpPlaneY;
                ipGO.transform.position = p;
            }
        }
    }

    // ---------------------------------------------------------------------
    //  MAC<->IP mapping edges (curved)
    // ---------------------------------------------------------------------
    void MakeMacIpMappingEdgesCurved()
    {
        foreach (var macGO in spawnedMacNodes)
        {
            var ni = macGO.GetComponent<NodeInfo>();
            if (ni == null || ni.data.ips == null) continue;

            foreach (var ip in ni.data.ips)
            {
                var ipGO = CreateOrGetIpNode(ip);

                if (curvedEdgePrefab != null && curvedEdgePrefab.GetComponent<LineRenderer>() != null)
                {
                    var edge = Instantiate(curvedEdgePrefab);
                    spawnedConnections.Add(edge);
                    var lr = edge.GetComponent<LineRenderer>();
                    ApplyEdgeMaterial(lr, ConnectionMed);
                    SetQuadraticCurve(lr, macGO.transform.position, ipGO.transform.position, bulge: 0.35f, segments: 20);
                }
                else
                {
                    var edge = Instantiate(connectionPrefab);
                    spawnedConnections.Add(edge);
                    ConnectStraight(edge.transform, macGO.transform, ipGO.transform, ConnectionMed);
                }
            }
        }
    }

    // ---------------------------------------------------------------------
    //  Helpers (nodes, hubs, keys)
    // ---------------------------------------------------------------------
    GameObject CreateOrGetMacSubnetHub(string cidr)
    {
        if (MacSubnetHubs.TryGetValue(cidr, out var hub)) return hub;
        hub = new GameObject($"MAC_SubnetHub_{cidr}");
        var p = hub.transform.position; p.y = MacPlaneY; hub.transform.position = p;
        MacSubnetHubs[cidr] = hub;
        return hub;
    }

    GameObject CreateOrGetIpSubnetHub(string cidr)
    {
        if (IpSubnetHubs.TryGetValue(cidr, out var hub)) return hub;
        hub = new GameObject($"IP_SubnetHub_{cidr}");
        var p = hub.transform.position; p.y = IpPlaneY; hub.transform.position = p;
        IpSubnetHubs[cidr] = hub;
        return hub;
    }

    GameObject CreateOrGetIpNode(IPAddress ip)
    {
        if (IpNodes.TryGetValue(ip, out var existing)) return existing;

        GameObject go = Instantiate(NodePrefab);
        var p = go.transform.position; p.y = IpPlaneY; go.transform.position = p;
        EnsureSphereCollider(go, 0.25f);
        IpNodes[ip] = go;
        return go;
    }

    // Pick a "primary" IP for a MAC (first in the list; swap if you prefer most-recent)
    IPAddress PrimaryIp(NodeInfo ni)
    {
        if (ni == null || ni.data.ips == null || ni.data.ips.Count == 0) return null;
        return ni.data.ips[0];
    }

    // Build a stable, shared order of subnets across all IPs we see (for vertical alignment)
    List<string> ComputeSubnetOrder()
    {
        var set = new HashSet<string>();

        // From MAC primary IPs
        foreach (var node in nodeList)
        {
            if (node.ips != null && node.ips.Count > 0)
                set.Add(SubnetKey(node.ips[0]));
            else
                set.Add("unknown");
        }

        // From all sub-connections: add every IP in node_a / node_b lists
        foreach (var sc in SubConnectionList)
        {
            if (sc.node_a != null)
                foreach (var ip in sc.node_a)
                    if (ip != null) set.Add(SubnetKey(ip));

            if (sc.node_b != null)
                foreach (var ip in sc.node_b)
                    if (ip != null) set.Add(SubnetKey(ip));
        }

        // From all node IPs
        foreach (var node in nodeList)
        {
            if (node.ips == null) continue;
            foreach (var ip in node.ips)
                if (ip != null) set.Add(SubnetKey(ip));
        }

        var order = set.ToList();
        order.Sort(StringComparer.Ordinal);
        if (order.Remove("unknown")) order.Add("unknown"); // keep unknown last
        return order;
    }

    string SubnetKey(IPAddress ip)
    {
        if (ip == null) return "unknown";
        var b = ip.GetAddressBytes();
        if (b.Length == 4) return $"{b[0]}.{b[1]}.{b[2]}.0/24";
        return $"v6:{b[0]:X2}{b[1]:X2}:{b[2]:X2}{b[3]:X2}:{b[4]:X2}{b[5]:X2}:{b[6]:X2}{b[7]:X2}/64";
    }

    // ---------------------------------------------------------------------
    //  Geometry / drawing
    // ---------------------------------------------------------------------
    void ConnectStraight(Transform edge, Transform a, Transform b, Material mat = null, int protoIndex = 0, int protoCount = 1)
    {
        if (a == null || b == null) return;

        float thickness = 0.05f;
        Vector3 pa = a.position, pb = b.position, dir = pb - pa;
        float len = dir.magnitude;

        // --- offset per protocol ---
        Vector3 side = Vector3.Cross(dir.normalized, Vector3.up).normalized;
        float offsetAmount = 0.1f * (protoIndex - (protoCount - 1) * 0.5f);
        Vector3 offset = side * offsetAmount;

        pa += offset;
        pb += offset;
        // ----------------------------

        edge.position = (pa + pb) * 0.5f;
        edge.rotation = Quaternion.LookRotation(pb - pa);
        edge.localScale = new Vector3(thickness, thickness, len);

        var rend = edge.GetComponentInChildren<Renderer>();
        if (rend != null)
        {
            if (mat != null) rend.material = mat;
            else rend.material.color = Color.white;
        }
    }


    void ApplyEdgeMaterial(LineRenderer lr, Material mat)
    {
        if (lr == null) return;
        if (mat != null) lr.material = mat;
        else { lr.startColor = lr.endColor = Color.white; }
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lr.receiveShadows = false;
        lr.alignment = LineAlignment.View;
        lr.numCornerVertices = 2;
        lr.widthMultiplier = 0.03f;
    }

    public static void SetQuadraticCurve(LineRenderer lr, Vector3 a, Vector3 b, float bulge = 0.35f, int segments = 20)
    {
        if (lr == null) return;

        Vector3 mid = (a + b) * 0.5f;
        Vector3 dir = (b - a);
        Vector3 n = Vector3.Cross(dir.normalized, Vector3.up).normalized;
        Vector3 c = mid + n * Mathf.Min(2f, dir.magnitude * 0.25f) * bulge;

        lr.positionCount = segments + 1;
        for (int i = 0; i <= segments; i++)
        {
            float t = i / (float)segments;
            Vector3 p = (1 - t) * (1 - t) * a + 2 * (1 - t) * t * c + t * t * b;
            lr.SetPosition(i, p);
        }
    }

    // flat-plane relax to reduce overlaps; can pin certain transforms (e.g., router)
    void RelaxPlane(List<Transform> list, float yPlane, int iters, float repel, float maxStep, HashSet<Transform> pinned)
    {
        if (iters <= 0 || list == null || list.Count <= 1) return;

        for (int it = 0; it < iters; it++)
        {
            for (int i = 0; i < list.Count; i++)
            {
                var a = list[i];
                if (pinned != null && pinned.Contains(a)) continue; // keep pinned in place

                Vector3 f = Vector3.zero;
                for (int j = 0; j < list.Count; j++)
                {
                    if (i == j) continue;
                    var b = list[j];
                    Vector3 d = a.position - b.position;
                    d.y = 0f;
                    float dist2 = d.sqrMagnitude + 0.0001f;
                    f += d.normalized * (repel / dist2);
                }
                Vector3 step = Vector3.ClampMagnitude(new Vector3(f.x, 0f, f.z), maxStep);
                var p = a.position + step; p.y = yPlane; a.position = p;
            }
        }

        // Reassert exact pin after relax (just to be safe)
        if (pinned != null)
        {
            foreach (var t in pinned)
            {
                var p = t.position; p.y = yPlane; t.position = p;
            }
        }
    }

    // ---------------------------------------------------------------------
    //  Utilities
    // ---------------------------------------------------------------------
    GameObject FindNodeByMac(PhysicalAddress mac)
    {
        if (mac == null) return null;
        string key = mac.ToString();
        if (NodeObjects.TryGetValue(key, out var go) && go != null) return go;

        foreach (var go2 in spawnedMacNodes)
        {
            var ni = go2.GetComponent<NodeInfo>();
            if (ni != null && ni.data.mac != null && ni.data.mac.Equals(mac))
                return go2;
        }
        return null;
    }

    void EnsureSphereCollider(GameObject go, float radius)
    {
        var col = go.GetComponent<SphereCollider>();
        if (!col) col = go.AddComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = radius;
    }

    // ---------------------------------------------------------------------
    //  live traffic functions
    // ---------------------------------------------------------------------
    void PollForNewTraffic()
    {
        if (dbConnection == null) return;

        // 1) ask DB for anything newer than lastFetchTime
        var newNodes = dbConnection.getNodesAfter(lastFetchTime);
        var newConns = dbConnection.getConnectionsAfter(lastFetchTime);

        // 2) bump lastFetchTime to the newest first_seen we saw
        DateTime maxTs = lastFetchTime;
        foreach (var n in newNodes)
            if (n.first_seen > maxTs) maxTs = n.first_seen;
        foreach (var c in newConns)
            if (c.first_seen > maxTs) maxTs = c.first_seen;
        if (maxTs > lastFetchTime)
            lastFetchTime = maxTs;

        // 3) spawn any *new* MAC nodes
        foreach (var n in newNodes)
        {
            nodeList.Add(n);              // keep global list in sync
            SpawnMacNodeIfNeeded(n);
        }

        // 4) subdivide new connections into sub-connections
        var newSubs = new List<DBConnection.SubConnection>();
        foreach (var conn in newConns)
        {
            var parts = dbConnection.subdivideConnectionByProtocol(conn);
            if (parts != null) newSubs.AddRange(parts);
        }
        SubConnectionList.AddRange(newSubs);

        // 5) draw edges only for the new sub-connections
        MakeMacTrafficConnectionsFor(newSubs);

        // 6) OPTIONAL: update IP layer & MAC<->IP mapping for new nodes
        //    (simple version: just hook their IPs into the existing IP hubs)
        foreach (var n in newNodes)
        {
            GameObject macGO = FindNodeByMac(n.mac);
            if (macGO == null || n.ips == null) continue;

            foreach (var ip in n.ips)
            {
                var ipGO = CreateOrGetIpNode(ip);

                if (curvedEdgePrefab != null && curvedEdgePrefab.GetComponent<LineRenderer>() != null)
                {
                    var edge = Instantiate(curvedEdgePrefab);
                    spawnedConnections.Add(edge);
                    var lr = edge.GetComponent<LineRenderer>();
                    ApplyEdgeMaterial(lr, ConnectionMed);
                    SetQuadraticCurve(lr, macGO.transform.position, ipGO.transform.position, 0.35f, 20);
                }
                else
                {
                    var edge = Instantiate(connectionPrefab);
                    spawnedConnections.Add(edge);
                    ConnectStraight(edge.transform, macGO.transform, ipGO.transform, ConnectionMed);
                }
            }
        }
    }

    void SpawnMacNodeIfNeeded(DBConnection.Node node)
    {
        string macKey = node.mac?.ToString();
        if (!string.IsNullOrEmpty(macKey) && NodeObjects.ContainsKey(macKey))
            return; // already visualized

        GameObject go = Instantiate(NodePrefab);
        var info = go.GetComponent<NodeInfo>();
        info.Initialize(node);

        // quick placement: near its subnet hub, on the MAC plane
        IPAddress primary = (node.ips != null && node.ips.Count > 0) ? node.ips[0] : null;
        string subnet = (primary != null) ? SubnetKey(primary) : "unknown";
        var hub = CreateOrGetMacSubnetHub(subnet);

        float localR = Mathf.Lerp(MacLocalRadiusMin, MacLocalRadiusMax, 0.5f);
        float ang = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
        Vector3 local = new Vector3(Mathf.Cos(ang) * localR, 0f, Mathf.Sin(ang) * localR);
        Vector3 pos = hub.transform.position + local;
        pos.y = MacPlaneY;
        go.transform.position = pos;

        spawnedMacNodes.Add(go);
        if (!string.IsNullOrEmpty(macKey))
            NodeObjects[macKey] = go;
    }


    void MakeMacTrafficConnectionsFor(IEnumerable<DBConnection.SubConnection> subs)
    {
        foreach (var sc in subs)
        {
            // Skip if we've already drawn this MAC-pair+protocol
            string key = ConnectionKey(sc);
            if (!spawnedConnectionKeys.Add(key))
                continue;

            GameObject a = FindNodeByMac(sc.node_a_macs);
            GameObject b = FindNodeByMac(sc.node_b_macs);
            if (a == null || b == null) continue;

            GameObject edge = Instantiate(connectionPrefab);
            spawnedConnections.Add(edge);

            // simple straight connection; your overload has default protoIndex/protoCount
            ConnectStraight(edge.transform, a.transform, b.transform, ConnectionLight);
        }
    }


    string ConnectionKey(DBConnection.SubConnection sc)
    {
        string pair = EdgeKey(sc.node_a_macs, sc.node_b_macs);
        // include protocol so each MAC pair+proto is unique
        return $"{pair}|{(int)sc.protocol}";
    }


}

