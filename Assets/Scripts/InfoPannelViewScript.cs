using UnityEngine;

public class InfoPannelViewScript : MonoBehaviour
{
    [SerializeField] private Camera cam;

    public float zDistance = 1f;       // forward distance from camera
    public Vector2 margin = new Vector2(0.35f, 0.25f); // right & bottom margins
    public float moveSpeed = 10f;
    public float rotateSpeed = 12f;

    void Awake()
    {
        if (!cam) cam = Camera.main;
    }

    void LateUpdate()
    {
        if (!cam) return;

        // Bottom-right of screen in viewport space = (1,0)
        Vector3 viewportPoint = new Vector3(1f - margin.x, 0f + margin.y, zDistance);

        // Convert from viewport to world position
        Vector3 targetPos = cam.ViewportToWorldPoint(viewportPoint);

        // Smooth movement
        transform.position = Vector3.Lerp(
            transform.position,
            targetPos,
            Time.deltaTime * moveSpeed
        );

        // Make it face the camera
        Quaternion targetRot = Quaternion.LookRotation(
            transform.position - cam.transform.position
        );

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRot,
            rotateSpeed * Time.deltaTime
        );
    }
}
