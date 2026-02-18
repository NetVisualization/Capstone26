using System;
using UnityEngine;
using models; // for l7_proto

public class FilterSystem : MonoBehaviour
{
    [SerializeField] private NodeSpawnerScript spawner;

    private void Awake()
    {
        if (!spawner)
            spawner = FindFirstObjectByType<NodeSpawnerScript>();
    }

    // Call this from your button: filterKey = "DNS", isOn = true/false
    public void SetProtocolVisible(string filterKey, bool visible)
    {
        if (spawner == null) return;

        if (!Enum.TryParse(filterKey, ignoreCase: true, out l7_proto proto))
        {
            Debug.LogWarning($"Filter key '{filterKey}' doesn't match l7_proto enum.");
            return;
        }

        // 1) Toggle only edges of that protocol
        foreach (var edge in spawner.Connections)
        {
            if (!edge) continue;

            var tag = edge.GetComponent<EdgeTag>();
            if (tag == null) continue;

            // Only affect MAC-MAC protocol edges (your MAC-IP edges are UNKNOWN)
            if (tag.isMacMac && tag.protocol == proto)
                edge.SetActive(visible);
        }

        // 2) (Optional but nice) Update node visibility so orphan nodes disappear
        RefreshNodeVisibility();
    }

    // Optional: hide nodes that have ZERO active edges touching them
    public void RefreshNodeVisibility()
    {
        if (spawner == null) return;

        // MAC nodes
        foreach (var kv in spawner.MacNodes) // Dictionary<string, GameObject>
        {
            string mac = kv.Key;
            var nodeGO = kv.Value;
            if (!nodeGO) continue;

            bool anyActive = false;
            if (spawner.EdgesByMac.TryGetValue(mac, out var edges))
            {
                for (int i = 0; i < edges.Count; i++)
                {
                    var e = edges[i];
                    if (e && e.activeSelf) { anyActive = true; break; }
                }
            }

            nodeGO.SetActive(anyActive);
        }

        // IP nodes (only if you want them hidden too)
        foreach (var ipGO in spawner.IpNodeList)
        {
            if (!ipGO) continue;

            // if your IP nodes have no data key attached, simplest is: keep them on
            // Or you can add an IpTag component when creating IP nodes and check EdgesByIp.
            ipGO.SetActive(true);
        }
    }

    public void HideAllConnections()
    {
        foreach (var c in spawner.Connections)
            if (c) c.SetActive(false);

        RefreshNodeVisibility();
    }

    public void ShowAllConnections()
    {
        foreach (var c in spawner.Connections)
            if (c) c.SetActive(true);

        RefreshNodeVisibility();
    }
}
