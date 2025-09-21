using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeData : MonoBehaviour
{
    public string Id;
    public string MACAddress;
    public string IPAddress;
    public string DeviceType;
    public int NumConnections;
    public int NumPackets;
    public bool[] FilterFlags = {true, true, true};
    public string Vendor;
    public bool isHidden = false;
}
