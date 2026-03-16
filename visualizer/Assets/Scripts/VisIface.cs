using ClickHouse.Client.ADO;
using ClickHouse.Client.ADO.Parameters;
using models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

public class VisIface : MonoBehaviour
{
    [Header("Database Configuration")]
    public string DB_NAME = "net";
    public string DB_HOST = "localhost";
    public string DB_PORT = "8123";
    public string DB_USER = "capstone";
    public string DB_PASS = "boogle";

    // Unity MonoBehaviour owns the raw db conn
    private DBConnection _connection;

    // expose data to visualize to other unity scripts
    public List<Node> LoadedNodes { get; private set; }
    public List<Connection> LoadedConnections { get; private set; }

    /// <summary>
    /// Connect to the database when spawnerScript is ready
    /// </summary>
    public async Task Initialize()
    {
        if (_connection == null)
        {
            _connection = new DBConnection();
        }

        if (!_connection.IsConnected)
        {
            try
            {
                await _connection.connect(DB_HOST, DB_PORT, DB_NAME, DB_USER, DB_PASS);
                DateTime initialDT = new DateTime(1970, 01, 01, 00, 00, 00);
                LoadedNodes = await GetNodesAfterAsync(initialDT);
                LoadedConnections = await GetConnectionsAfterAsync(initialDT);
                Debug.Log($"Loaded {LoadedNodes.Count} nodes and {LoadedConnections.Count} connections.");
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
            }
        }
    }

    /// <summary>
    /// Close the database connection and free any held resources
    /// </summary>
    private void OnDestroy()
    {
        if (_connection != null)
        {
            _connection?.Dispose();
        }
    }

    /// <summary>
    /// Get a list of node structs that were added to the database after a certain time.
    /// </summary>
    public async Task<List<Node>> GetNodesAfterAsync(DateTime time)
    {
        string sql = @"
            SELECT hex(mac) AS mac_hex, pkts, bytes, first_seen, last_seen, degree, 
            ips, src_ports, l7_protos, device_type
            FROM nodes WHERE nodes.first_seen > toDateTime(@time);";

        var parameters = new Dictionary<string, object> { { "time", time } };
        List<Node> nodes = new List<Node>();

        // We use 'using' here to ensure the Reader is closed immediately after use
        using var reader = await _connection.ExecuteReader(sql, parameters);

        while (reader.Read())
        {
            Node node = new Node();
            string macHex = reader.GetString(reader.GetOrdinal("mac_hex"));

            node.mac = PhysicalAddress.Parse(macHex);
            node.pkts = Convert.ToUInt64(reader.GetValue(reader.GetOrdinal("pkts")));
            node.bytes = Convert.ToUInt64(reader.GetValue(reader.GetOrdinal("bytes")));
            node.first_seen = reader.GetDateTime(reader.GetOrdinal("first_seen"));
            node.last_seen = reader.GetDateTime(reader.GetOrdinal("last_seen"));
            node.degree = Convert.ToInt32(reader.GetValue(reader.GetOrdinal("degree")));

            var ips = (IPAddress[])reader["ips"];
            node.ips = ips.ToList();

            node.src_ports = (ushort[])reader.GetValue(reader.GetOrdinal("src_ports"));
            UInt16[] l7s = (UInt16[])reader.GetValue(reader.GetOrdinal("l7_protos"));
            node.l7_protos = NetworkUtils.intArrayToAppLayerList(l7s); // Use Helper
            node.device_type = reader.GetString(reader.GetOrdinal("device_type"));

            // these fields are from the zeek db and always initialize to false until alert triggered
            node.isWarning = false;
            node.isAlert = false;

            nodes.Add(node);
        }
        return nodes;
    }

