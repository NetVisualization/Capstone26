using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
[RequireComponent(typeof(XRRayInteractor))]
public class RayPointer : MonoBehaviour
{
    public float length = 5f; // visual length of the laser
    XRRayInteractor ray;
    XRInteractorLineVisual lineVisual;

    void Awake()
    {
        ray = GetComponent<XRRayInteractor>();
        lineVisual = GetComponent<XRInteractorLineVisual>();
    }

    void LateUpdate()
    {
        if (!ray) return;

        // Keep it a straight line, never curve toward targets
        ray.lineType = XRRayInteractor.LineType.StraightLine;

        // Optional: fix visual length (just for display)
        if (lineVisual)
        {
            // This ensures the line doesn't shorten on hit
            lineVisual.stopLineAtFirstRaycastHit = false;
            lineVisual.lineLength = length;
        }
    }
}

