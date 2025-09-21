using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelToggler : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private GameObject targetPannel;

    private void Start()
    {
        if(targetPannel != null)
        {
            targetPannel.SetActive(false); // hides pannel at runtime
        }
    }
    public void Toggle()
    {
        if (targetPannel != null)
            targetPannel.SetActive(!targetPannel.activeSelf); // toggle the pannel to show and not show
    }
}
