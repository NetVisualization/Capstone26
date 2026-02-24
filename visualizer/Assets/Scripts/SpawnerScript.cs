
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Timers;
using UnityEngine;
using UnityEngine.UI;
using models;
using System.Threading.Tasks;

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

    [Header("Node Status Materials")]
    [SerializeField] private Material normalNodeMaterial;
    [SerializeField] private Material warningNodeMaterial; // yellow
    [SerializeField] private Material alertNodeMaterial;   // red

    // ---------------------------------------------------------------------
    // DB / External
    // ---------------------------------------------------------------------
    public GameObject dbInteractor;
    [SerializeField] VisIface dataManager;
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

    public IReadOnlyList<GameObject> Connections => spawnedConnections; // for use in other files
    public IReadOnlyDictionary<string, GameObject> MacNodes => NodeObjects; // for filtering nodes by MAC

    // ---- Public node access for filtering ----
public IReadOnlyList<GameObject> MacNodeList => spawnedMacNodes;
public IEnumerable<GameObject> IpNodeList => IpNodes.Values;
private readonly Dictionary<string, List<GameObject>> edgesByIpString = new();
public IReadOnlyDictionary<string, List<GameObject>> EdgesByIpString => edgesByIpString;

// easy "everything" view (MAC + IP nodes)
public IEnumerable<GameObject> AllNodes => spawnedMacNodes.Concat(IpNodes.Values);

// ---- Adjacency: which edges touch each node key ----
private readonly Dictionary<string, List<GameObject>> edgesByMac = new();
private readonly Dictionary<IPAddress, List<GameObject>> edgesByIp = new();