    /// <summary>
    /// Get all connections from the database after a certain time
    /// </summary>
    public async Task<List<Connection>> GetConnectionsAfterAsync(DateTime time)
    {
        string sql = @"
            SELECT
                LEAST(hex(src_mac),hex(dst_mac)) AS mac_a,
                groupUniqArray(if(src_mac <= dst_mac, src_ip, dst_ip)) AS ip_a,
                GREATEST(hex(src_mac), hex(dst_mac)) AS mac_b,
                groupUniqArray(if(src_mac <= dst_mac, dst_ip, src_ip)) AS ip_b,
                MIN(first_seen) AS first_seen, MAX(last_seen) AS last_seen,
                SUM(pkts) AS pkts, SUM(bytes) AS bytes,
                arrayDistinct(arrayFlatten(groupArrayArray(if(src_mac <= dst_mac, src_ports, dst_ports)))) AS ports_a,
                arrayDistinct(arrayFlatten(groupArrayArray(if(src_mac <= dst_mac, dst_ports, src_ports)))) AS ports_b,
                arrayDistinct(arrayFlatten(groupArrayArray(if(src_mac <= dst_mac, l7_protos, l7_protos)))) AS l7_protos_a,
                arrayDistinct(arrayFlatten(groupArrayArray(if(src_mac <= dst_mac, l7_protos, l7_protos)))) AS l7_protos_b,
                arrayDistinct(arrayFlatten(groupArrayArray(protos))) AS protos
            FROM connections WHERE connections.first_seen > toDateTime(@time)
            GROUP BY mac_a, mac_b;";

        var parameters = new Dictionary<string, object> { { "time", time } };
        List<Connection> conns = new List<Connection>();

        using var reader = await _connection.ExecuteReader(sql, parameters);

        while (reader.Read())
        {
            Connection conn = new Connection();
            conn.node_a = ((IPAddress[])reader["ip_a"]).ToList();
            conn.node_b = ((IPAddress[])reader["ip_b"]).ToList();
            conn.pkts = Convert.ToUInt64(reader.GetValue(reader.GetOrdinal("pkts")));
            conn.bytes = Convert.ToUInt64(reader.GetValue(reader.GetOrdinal("bytes")));
            conn.first_seen = reader.GetDateTime(reader.GetOrdinal("first_seen"));
            conn.last_seen = reader.GetDateTime(reader.GetOrdinal("last_seen"));

            string macHexA = reader.GetString(reader.GetOrdinal("mac_a"));
            conn.node_a_macs = PhysicalAddress.Parse(macHexA);
            string macHexB = reader.GetString(reader.GetOrdinal("mac_b"));
            conn.node_b_macs = PhysicalAddress.Parse(macHexB);

            conn.node_a_src_ports = (ushort[])reader.GetValue(reader.GetOrdinal("ports_a"));
            conn.node_a_dst_ports = (ushort[])reader.GetValue(reader.GetOrdinal("ports_b"));

            var l7sA = (ushort[])reader.GetValue(reader.GetOrdinal("l7_protos_a"));
            var l7sB = (ushort[])reader.GetValue(reader.GetOrdinal("l7_protos_b"));

            conn.node_a_l7_protos = NetworkUtils.intArrayToAppLayerList(l7sA);
            // Union handles the merging of the two lists
            conn.node_a_l7_protos.AddRange(NetworkUtils.intArrayToAppLayerList(l7sB));
            conn.node_a_l7_protos = conn.node_a_l7_protos.Distinct().ToList();

            conns.Add(conn);
        }
        return conns;
    }

