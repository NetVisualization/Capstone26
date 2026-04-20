using models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Unity.VisualScripting;
using UnityEngine;

public static class NetworkUtils
{
    public static List<l7_proto> intArrayToAppLayerList(UInt16[] ints)
    {
        List<l7_proto> myAppLayerValues = new List<l7_proto>();
        foreach (var i in ints)
        {
            l7_proto currentProto = (l7_proto)i;
            myAppLayerValues.Add(currentProto);
        }
        return myAppLayerValues;
    }

    public static List<SubConnection> subdivideConnectionByProtocol(Connection conn)
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
                subConn.pkts = conn.pkts;
                subConn.bytes = conn.bytes;
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

    public static string GetStandardizedIPString(List<IPAddress> ipAddresses)
    {
        if (ipAddresses == null)
        {
            return string.Empty;
        }

        return string.Join(", ", ipAddresses.Select(ip =>
        {
            if (ip == null)
            {
                return string.Empty;
            }

            if (ip.IsIPv4MappedToIPv6)
            {
                return ip.MapToIPv4().ToString();
            }
            else { 
                return ip.ToString(); 
            }
        }));
    }

    public static string GetStandardizedIPString(string ipAddresses)
    {
        if (ipAddresses == null)
        {
            return string.Empty;
        }

        IPAddress.TryParse(ipAddresses, out IPAddress ip);
        if (ip == null)
        {
            return string.Empty;
        }

        if (ip.IsIPv4MappedToIPv6)
        {
            return ip.MapToIPv4().ToString();
        }
        else
        {
            return ip.ToString();
        }
    }
}