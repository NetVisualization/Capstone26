using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixVRViewport : MonoBehaviour
{
    public Camera cam;
    void Reset() { if (!cam) cam = Camera.main; }

    private void LateUpdate()
    {
        {
            if(!cam) return;
            if (cam.targetTexture) cam.targetTexture = null;
            if(cam.rect != new Rect(0,0,1,1)) cam.rect = new Rect(0,0,1,0);
            if (cam.clearFlags == CameraClearFlags.Nothing || cam.clearFlags == CameraClearFlags.Depth)
                cam.clearFlags = CameraClearFlags.Skybox;
            cam.stereoTargetEye = StereoTargetEyeMask.Both;

        }
    }
}
