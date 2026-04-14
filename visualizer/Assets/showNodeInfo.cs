using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.XR.Interaction.Toolkit;

public class showNodeInfo : MonoBehaviour
{
    [SerializeField] private NodeInfo NodeData;
    [SerializeField] private ConnectionInfo ConnectionData;
    [SerializeField] private IpTag IpData;
    [SerializeField] public InfoPannelController pannel;

    private XRBaseInteractable interactable;

    private void Awake()
    {
        if (!pannel) pannel = FindObjectOfType<InfoPannelController>(true);
        interactable = GetComponent<XRBaseInteractable>();

        if (interactable == null)
        {
            Debug.LogError("showNodeInfo is missing XRBaseInteractable!");
        }
    }

    private void OnEnable()
    {
        if (interactable != null)
            interactable.selectEntered.AddListener(OnSelect);
    }

    private void OnDisable()
    {
        if (interactable != null)
            interactable.selectEntered.RemoveListener(OnSelect);
    }

    private void OnSelect(SelectEnterEventArgs args)
    {
        Debug.Log("selected something");

        // Re-check at click time, because IpTag may be added after Awake()
        IpData = GetComponent<IpTag>();
        NodeData = GetComponent<NodeInfo>();
        ConnectionData = GetComponent<ConnectionInfo>();

        if (pannel && IpData != null)
        {
            Debug.Log($"trying to display IP {IpData.ipString}");
            pannel.setText(IpData);
        }
        else if (pannel && NodeData != null)
        {
            pannel.setText(NodeData);
        }
        else if (pannel && ConnectionData != null)
        {
            Debug.Log($"trying to display {ConnectionData}");
            pannel.setText(ConnectionData);
        }
        else
        {
            Debug.Log("bad");
        }
    }
}