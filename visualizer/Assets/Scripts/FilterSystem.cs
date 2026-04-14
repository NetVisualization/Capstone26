using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using models; // l7_proto

public class FilterSystem : MonoBehaviour
{
    [SerializeField] private NodeSpawnerScript spawner;

    private readonly System.Collections.Generic.HashSet<l7_proto> hiddenProtocols = new();

    private bool macIpHidden = false;
    private bool ipv6NoiseHidden = true;   // hidden by default
    private bool noisyMacHidden = true;    // hidden by default

    private void Awake()
    {
        if (!spawner)
            spawner = FindFirstObjectByType<NodeSpawnerScript>();
    }

    private void Start()
    {
        if (spawner == null) return;
        ApplyDefaultFilters();
    }

    // filterKey: "SSH", "DNS", ... or "MACIP" or "IPV6NOISE" or "NOISYMAC"
    // filterOn: true means "hide that thing"
    public void SetFilterState(string filterKey, bool filterOn)
    {
        if (spawner == null) return;

        if (filterKey.Equals("MACIP", StringComparison.OrdinalIgnoreCase))
        {
            macIpHidden = filterOn;
            ReapplyAllFilters();
            return;
        }

        if (filterKey.Equals("IPV6NOISE", StringComparison.OrdinalIgnoreCase))
        {
            ipv6NoiseHidden = filterOn;
            ReapplyAllFilters();
            return;
        }

        if (filterKey.Equals("NOISYMAC", StringComparison.OrdinalIgnoreCase))
        {
            noisyMacHidden = filterOn;
            ReapplyAllFilters();
            return;
        }

        if (!Enum.TryParse(filterKey, ignoreCase: true, out l7_proto proto))
        {
            Debug.LogWarning($"Filter key '{filterKey}' doesn't match l7_proto enum.");
            return;
        }

        if (filterOn) hiddenProtocols.Add(proto);
        else hiddenProtocols.Remove(proto);

        ReapplyAllFilters();
    }

    private void ReapplyAllFilters()
    {
        if (spawner == null) return;

        // ----- EDGES -----
        foreach (var edge in spawner.Connections)
        {
            if (!edge) continue;
            edge.SetActive(ShouldEdgeBeVisible(edge));
        }

        // ----- MAC NODES -----
        foreach (var kv in spawner.MacNodes)
        {
            string mac = kv.Key;
            var nodeGO = kv.Value;
            if (!nodeGO) continue;

            if (noisyMacHidden && IsNoisyMac(mac))
            {
                nodeGO.SetActive(false);
                continue;
            }

            bool anyActive = false;
            if (spawner.EdgesByMac.TryGetValue(mac, out var edges) && edges != null)
            {
                for (int i = 0; i < edges.Count; i++)
                {
                    var e = edges[i];
                    if (e && e.activeSelf)
                    {
                        anyActive = true;
                        break;
                    }
                }
            }

            nodeGO.SetActive(anyActive);
        }

        // ----- IP NODES -----
        foreach (var ipGO in spawner.IpNodeList)
        {
            if (!ipGO) continue;

            var ipTag = ipGO.GetComponent<IpTag>();
            if (ipTag == null || string.IsNullOrWhiteSpace(ipTag.ipString))
            {
                ipGO.SetActive(false);
                continue;
            }

            if (IPAddress.TryParse(ipTag.ipString, out var ip))
            {
                if (ipv6NoiseHidden && IsIPv6Noise(ip))
                {
                    ipGO.SetActive(false);
                    continue;
                }
            }

            if (!spawner.EdgesByIpString.TryGetValue(ipTag.ipString, out var edges) || edges == null)
            {
                ipGO.SetActive(false);
                continue;
            }

            bool anyActive = false;
            for (int i = 0; i < edges.Count; i++)
            {
                var e = edges[i];
                if (e && e.activeSelf)
                {
                    anyActive = true;
                    break;
                }
            }

            ipGO.SetActive(anyActive);
        }
    }

    private bool ShouldEdgeBeVisible(GameObject edge)
    {
        if (!edge) return false;

        var tag = edge.GetComponent<EdgeTag>();
        if (tag == null) return true;

        // Hide MAC<->IP edges if MACIP filter says so
        if (tag.isMacIp && macIpHidden)
            return false;

        // Hide protocol-specific MAC<->MAC edges
        if (tag.isMacMac && hiddenProtocols.Contains(tag.protocol))
            return false;

        // Hide noisy MAC edges
        if (noisyMacHidden)
        {
            if (!string.IsNullOrWhiteSpace(tag.mac_a) && IsNoisyMac(tag.mac_a))
                return false;

            if (!string.IsNullOrWhiteSpace(tag.mac_b) && IsNoisyMac(tag.mac_b))
                return false;
        }

        // Hide IPv6 multicast/link-local IP edges
        if (ipv6NoiseHidden && tag.ip != null && IsIPv6Noise(tag.ip))
            return false;

        return true;
    }

    private bool IsIPv6Noise(IPAddress ip)
    {
        if (ip == null || ip.AddressFamily != AddressFamily.InterNetworkV6)
            return false;

        byte[] bytes = ip.GetAddressBytes();

        // Unspecified ::
        bool allZero = true;
        for (int i = 0; i < bytes.Length; i++)
        {
            if (bytes[i] != 0)
            {
                allZero = false;
                break;
            }
        }
        if (allZero)
            return true;

        // Multicast: ff00::/8
        if (bytes[0] == 0xFF)
            return true;

        // Link-local: fe80::/10
        if (bytes[0] == 0xFE && (bytes[1] & 0xC0) == 0x80)
            return true;

        return false;
    }

    private bool IsNoisyMac(string macString)
    {
        if (string.IsNullOrWhiteSpace(macString))
            return false;

        string m = macString.Replace(":", "").Replace("-", "").Trim().ToUpperInvariant();

        // Broadcast
        if (m == "FFFFFFFFFFFF")
            return true;

        // IPv6 multicast MAC 33:33:xx:xx:xx:xx
        if (m.StartsWith("3333"))
            return true;

        // IPv4 multicast MAC 01:00:5E:xx:xx:xx
        if (m.StartsWith("01005E"))
            return true;

        return false;
    }

    public void HideAllConnections()
    {
        foreach (var c in spawner.Connections)
            if (c) c.SetActive(false);

        hiddenProtocols.Clear();
        macIpHidden = true;
        ipv6NoiseHidden = true;
        noisyMacHidden = true;

        ReapplyAllFilters();
    }

    public void ShowAllConnections()
    {
        hiddenProtocols.Clear();
        macIpHidden = false;
        ipv6NoiseHidden = false;
        noisyMacHidden = false;

        ReapplyAllFilters();
    }

    public void RefreshNodeVisibility()
    {
        ReapplyAllFilters();
    }

    public void ApplyDefaultFilters()
    {
        ReapplyAllFilters();
    }
}