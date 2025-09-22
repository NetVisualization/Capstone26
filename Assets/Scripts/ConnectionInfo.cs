using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectionInfo : MonoBehaviour
{
    public DBConnection.Connection data;

    public void Initialize(DBConnection.Connection node)
    {
        data = node;
    }
}
