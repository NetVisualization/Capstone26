using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeBehaviour : MonoBehaviour
{
    public DBConnection.Node data;
    // Start is called before the first frame update
    public void Initialize(DBConnection.Node node)
    {
        data = node;
    }
}
