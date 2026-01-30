using UnityEngine;
using UnityEngine.InputSystem;

public class MenuHandler : MonoBehaviour
{
    // Reference to the X button on the left controller
    public InputAction xButton;

    public GameObject inspectorPanel;
    public GameObject protocolPanel;
    public GameObject blacklistPanel;
    public GameObject timestampPanel;

    public FilterSystem filterSystem;

    private void OnEnable()
    {
        xButton.Enable();
        //xButton.performed += CyclePanels;

        //xButton.performed += Whitelistbuttons;
    }

    private void ToggleProtocolPanel(InputAction.CallbackContext context)
    {
        protocolPanel.SetActive(!protocolPanel.activeSelf);
    }

    private void Whitelistbuttons(InputAction.CallbackContext context)
    {
        //protocolPanel.SetActive(!protocolPanel.activeSelf);
        //filterSystem.CreateWhitelistButtons();
    }

    // Cycle panels from all=>inspector=>protocolfilter=>none
    // As more panels get added, either get a new system or use binary strings
    private void CyclePanels(InputAction.CallbackContext context)
    {
        if (protocolPanel.activeSelf == inspectorPanel.activeSelf)
        {
            ToggleLH(context);
        } else {
            inspectorPanel.SetActive(!inspectorPanel.activeSelf);
        }
        /*if (protocolPanel.activeSelf && inspectorPanel.activeSelf)
        {
            protocolPanel.SetActive(false);
            inspectorPanel.SetActive(true);

        } else if (!protocolPanel.activeSelf && inspectorPanel.activeSelf)
        {
            protocolPanel.SetActive(true);
            inspectorPanel.SetActive(false);

        } else if (protocolPanel.activeSelf && !inspectorPanel.activeSelf)
        {
            protocolPanel.SetActive(false);
            inspectorPanel.SetActive(false);
        } else
        {
            protocolPanel.SetActive(true);
            inspectorPanel.SetActive(true);
        }*/
    }

    private void ToggleLH(InputAction.CallbackContext context)
    {
        if (protocolPanel.activeSelf)
        {
            protocolPanel.SetActive(false);
            blacklistPanel.SetActive(false);
            timestampPanel.SetActive(false);
        } else
        {
            protocolPanel.SetActive(true);
            blacklistPanel.SetActive(true);
            timestampPanel.SetActive(true);
        }
    }

    private void OnDisable()
    {
        xButton.Disable();
    }
}
