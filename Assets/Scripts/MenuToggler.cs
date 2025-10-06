using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuToggler : MonoBehaviour
{
    [SerializeField] private GameObject targetMenu;

    private void Start()
    {
        if(targetMenu!= null)
        {
            targetMenu.SetActive(false);
        }

    }

    public void toggleMenu()
    {
        if (targetMenu != null)
        {
            bool newstate = !targetMenu.activeSelf;
            targetMenu.SetActive(newstate);
            Debug.Log("Pannel Clicked");
        }
    }
}
