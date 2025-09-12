using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR;

public class ObjectSelector : MonoBehaviour
{
    public XRRayInteractor ray;
    public ActionBasedController controller;
    public InputDevice inputDevice;

    public TextMeshProUGUI display;

    public Material black;
    public Material blue;
    public Material transGray;

    public bool gVal;
    public RaycastHit hit;

    private void Start()
    {
        var rightHandDevices = new List<UnityEngine.XR.InputDevice>();
        UnityEngine.XR.InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Right, rightHandDevices);

        if (rightHandDevices.Count >= 1)
        {
            inputDevice = rightHandDevices[0];
        }
        else if (rightHandDevices.Count > 1)
        {
            Debug.Log("Found more than one left hand!");
        }
    }

    private void Update()
    {
        if (inputDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out gVal) && gVal)
        {
            display.text = $"hit: yes";
            //targets = ray.interactablesHovered;

            //if (targets.Count > 0)
            //{
            //    SelectEntered();
            //}
            if (ray.TryGetCurrent3DRaycastHit(out hit))
            {
                SelectEntered();
            }
        }
    }

    public void HoverEntered(HoverEnterEventArgs e)
    {
        // Get targeted game object
        GameObject obj = e.interactableObject.transform.gameObject;

        //obj.transform.localScale.Scale(growth.Abs());

        if (obj.CompareTag("node"))
        {
            if (obj.GetComponent<NodeData>().isHidden)
            {
                obj.GetComponent<MeshRenderer>().material = transGray;
            }
            else
            {
                obj.GetComponent<MeshRenderer>().material = blue;
            }

            NodeData nodeData = obj.GetComponent<NodeData>();

            display.text = $"Node ID: {nodeData.Id}\n" +
                           $"IP Address: {nodeData.IPAddress}\n" +
                           $"MAC Address: {nodeData.MACAddress}\n" +
                           $"Device Type: {nodeData.DeviceType}\n" +
                           $"# Connections: {nodeData.NumConnections}\n" +
                           $"# Packets: {nodeData.NumPackets}\n" +
                           $"Vendor: {nodeData.Vendor}";
        }

        else if (obj.CompareTag("connection"))
        {
            ConnectionData connectionData = obj.GetComponent<ConnectionData>();

            NodeData nodeAData = connectionData.NodeA.GetComponent<NodeData>();
            NodeData nodeBData = connectionData.NodeB.GetComponent<NodeData>();

            display.text = $"Connection ID: {connectionData.Id}\n" +
                           $"Node A IP: {nodeAData.IPAddress}\n" +
                           $"Node B IP: {nodeBData.IPAddress}\n" +
                           $"# Packets: {connectionData.NumPackets}";
        }
    }

    public void HoverExited(HoverExitEventArgs e)
    {
        GameObject obj = e.interactableObject.transform.gameObject;

        //obj.transform.localScale.Scale(growth);

        if (obj.CompareTag("node"))
        {
            if (obj.GetComponent<NodeData>().isHidden)
            {
                obj.GetComponent<MeshRenderer>().material = transGray;
            } else
            {
                obj.GetComponent<MeshRenderer>().material = black;
            }
        }
    }

    public void SelectEntered()
    {
        // When hiding a node do the following:
        //   Set node transparency to 20%
        //   Set isHidden attribute of node's NodeData to true
        //   Hide all connections associated with the node

        // Get targeted game object
        GameObject obj = hit.collider.gameObject;

        if (obj.CompareTag("node"))
        {
            if (!obj.GetComponent<NodeData>().isHidden)
            {
                // Set transparency
                obj.GetComponent<MeshRenderer>().material = transGray;

                // Set isHidden
                obj.GetComponent<NodeData>().isHidden = true;

                // SpawnerScript.cs handles hiding connections

            }
            else
            {
                // Set transparency
                obj.GetComponent<MeshRenderer>().material = black;

                // Set isHidden
                obj.GetComponent<NodeData>().isHidden = false;

                // SpawnerScript.cs handles hiding connections
            }
        }
    }

    public void SelectEntered(SelectEnterEventArgs e)
    {
        // When hiding a node do the following:
        //   Set node transparency to 20%
        //   Set isHidden attribute of node's NodeData to true
        //   Hide all connections associated with the node


        // Get targeted game object
        GameObject obj = e.interactableObject.transform.gameObject;

        if (obj.CompareTag("node"))
        {
            if (!obj.GetComponent<NodeData>().isHidden)
            {
                // Set transparency
                obj.GetComponent<MeshRenderer>().material = transGray;

                // Set isHidden
                obj.GetComponent<NodeData>().isHidden = true;

                // SpawnerScript.cs handles hiding connections

            } else
            {
                // Set transparency
                obj.GetComponent<MeshRenderer>().material = black;

                // Set isHidden
                obj.GetComponent<NodeData>().isHidden = false;

                // SpawnerScript.cs handles hiding connections
            }
        }
    }

    public void SelectExited(SelectExitEventArgs e)
    {
        ;
    }
}
