using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeInfo : MonoBehaviour
{
    public DBConnection.Node data;

    public void Initialize(DBConnection.Node node)
    {
        data = node;
    }

}
