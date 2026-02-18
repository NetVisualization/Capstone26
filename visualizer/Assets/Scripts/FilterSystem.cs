using System;
using UnityEngine;
using models; // l7_proto

public class FilterSystem : MonoBehaviour
{
    [SerializeField] private NodeSpawnerScript spawner;

    // Track state so live traffic can be applied correctly later if you want
    private readonly System.Collections.Generic.HashSet<l7_proto> hiddenProtocols = new();
    private bool macIpHidden = false;

    private void Awake()
    {
        if (!spawner)
            spawner = FindFirstObjectByType<NodeSpawnerScript>();
    }

    // ✅ Call THIS from FilterButtonVisual:
    // filterKey: "SSH", "DNS", ... or "MACIP"
    // filterOn: true means "hide that thing"
    public void SetFilterState(string filterKey, bool filterOn)
    {
        if (spawner == null) return;

        // Special case: vertical edges (MAC <-> IP)
        if (filterKey.Equals("MACIP", StringComparison.OrdinalIgnoreCase))
        {
            macIpHidden = filterOn;
            SetMacIpEdgesVisible(!macIpHidden);   // visible = !hidden
            RefreshNodeVisibility();
            return;
        }

        // Otherwise treat as protocol
        if (!Enum.TryParse(filterKey, ignoreCase: true, out l7_proto proto))
        {
            Debug.LogWarning($"Filter key '{filterKey}' doesn't match l7_proto enum.");
            return;
        }

        if (filterOn) hiddenProtocols.Add(proto);
        else hiddenProtocols.Remove(proto);

        SetProtocolEdgesVisible(proto, visible: !filterOn); // visible = !hidden
        RefreshNodeVisibility();
    }

    private void SetProtocolEdgesVisible(l7_proto proto, bool visible)
    {
        foreach (var edge in spawner.Connections)
        {
            if (!edge) continue;

            var tag = edge.GetComponent<EdgeTag>();
            if (tag == null) continue;

            if (tag.isMacMac && tag.protocol == proto)
                edge.SetActive(visible);
        }
    }

    public void SetMacIpEdgesVisible(bool visible)
    {
        foreach (var edge in spawner.Connections)
        {
            if (!edge) continue;

            var tag = edge.GetComponent<EdgeTag>();
            if (tag != null && tag.isMacIp)
                edge.SetActive(visible);
        }
    }

    public void HideAllConnections()
    {
        foreach (var c in spawner.Connections)
            if (c) c.SetActive(false);

        hiddenProtocols.Clear();
        macIpHidden = true;

        RefreshNodeVisibility();
    }

    public void ShowAllConnections()
    {
        foreach (var c in spawner.Connections)
            if (c) c.SetActive(true);

        hiddenProtocols.Clear();
        macIpHidden = false;

        RefreshNodeVisibility();
    }

    // Same as your current version (MAC nodes hide if no active edges)
    public void RefreshNodeVisibility()
    {
        if (spawner == null) return;

        foreach (var kv in spawner.MacNodes)
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

        // keep IP nodes on (or later you can hide them similarly)
        foreach (var ipGO in spawner.IpNodeList)
            if (ipGO) ipGO.SetActive(true);
    }
}
