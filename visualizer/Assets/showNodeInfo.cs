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
    [SerializeField] public InfoPannelController pannel;
    private XRBaseInteractable interactable;
    // Start is called before the first frame update
    private void Awake()
    {
        if (!pannel) pannel = FindObjectOfType<InfoPannelController>(true);
        interactable = GetComponent<XRBaseInteractable>();

        // find the sibling component on this connection object
        if (ConnectionData == null)
        {
            ConnectionData = GetComponent<ConnectionInfo>();
        }

        if (ConnectionData == null && NodeData == null)
        {
            Debug.LogError("showNodeInfo is missing a data source!");
        }

        if (pannel)
        {
            Debug.Log("Found panel");
        }
        else
        {
            Debug.Log("Pannel not found");
        }
    }
    private void OnEnable()
    {

        interactable.selectEntered.AddListener(OnSelect);
    }
    private void OnDisable()
    {
        interactable.selectEntered.RemoveListener(OnSelect);
    }

    private void OnSelect(SelectEnterEventArgs args)
    {
        Debug.Log("selected something");
        if (pannel && NodeData)
        {
            pannel.setText(NodeData);
        }
        else if (pannel && ConnectionData)
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
