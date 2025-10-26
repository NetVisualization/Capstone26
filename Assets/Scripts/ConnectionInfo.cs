using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectionInfo : MonoBehaviour
{
    public DBConnection.SubConnection data;

    public void Initialize(DBConnection.SubConnection node)
    {
        data = node;
    }
}
