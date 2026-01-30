using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using models;

public class ConnectionInfo : MonoBehaviour
{
    public SubConnection data;

    public void Initialize(SubConnection node)
    {
        data = node;
    }
}
