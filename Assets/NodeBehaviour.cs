using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using models;

public class NodeBehaviour : MonoBehaviour
{
    public Node data;
    // Start is called before the first frame update
    public void Initialize(Node node)
    {
        data = node;
    }
}
