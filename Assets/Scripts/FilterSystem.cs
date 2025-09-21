using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FilterSystem : MonoBehaviour
{
    //public NodeSpawnerScript spawner;

    //public Slider startslider;
    //public Slider endslider;

    //public TextMeshProUGUI whitelistDisplay;
    //public GameObject whitelistButtonPrefab;
    //public GameObject whitelistCanvas;
    //public GameObject whitelistToggle;

    //// Hold IPs that we want to (exclusively) show
    //public List<string> IPWhitelist = new();
    //public Dictionary<string, GameObject> WhitelistButtons = new();

    //public Dictionary<string, DateTime> packetTSMap = new();

    //public void Start()
    //{
    //    // The 0.0.0.0 button stays active in the unity editor cause I want it to, this hides it come runtime
    //    whitelistButtonPrefab.SetActive(false);
    //}

    //public void ApplyFilterStates()
    //{
    //    Dictionary<string, GameObject> nodes = spawner.GetNodes();
    //    Dictionary<string, GameObject> conns = spawner.GetConnections();

    //    foreach ((_, GameObject node) in nodes)
    //    {
    //        // If every element in the array is true, show the node
    //        node.SetActive(node.GetComponent<NodeData>().FilterFlags.All(b => b));
    //    }

    //    // Same thing for connections... show if both nodes are on
    //    foreach ((_, GameObject conn) in conns)
    //    {
    //        conn.SetActive((conn.GetComponent<ConnectionData>().NodeA.activeSelf &&
    //                        conn.GetComponent<ConnectionData>().NodeB.activeSelf));
    //    }
    //}

    //public void CreateWhitelistButtons(List<DBConnection.Node> nodes)
    //{
    //    // Get a reference to the canvas's transform (transforms combine position/scale/rotation)
    //    Transform canvasPos = whitelistCanvas.transform;

    //    // When setting the position of children objects we use relative numbers
    //    float yPos = 71f;

    //    // temporary: limit how many buttons we show
    //    int showlimit = 10;

    //    foreach (DBConnection.Node node in nodes)
    //    {
    //        // Ignore nodes that don't have IP data (i.e. ethernet packets)
    //        if (node.IPaddr == "" || WhitelistButtons.ContainsKey(node.IPaddr)) continue;

    //        // Create the new button with the correct "prefab", parent object, and location
    //        // "prefab" because it's not technically a prefab; the reference object is in the scene so we can
    //        //   have a reference to FilterSystem in the OnClick() panel of each button. Prefabs can't do this
    //        //   because references are only possible within the scope of the object
    //        GameObject buttonObject = Instantiate(whitelistButtonPrefab, canvasPos, false);
    //        buttonObject.transform.localPosition = new Vector3(0, yPos, 0);
    //        buttonObject.SetActive(true);

    //        // Set text to the IP address of the current node
    //        buttonObject.GetComponentInChildren<TextMeshProUGUI>().text = node.IPaddr;

    //        WhitelistButtons.Add(node.IPaddr, buttonObject);

    //        yPos -= 12f;

    //        showlimit--;
    //        if (showlimit == 0) break;
    //    }
    //}

    //// Functions for modifying the whitelist
    //public void ClearIPWhitelist()
    //{
    //    IPWhitelist.Clear();

    //    foreach ((_, GameObject button) in WhitelistButtons)
    //    {
    //        button.GetComponent<UnityEngine.UI.Image>().color = Color.white;
    //    }

    //    whitelistToggle.GetComponent<Toggle>().isOn = true;
    //    FilterOnWhitelist();
    //}
    //public void ToggleIPInWhitelist(GameObject button)
    //{
    //    string ip = button.GetComponentInChildren<TextMeshProUGUI>().text;

    //    if (IPWhitelist.Contains(ip))
    //    {
    //        IPWhitelist.Remove(ip);
    //        button.GetComponent<UnityEngine.UI.Image>().color = Color.white;
    //    }
    //    else
    //    {
    //        IPWhitelist.Add(ip);
    //        button.GetComponent<UnityEngine.UI.Image>().color = Color.green;
    //    }

    //    // From a UX perspective it makes sense to turn on the filter when we toggle an IP
    //    whitelistToggle.GetComponent<Toggle>().isOn = true;
    //    FilterOnWhitelist();
    //}

    //public void updateWhitelistDisplay()
    //{
    //    whitelistDisplay.text = "";
    //    foreach (string ip in IPWhitelist)
    //    {
    //        whitelistDisplay.text += $"{ip}\n";
    //    }
    //}

    //public void ToggleApplyWhitelist(GameObject whitelistToggle)
    //{
    //    if (whitelistToggle.GetComponent<UnityEngine.UI.Toggle>().isOn)
    //    {
    //        FilterOnWhitelist();
    //    }
    //    else
    //    {
    //        ResetFilter("ipwhitelist");
    //    }
    //}

    //public void UpdatePacketMap()
    //{
    //    List<DBConnection.Packet> packets = spawner.GetPackets();
    //    packetTSMap.Clear();

    //    foreach (DBConnection.Packet packet in packets)
    //    {
    //        packetTSMap[packet._id.oid] = DateTime.ParseExact(packet.TimestampStr, "MM-dd-yyyy HH:mm:ss.fffff", null);
    //    }
    //}

    //public void FilterOnProtocol(string protocol)
    //{
    //    bool show;
    //    string ip;

    //    // Get data objects from the spawner
    //    Dictionary<string, GameObject> nodes = spawner.GetNodes();
    //    List<DBConnection.Packet> packets = spawner.GetPackets();

    //    foreach ((_, GameObject node) in nodes)
    //    {
    //        // Default to hiding the node
    //        show = false;
    //        ip = node.GetComponent<NodeData>().IPAddress;

    //        foreach (DBConnection.Packet packet in packets)
    //        {
    //            // If we find a packet that has the required protocol and travels to/from this node, show the node
    //            if (packet.Protocol == protocol && (packet.SourceIP == ip || packet.DestinationIP == ip))
    //            {
    //                show = true;
    //                break;
    //            }
    //        }

    //        node.GetComponent<NodeData>().FilterFlags[0] = show;
    //    }

    //    ApplyFilterStates();
    //}

    //public void FilterOnTimestamp(GameObject display)
    //{
    //    // Get data objects from the spawner
    //    Dictionary<string, GameObject> nodes = spawner.GetNodes();
    //    Dictionary<string, GameObject> connections = spawner.GetConnections();
    //    List<DBConnection.Packet> packets = spawner.GetPackets();
    //    UpdatePacketMap();

    //    // Find the earliest/latest timestamps
    //    DateTime start = DateTime.MaxValue;
    //    DateTime end = DateTime.MinValue;

    //    foreach (DBConnection.Packet packet in packets)
    //    {
    //        // Convert the stringstamp stored in the database to a DateTime object for comparison
    //        DateTime ptime = packetTSMap[packet._id.oid];

    //        if (ptime < start) start = ptime;
    //        if (ptime > end) end = ptime;
    //    }

    //    // Get the time delta between the start/end
    //    TimeSpan delta = end - start;

    //    // Convert the 0-1 ranges from the sliders to actual times
    //    // startslider: 0=earliest, 1=latest
    //    // endslider: 1=earliest, 0=latest
    //    DateTime fromTime = start + (delta * startslider.value);
    //    DateTime toTime = end - (delta * endslider.value);

    //    bool show;
    //    string ip;

    //    // Loop through each node and check if it has a packet within the time constraints
    //    foreach ((_, GameObject node) in nodes)
    //    {
    //        // Default to hiding the node
    //        show = false;
    //        ip = node.GetComponent<NodeData>().IPAddress;

    //        foreach (DBConnection.Packet packet in packets)
    //        {
    //            DateTime ptime = packetTSMap[packet._id.oid];

    //            // If we find a packet that has the required timestamp, show the node
    //            if (fromTime <= ptime && ptime <= toTime && (packet.SourceIP == ip || packet.DestinationIP == ip))
    //            {
    //                show = true;
    //                break;
    //            }
    //        }

    //        node.GetComponent<NodeData>().FilterFlags[1] = show;
    //    }

    //    ApplyFilterStates();

    //    // Update the text on the panel
    //    display.GetComponent<TextMeshProUGUI>().text = $"{fromTime:MM-dd-yyyy HH:mm:ss.fffff}\n{toTime:MM-dd-yyyy HH:mm:ss.fffff}";
    //}

    //public void FilterOnWhitelist()
    //{
    //    // Get node/connection objects
    //    Dictionary<string, GameObject> nodes = spawner.GetNodes();
    //    Dictionary<string, GameObject> connections = spawner.GetConnections();

    //    foreach ((_, GameObject node) in nodes)
    //    {
    //        // Show a node if its IP is in the whitelist
    //        node.GetComponent<NodeData>().FilterFlags[2] = IPWhitelist.Contains(node.GetComponent<NodeData>().IPAddress);
    //    }

    //    ApplyFilterStates();

    //    // Add IPs to the display
    //    updateWhitelistDisplay();
    //}

    //public void ResetFilter(string filter)
    //{
    //    // Get data objects from the spawner
    //    Dictionary<string, GameObject> nodes = spawner.GetNodes();
    //    Dictionary<string, GameObject> connections = spawner.GetConnections();
    //    //List<DBConnection.Packet> packets = spawner.GetPackets();

    //    // Protocol filter
    //    if (filter == "protocol")
    //    {
    //        foreach ((_, GameObject node) in nodes)
    //        {
    //            node.GetComponent<NodeData>().FilterFlags[0] = true;
    //        }
    //    }

    //    // Timestamp filter
    //    else if (filter == "timestamp")
    //    {
    //        // Reset the slider values
    //        startslider.value = 0;
    //        endslider.value = 0;

    //        foreach ((_, GameObject node) in nodes)
    //        {
    //            node.GetComponent<NodeData>().FilterFlags[1] = true;
    //        }
    //    }

    //    // IP Whitelist
    //    else if (filter == "ipwhitelist")
    //    {
    //        foreach ((_, GameObject node) in nodes)
    //        {
    //            node.GetComponent<NodeData>().FilterFlags[2] = true;
    //        }
    //    }

    //    ApplyFilterStates();
    //}
}
