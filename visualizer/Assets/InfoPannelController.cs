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

        string ips = string.Join(", ", node.ips);
        string src_ports = string.Join(", ", node.src_ports);
        string l7_protos = string.Join(", ", node.l7_protos);

        infoText.text =
            $"Mac Address: {node.mac}    " +
            $"Packets: {node.pkts}    " +
            $"Bytes: {node.bytes}    " +
            $"First Seen: {node.first_seen}    " +
            $"Last Seen: {node.last_seen}    " +
            $"Degree: {node.degree}    " +
            $"IP Address: {ips}    " +
            $"Source Ports: {src_ports}    " +
            $"Layer 7: {l7_protos}    " +
            $"Vendor: {node.device_type}";

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