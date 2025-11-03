using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using TMPro;
using UnityEngine;

public class InfoPannelController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI infoText;
    // Start is called before the first frame update

    public void setText(NodeInfo data)
    {
        if (data == null) {
            Debug.LogWarning("data is null");
              return;
        }
        var node = data.data;
        infoText.text =
            $"<b>Mac Address:</b> {node.mac}\n" +
            $"<b># packets:</b> {node.pkts}\n" +
            $"<b># Bytes:</b> {node.bytes}\n" +
            $"<b>:First Seen</b> {node.first_seen}\n" +
            $"<b>Last Seen:</b> {node.last_seen}\n" +
            $"<b>Degree:</b> {node.degree}\n" +
            //$"<b>IP Adress:</b> {node.ips}\n" +
            //$"<b>Source Ports:</b> {node.src_ports}\n" +
            //$"<b>Layer 7:</b> {node.l7_protos}\n" +
            $"<b>Device Type:</b> {node.device_type}\n";
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
    }
}