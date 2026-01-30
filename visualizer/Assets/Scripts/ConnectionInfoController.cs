using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.XR.Interaction.Toolkit;

public class ConnectionInfoController : MonoBehaviour
{
    [SerializeField] private ConnectionInfo data;
    [SerializeField] public InfoPannelController pannel;
    private XRBaseInteractable interactable;
    // Start is called before the first frame update
    private void Awake()
    {
        if (!pannel) pannel = FindObjectOfType<InfoPannelController>(true);
        interactable = GetComponent<XRBaseInteractable>();
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
        if (pannel && data)
        {
            pannel.setText(data);
        }
    }
}