    /// <summary>
    /// Get all subconnections from the database after a certain time
    /// </summary>
    public async Task<List<SubConnection>> GetSubonnectionsAfterAsync(DateTime time)
    {
        string sql = @"
            SELECT
                LEAST(hex(src_mac),hex(dst_mac)) AS mac_a,
                groupUniqArray(if(src_mac <= dst_mac, src_ip, dst_ip)) AS ip_a,
                GREATEST(hex(src_mac), hex(dst_mac)) AS mac_b,
                groupUniqArray(if(src_mac <= dst_mac, dst_ip, src_ip)) AS ip_b,
                MIN(first_seen) AS first_seen, 
                MAX(last_seen) AS last_seen,
                SUM(pkts) AS pkts,
                SUM(bytes) AS bytes,
                arrayDistinct(arrayFlatten(groupArrayArray(if(src_mac <= dst_mac, src_ports, dst_ports)))) AS ports_a,
                arrayDistinct(arrayFlatten(groupArrayArray(if(src_mac <= dst_mac, dst_ports, src_ports)))) AS ports_b,
                groupUniqArray(l7_proto) AS l7_protos,
                groupUniqArray(l4_proto) AS protos
            FROM subconnections WHERE subconnections.first_seen > toDateTime(@time)
            GROUP BY mac_a, mac_b;";

        var parameters = new Dictionary<string, object> { { "time", time } };
        List<SubConnection> subConnections = new List<SubConnection>();

        using var reader = await _connection.ExecuteReader(sql, parameters);

        while (reader.Read())
        {
            SubConnection sc = new SubConnection();
            sc.node_a = ((IPAddress[])reader["ip_a"]).ToList();
            sc.node_b = ((IPAddress[])reader["ip_b"]).ToList();
            sc.pkts = Convert.ToUInt64(reader.GetValue(reader.GetOrdinal("pkts")));
            sc.bytes = Convert.ToUInt64(reader.GetValue(reader.GetOrdinal("bytes")));
            sc.first_seen = reader.GetDateTime(reader.GetOrdinal("first_seen"));
            string macHexA = reader.GetString(reader.GetOrdinal("mac_a"));
            sc.node_a_macs = PhysicalAddress.Parse(macHexA);
            string macHexB = reader.GetString(reader.GetOrdinal("mac_b"));
            sc.node_b_macs = PhysicalAddress.Parse(macHexB);
            sc.protocol = (l7_proto)reader.GetValue(reader.GetOrdinal("l7_protos"));

            subConnections.Add(sc);
        }
        return subConnections;
    }

    /// <summary>
    /// Query the weird table and flag nodes that have warnings (will be colored yellow)
    /// </summary>
    public async Task<List<Node>> FlagWeirdNodes(DateTime time, List<Node> loadedNodes)
    {
        string sql = "SELECT orig_h FROM net.weird WHERE weird.ts > toDateTime(@time)";
        var parameters = new Dictionary<string, object> { { "time", time } };

        using var reader = await _connection.ExecuteReader(sql, parameters);
        List<IPAddress> ips = new List<IPAddress>();
        while (reader.Read())
        {
            var rawData = reader["orig_h"];
            if (rawData is IPAddress[] ipArray)
            {
                ips.AddRange(ipArray);
            }
            else if (rawData is IPAddress singleIp)
            {
                ips.Add(singleIp);
            }
        }

        // update nodes whose IP addresses match warnings above.
        foreach (IPAddress ip in ips)
        {
            var index = loadedNodes.FindIndex(Node => Node.ips.Contains(ip));
            if (index  != -1)
            {
                var temp = loadedNodes[index];
                temp.isWarning = true;
                loadedNodes[index] = temp;
            }
        }

        return loadedNodes;
    }

    /// <summary>
    /// Query the notice table and flag nodes that have alerts (will be colored red)
    /// </summary>
    public async Task<List<Node>> FlagAlertedNodes(DateTime time, List<Node> loadedNodes)
    {
        string sql = "SELECT src, note FROM net.notice WHERE notice.ts > toDateTime(@time);";
        var parameters = new Dictionary<string, object> { { "time", time } };

        using var reader = await _connection.ExecuteReader(sql, parameters);
        List<IPAddress> ips = new List<IPAddress>();
        List<String> alertList = new List<String>();
        while (reader.Read())
        {
            var rawData = reader["src"];
            if (rawData is IPAddress[] ipArray)
            {
                ips.AddRange(ipArray);
            }
            else if (rawData is IPAddress singleIp)
            {
                ips.Add(singleIp);
            }

            rawData = reader["note"];
            if (rawData is not null)
            {
                alertList.Add((String)rawData);
            }
        }

        // update nodes whose IP addresses match warnings above.
        foreach (IPAddress ip in ips)
        {
            var index = loadedNodes.FindIndex(Node => Node.ips.Contains(ip));
            if (index != -1)
            {
                var temp = loadedNodes[index];
                temp.isAlert = true;
                alertList = alertList.Distinct().ToList();
                temp.alerts = String.Join(", ", alertList);
                loadedNodes[index] = temp;
            }
        }

        return loadedNodes;
    }
}