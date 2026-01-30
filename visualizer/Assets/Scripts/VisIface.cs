using System;
using System.Net;
using UnityEngine;

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
        dbRecord.pkts = Convert.ToUInt64(reader.GetValue(reader.GetOrdinal("pkts")));
        dbRecord.bytes = Convert.ToUInt64(reader.GetValue(reader.GetOrdinal("bytes")));
        dbRecord.first_seen = (DateTime)reader.GetDateTime(reader.GetOrdinal("first_seen"));
        dbRecord.last_seen = (DateTime)reader.GetDateTime(reader.GetOrdinal("last_seen"));
        dbRecord.degree = Convert.ToInt32(reader.GetValue(reader.GetOrdinal("degree")));
        var ips = (System.Net.IPAddress[])reader["ips"];
        dbRecord.ips = ips.ToList();
        dbRecord.src_ports = (ushort[])reader.GetValue(reader.GetOrdinal("src_ports"));
        UInt16[] l7s = (UInt16[])reader.GetValue(reader.GetOrdinal("l7_protos"));
        dbRecord.l7_protos = intArrayToAppLayerList(l7s);
        dbRecord.device_type = reader.GetString(reader.GetOrdinal("device_type"));
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
        dbRecord.pkts = Convert.ToUInt64(reader.GetValue(reader.GetOrdinal("pkts")));
        dbRecord.bytes = Convert.ToUInt64(reader.GetValue(reader.GetOrdinal("bytes")));
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
