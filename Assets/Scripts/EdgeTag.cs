using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class EdgeTag : MonoBehaviour
{
    public string mac_a;
    public string mac_b;

    public IPAddress ipA;
    public IPAddress ipB;

    public IPAddress ip;

    public bool isMacMac;
    public bool isMacIp;

    public l7_proto protocol;
}
