using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class InfoPannelController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI infoText;


    public void setText(NodeInfo data)
    {
        if (data == null)
        {
            Debug.LogWarning("NodeInfo data is null");
            return;
        }

        var node = data.data;
        string ips = NetworkUtils.GetStandardizedIPString(node.ips);
        string src_ports = string.Join(", ", node.src_ports);
        string l7_protos = string.Join(", ", node.l7_protos);

        infoText.text =
            $"<b>Mac Address:</b> {node.mac}\n" +
            $"<b># packets:</b> {node.pkts}\n" +
            $"<b># Bytes:</b> {node.bytes}\n" +
            $"<b>First Seen:</b> {node.first_seen}\n" +
            $"<b>Last Seen:</b> {node.last_seen}\n" +
            $"<b>Degree:</b> {node.degree}\n" +
            $"<b>IP Address:</b> {ips}\n" +
            $"<b>Source Ports:</b> {src_ports}\n" +
            $"<b>Layer 7:</b> {l7_protos}\n" +
            $"<b>Vendor:</b> {node.device_type}\n" +
            $"<b>Alerts:</b> {node.alerts}\n";

        RestartScroll();
    }

    public void setText(ConnectionInfo data)
    {
        if (data == null)
        {
            Debug.LogWarning("ConnectionInfo data is null");
            return;
        }

        var node = data.data;

        infoText.text =
            $"Mac Address 1: {node.node_a_macs}    " +
            $"Mac Address 2: {node.node_b_macs}    " +
            $"Packets: {node.pkts}    " +
            $"Bytes: {node.bytes}";

        RestartScroll();
    }

    private void RestartScroll()
    {
        if (infoText != null)
            infoText.ForceMeshUpdate();

        if (autoScroll != null)
            autoScroll.ResetToStart();
    }
}
