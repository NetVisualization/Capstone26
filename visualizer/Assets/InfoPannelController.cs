using TMPro;
using UnityEngine;

public class InfoPannelController : MonoBehaviour
{
    [Header("Static Value Fields")]
    [SerializeField] private TextMeshProUGUI macValueText;
    [SerializeField] private TextMeshProUGUI packetsValueText;
    [SerializeField] private TextMeshProUGUI bytesValueText;
    [SerializeField] private TextMeshProUGUI vendorValueText;

    [Header("Scrolling Value Fields")]
    [SerializeField] private TextMeshProUGUI ipValueText;
    [SerializeField] private TextMeshProUGUI portsValueText;
    [SerializeField] private TextMeshProUGUI layer7ValueText;
    [SerializeField] private TextMeshProUGUI alertsValueText;

    [Header("Scrolling Scripts")]
    [SerializeField] private AutoScrollTMP ipScroller;
    [SerializeField] private AutoScrollTMP portsScroller;
    [SerializeField] private AutoScrollTMP layer7Scroller;
    [SerializeField] private AutoScrollTMP alertsScroller;

    public void setText(NodeInfo data)
    {
        if (data == null)
        {
            Debug.LogWarning("NodeInfo data is null");
            return;
        }

        var node = data.data;
<<<<<<< HEAD

        string mac = node.mac != null ? node.mac.ToString() : "";
        string ips = node.ips != null ? string.Join(", ", node.ips) : "";
        string srcPorts = node.src_ports != null ? string.Join(", ", node.src_ports) : "";
        string l7Protos = node.l7_protos != null ? string.Join(", ", node.l7_protos) : "";
        string alerts = node.alerts != null ? string.Join(", ", node.alerts) : "";
        string vendor = node.device_type != null ? node.device_type.ToString() : "";
=======
        string ips = NetworkUtils.GetStandardizedIPString(node.ips);
        string src_ports = string.Join(", ", node.src_ports);
        string l7_protos = string.Join(", ", node.l7_protos);
>>>>>>> 2fe4e3363b60daea8fcc8ac9cb59230c8cab65f5

        if (macValueText != null) macValueText.text = mac;
        if (packetsValueText != null) packetsValueText.text = node.pkts.ToString();
        if (bytesValueText != null) bytesValueText.text = node.bytes.ToString();
        if (vendorValueText != null) vendorValueText.text = vendor;

        if (ipValueText != null) ipValueText.text = ips;
        if (portsValueText != null) portsValueText.text = srcPorts;
        if (layer7ValueText != null) layer7ValueText.text = l7Protos;
        if (alertsValueText != null) alertsValueText.text = alerts;

        RestartScrollers();
    }

    public void setText(ConnectionInfo data)
    {
        if (data == null)
        {
            Debug.LogWarning("ConnectionInfo data is null");
            return;
        }

        var node = data.data;

        string mac1 = node.node_a_macs != null ? node.node_a_macs.ToString() : "";
        string mac2 = node.node_b_macs != null ? node.node_b_macs.ToString() : "";

        if (macValueText != null) macValueText.text = $"{mac1}  <->  {mac2}";
        if (packetsValueText != null) packetsValueText.text = node.pkts.ToString();
        if (bytesValueText != null) bytesValueText.text = node.bytes.ToString();
        if (vendorValueText != null) vendorValueText.text = "";

        if (ipValueText != null) ipValueText.text = "";
        if (portsValueText != null) portsValueText.text = "";
        if (layer7ValueText != null) layer7ValueText.text = "";
        if (alertsValueText != null) alertsValueText.text = "";

        RestartScrollers();
    }

    private void RestartScrollers()
    {
        if (ipValueText != null) ipValueText.ForceMeshUpdate();
        if (portsValueText != null) portsValueText.ForceMeshUpdate();
        if (layer7ValueText != null) layer7ValueText.ForceMeshUpdate();
        if (alertsValueText != null) alertsValueText.ForceMeshUpdate();

        if (ipScroller != null) ipScroller.ResetToStart();
        if (portsScroller != null) portsScroller.ResetToStart();
        if (layer7Scroller != null) layer7Scroller.ResetToStart();
        if (alertsScroller != null) alertsScroller.ResetToStart();
    }
}