public IReadOnlyDictionary<string, List<GameObject>> EdgesByMac => edgesByMac;
public IReadOnlyDictionary<IPAddress, List<GameObject>> EdgesByIp => edgesByIp;



    List<Node> nodeList = new();
    List<Connection> ConnectionList = new();
    List<SubConnection> SubConnectionList = new();

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
    private bool isPolling = false; // Prevent overlapping async calls
    private DateTime lastFetchTime;


    // ---------------------------------------------------------------------
    // Unity lifecycle
    // ---------------------------------------------------------------------
    async void Start()
    {
        // --- DB pulls ---
        if (dataManager == null)
        {
            Debug.Log("visIface dataManager is not initialized");
            return;
        }

        try
        {
            await dataManager.Initialize();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to connect to DB: {ex.Message}");
            return;
        }

        // ensure the initial frame renders all nodes
        // use WhenAll method to fetch in parallel for performance
        DateTime initTime = new DateTime(1970, 01, 01, 00, 00, 00);
        var nodesTask =  dataManager.GetNodesAfterAsync(initTime);
        var connsTask = dataManager.GetConnectionsAfterAsync(initTime);
        await Task.WhenAll(nodesTask, connsTask);
        nodeList = nodesTask.Result;
        ConnectionList = connsTask.Result;

        foreach (var conn in ConnectionList)
        {
            var parts = NetworkUtils.subdivideConnectionByProtocol(conn);
            if (parts != null) SubConnectionList.AddRange(parts);
        }
        lastRender = DateTime.Now;

        Debug.Log($"{ConnectionList.Count} connections");
        Debug.Log($"{SubConnectionList.Count} sub-connections");

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

            ApplyNodeStatusMaterial(go, node);

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
                var connInfo = edge.GetComponent<ConnectionInfo>();
                if (connInfo != null)
                {
                    connInfo.Initialize(sc);
                }

                var tag = edge.AddComponent<EdgeTag>();
                tag.isMacMac = true;
                tag.mac_a = sc.node_a_macs?.ToString();
                tag.mac_b = sc.node_b_macs?.ToString();
                tag.protocol = sc.protocol;

                spawnedConnections.Add(edge);
                RegisterEdge(edge, tag);


                // keep your parallel offset behavior
                ConnectStraight(edge.transform, a.transform, b.transform, null, i, count);

                // color by protocol
                var rend = edge.GetComponentInChildren<Renderer>();
                SetRendererColor(rend, GetColorForProtocol(sc.protocol));
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

                    var tag = edge.AddComponent<EdgeTag>();
                    tag.isMacIp = true;
                    tag.mac_a = ni.data.mac?.ToString();
                    tag.ip = ip;
                    tag.protocol = l7_proto.UNKNOWN;
                    
                    spawnedConnections.Add(edge);
                    RegisterEdge(edge, tag);

                    var lr = edge.GetComponent<LineRenderer>();
                    ApplyEdgeMaterial(lr, ConnectionMed);
                    SetQuadraticCurve(lr, macGO.transform.position, ipGO.transform.position, bulge: 0.35f, segments: 20);
                }
                else
                {
                    var edge = Instantiate(connectionPrefab);
                    var tag = edge.AddComponent<EdgeTag>();
                    tag.isMacIp = true;
                    tag.mac_a = ni.data.mac?.ToString();
                    tag.ip = ip;
                    tag.protocol = l7_proto.UNKNOWN;

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

        var tag = go.GetComponent<IpTag>();
        if (!tag) tag = go.AddComponent<IpTag>();
        tag.ipString = ip.ToString();

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

    // ---------- Protocol to Color mapping ----------
    Color GetColorForProtocol(l7_proto proto)
    {
        // Distinct, readable palette. Tweak as you like.
        switch (proto)
        {
            case l7_proto.SSH: return new Color(0.84f, 0.16f, 0.23f); // rgb(215, 41, 59)
            case l7_proto.SMTP: return new Color(0.98f, 0.62f, 0.01f); // rgb(250, 158, 3)
            case l7_proto.DNS: return new Color(0.18f, 0.60f, 0.96f); // rgb(46, 153, 245)
            case l7_proto.DHCP: return new Color(0.41f, 0.23f, 0.88f); // rgb(105, 59, 224)
            case l7_proto.HTTP: return new Color(0.95f, 0.38f, 0.09f); // rgb(242, 97, 23)
            case l7_proto.POP3: return new Color(0.11f, 0.74f, 0.73f); // rgb(29, 189, 186)
            case l7_proto.NTP: return new Color(0.16f, 0.80f, 0.26f); // rgb(41, 204, 67)
            case l7_proto.IMAP: return new Color(0.60f, 0.47f, 0.16f); // rgb(153, 120, 42)
            case l7_proto.TLS: return new Color(0.13f, 0.59f, 0.95f); // rgb(33, 143, 239) (HTTPS-ish)
            case l7_proto.SMB: return new Color(0.62f, 0.35f, 0.71f); // rgb(158, 89, 181)
            case l7_proto.QUIC: return new Color(0.96f, 0.23f, 0.33f); // rgb(246, 59, 85)
            case l7_proto.SSDP: return new Color(0.50f, 0.83f, 0.13f); // rgb(128, 211, 33)
            case l7_proto.RDP: return new Color(0.95f, 0.18f, 0.54f); // rgb(242, 46, 138)
            case l7_proto.MDNS: return new Color(0.00f, 0.67f, 0.52f); // rgb(0, 170, 133)
            case l7_proto.UNKNOWN:
            default: return Color.gray;
        }
    }

    // ---------- Safely apply color to MeshRenderer / Standard / URP shaders ----------
    static MaterialPropertyBlock _mpb;
    void SetRendererColor(Renderer r, Color c)
    {
        if (r == null) return;
        if (_mpb == null) _mpb = new MaterialPropertyBlock();
        r.GetPropertyBlock(_mpb);

        // Common color property names
        _mpb.SetColor("_Color", c);      // Standard / Unlit
        _mpb.SetColor("_BaseColor", c);  // URP Lit

        r.SetPropertyBlock(_mpb);
    }

    // ---------------- Node status coloring ----------------
    private void ApplyNodeStatusMaterial(GameObject nodeGO, Node nodeData)
{
    if (nodeGO == null) return;

    var renderers = nodeGO.GetComponentsInChildren<Renderer>(true);
    if (renderers == null || renderers.Length == 0) return;

    // Determine if we need an override
    bool isAlert = nodeData.isAlert;
    bool isWarning = nodeData.isWarning;

    foreach (var r in renderers)
    {
        if (r == null) continue;

        // If NOT alert or warning → reset to original material color
        if (!isAlert && !isWarning)
        {
            r.SetPropertyBlock(null);   // 🔥 This restores the material's default color
            continue;
        }

        if (_mpb == null) _mpb = new MaterialPropertyBlock();
        r.GetPropertyBlock(_mpb);

        Color c = isAlert ? Color.red : Color.yellow;

        _mpb.SetColor("_Color", c);      // Standard
        _mpb.SetColor("_BaseColor", c);  // URP

        r.SetPropertyBlock(_mpb);
    }
}

// Update an existing MAC node GO if it already exists (used for live polling)
private void UpdateExistingMacNodeVisuals(Node nodeData)
{
    string macKey = nodeData.mac?.ToString();
    if (string.IsNullOrEmpty(macKey)) return;

    if (NodeObjects.TryGetValue(macKey, out var go) && go != null)
    {
        // keep NodeInfo in sync too (optional but recommended)
        var info = go.GetComponent<NodeInfo>();
        if (info != null) info.Initialize(nodeData);

        ApplyNodeStatusMaterial(go, nodeData);
    }
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

    private void RegisterEdge(GameObject edge, EdgeTag tag)
{
    if (edge == null || tag == null) return;

    // MAC-MAC edge
    if (tag.isMacMac)
    {
        if (!string.IsNullOrEmpty(tag.mac_a))
        {
            if (!edgesByMac.TryGetValue(tag.mac_a, out var listA))
                edgesByMac[tag.mac_a] = listA = new List<GameObject>();
            listA.Add(edge);
        }

        if (!string.IsNullOrEmpty(tag.mac_b))
        {
            if (!edgesByMac.TryGetValue(tag.mac_b, out var listB))
                edgesByMac[tag.mac_b] = listB = new List<GameObject>();
            listB.Add(edge);
        }
    }

    // MAC-IP edge
    if (tag.isMacIp)
    {
        if (!string.IsNullOrEmpty(tag.mac_a))
        {
            if (!edgesByMac.TryGetValue(tag.mac_a, out var listA))
                edgesByMac[tag.mac_a] = listA = new List<GameObject>();
            listA.Add(edge);
        }

        if (tag.ip != null)
        {
            string ipKey = tag.ip.ToString();
            if (!edgesByIp.TryGetValue(tag.ip, out var listI))
                edgesByIp[tag.ip] = listI = new List<GameObject>();
            listI.Add(edge);
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

    //checks for any updates in the database asyncronously to prevent freezing
    async void PollForNewTraffic()
    {
        // If manager is missing or we are currently waiting for a previous poll, skip
        if (dataManager == null || isPolling) return;

        isPolling = true;

        try
        {
            // 1) Ask DB for anything newer than lastFetchTime (Async)
            var newNodes = await dataManager.GetNodesAfterAsync(lastFetchTime);
            var newConns = await dataManager.GetConnectionsAfterAsync(lastFetchTime);

            // check for any new zeek alerts
            newNodes = await dataManager.FlagWeirdNodes(lastFetchTime, newNodes);

            // Update visuals for nodes that already exist (warning/alert flags may change)
            foreach (var n in newNodes)
            {       
                UpdateExistingMacNodeVisuals(n);
            }
            
            var warningIps = newNodes.Where(node => node.isWarning).SelectMany(node => node.ips);
            foreach (var ip in warningIps)
            {
                Debug.Log($"weird node to color yellow: {ip.ToString()}");
            }


            // 2) Update timestamps
            DateTime maxTs = lastFetchTime;
            foreach (var n in newNodes)
                if (n.first_seen > maxTs) maxTs = n.first_seen;
            foreach (var c in newConns)
                if (c.first_seen > maxTs) maxTs = c.first_seen;

            if (maxTs > lastFetchTime)
                lastFetchTime = maxTs;

            // 3) Spawn new Nodes
            foreach (var n in newNodes)
            {
                nodeList.Add(n);
                SpawnMacNodeIfNeeded(n);
            }

            // 4) Process new Connections
            var newSubs = new List<SubConnection>();
            foreach (var conn in newConns)
            {
                // UPDATED: Use static helper
                var parts = NetworkUtils.subdivideConnectionByProtocol(conn);
                if (parts != null) newSubs.AddRange(parts);
            }
            SubConnectionList.AddRange(newSubs);

            // 5) Draw visual edges
            MakeMacTrafficConnectionsFor(newSubs);

            // 6) Update IP mappings
            foreach (var n in newNodes)
            {
                GameObject macGO = FindNodeByMac(n.mac);
                if (macGO == null || n.ips == null) continue;

                foreach (var ip in n.ips)
                {
                    var ipGO = CreateOrGetIpNode(ip);
                    CreateMacIpEdge(macGO, ipGO, n.mac, ip);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error polling traffic: {ex.Message}");
        }
        finally
        {
            isPolling = false;
        }
    }

    void CreateMacIpEdge(GameObject macGO, GameObject ipGO, PhysicalAddress mac, IPAddress ip)
    {
        if (curvedEdgePrefab != null && curvedEdgePrefab.GetComponent<LineRenderer>() != null)
        {
            var edge = Instantiate(curvedEdgePrefab);
            spawnedConnections.Add(edge);

            var tag = edge.AddComponent<EdgeTag>();
            tag.isMacIp = true;
            tag.mac_a = mac?.ToString();
            tag.ip = ip;
            tag.protocol = l7_proto.UNKNOWN;

            var lr = edge.GetComponent<LineRenderer>();
            ApplyEdgeMaterial(lr, ConnectionMed);
            SetQuadraticCurve(lr, macGO.transform.position, ipGO.transform.position, 0.35f, 20);
        }
        else
        {
            var edge = Instantiate(connectionPrefab);
            var tag = edge.AddComponent<EdgeTag>();
            tag.isMacIp = true;
            tag.mac_a = mac?.ToString();
            tag.ip = ip;
            tag.protocol = l7_proto.UNKNOWN;

            spawnedConnections.Add(edge);
            ConnectStraight(edge.transform, macGO.transform, ipGO.transform, ConnectionMed);
        }
    }

    //spawn any new nodes that have been found
    void SpawnMacNodeIfNeeded(Node node)
    {
        string macKey = node.mac?.ToString();
        if (!string.IsNullOrEmpty(macKey) && NodeObjects.ContainsKey(macKey))
            return; // already visualized

        GameObject go = Instantiate(NodePrefab);
        var info = go.GetComponent<NodeInfo>();
        info.Initialize(node);
        ApplyNodeStatusMaterial(go, node);

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

    // make any connections for the new nodes created
    void MakeMacTrafficConnectionsFor(IEnumerable<SubConnection> subs)
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
            var connInfo = edge.GetComponent<ConnectionInfo>();
            if (connInfo != null)
            {
                connInfo.Initialize(sc);
            }

            var tag = edge.AddComponent<EdgeTag>();
            tag.isMacMac = true;
            tag.mac_a = sc.node_a_macs?.ToString();
            tag.mac_b = sc.node_b_macs?.ToString();
            tag.protocol = sc.protocol;

            spawnedConnections.Add(edge);

            ConnectStraight(edge.transform, a.transform, b.transform, null);

            var rend = edge.GetComponentInChildren<Renderer>();
            SetRendererColor(rend, GetColorForProtocol(sc.protocol));
        }
    }


    // makes evey mac unique
    string ConnectionKey(SubConnection sc)
    {
        string pair = EdgeKey(sc.node_a_macs, sc.node_b_macs);
        // include protocol so each MAC pair+proto is unique
        return $"{pair}|{(int)sc.protocol}";
    }


}

