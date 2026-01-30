using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

public class VerticalMovement : MonoBehaviour
{
    public InputAction moveAction;
    public float speed = 2.0f;

    private Vector2 primaryAxis;

    private void OnEnable()
    {
        moveAction.Enable();
    }

    private void OnDisable()
    {
        moveAction.Disable();
    }

    // Update is called once per frame
    void Update()
    {
        primaryAxis = moveAction.ReadValue<Vector2>();
        Vector3 move = new Vector3(0, primaryAxis.y, 0) * speed * Time.deltaTime;
        transform.Translate(move);
    }
}
