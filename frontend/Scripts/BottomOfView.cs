using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;
using ZstdSharp.Unsafe;

public class BottomOfView : MonoBehaviour
{
    [SerializeField] private Camera cam;

    public float distance = 1.2f;
    public float bottomMargin = -0.75f;
    public float moveSpeed = 10f;
    public float maxTurnDegPerSec = 12f;

    public DBConnection db;
    
    void Awake()
    {
        //if (!cam) cam = Camera.main;
        db = new DBConnection();
    }

    void LateUpdate()
    {
        if (!cam) return;
        
        //Position at bottom of viewport

        // used to avoid flip/roll
        Vector3 flatForward = Vector3. ProjectOnPlane(cam.transform.forward,Vector3.up).normalized;
        if (flatForward.sqrMagnitude < 1e-6f)
            flatForward = Vector3.forward; //just in case

        float targetY = cam.transform.position.y + bottomMargin;
        Vector3 targetPos = cam.transform.position + flatForward * distance;
        targetPos.y = targetY;

        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * moveSpeed);


        Quaternion targetRot = Quaternion.LookRotation(flatForward, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, maxTurnDegPerSec);

    }
}
