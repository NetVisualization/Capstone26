using TMPro;
using UnityEngine;
using UnityEngine.UI;   // <-- IMPORTANT (for LayoutRebuilder)

public class InfoPannelController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI infoText;

    // Drag your black Panel (background) RectTransform here in the Inspector
    [SerializeField] private RectTransform panelRect;

    public void setText(NodeInfo data)
    {
        if (data == null) {
            Debug.LogWarning("data is null");
            return;
        }

        var node = data.data;

        string ips = string.Join(",", node.ips);
        string src_ports = string.Join(",", node.src_ports);
        string l7_protos = string.Join(",", node.l7_protos);

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
            $"<b>Vendor:</b> {node.device_type}\n";

        RefreshLayout();
    }

    public void setText(ConnectionInfo data)
    {
        if (data == null)
        {
            Debug.LogWarning("data is null");
            return;
        }

        var node = data.data;

        infoText.text =
            $"<b>Mac Address 1:</b> {node.node_a_macs}\n" +
            $"<b>Mac Address 2:</b> {node.node_b_macs}\n" +
            $"<b># packets:</b> {node.pkts}\n" +
            $"<b># Bytes:</b> {node.bytes}\n";

        RefreshLayout();
    }

    private void RefreshLayout()
    {
        // If you forget to assign panelRect, fall back to the text's parent
        if (panelRect == null)
            panelRect = infoText.transform.parent as RectTransform;

        // Force TMP to update its geometry + preferred values
        infoText.ForceMeshUpdate();

        // Force Unity UI to recalc layout right now
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(panelRect);
    }
}
