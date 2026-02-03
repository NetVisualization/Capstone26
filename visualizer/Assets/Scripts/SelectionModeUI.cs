using System.Collections;
using TMPro;
using UnityEngine;

public class SelectionModeUI : MonoBehaviour
{
    public enum SelectMode { Both, Nodes, Connections }

    [Header("UI References (TextMeshPro)")]
    [SerializeField] private TMP_Text persistentText;
    [SerializeField] private TMP_Text flashText;

    [Header("Persistent Labels")]
    [SerializeField] private string bothPersistentLabel = "Selection Mode: BOTH";
    [SerializeField] private string nodesPersistentLabel = "Selection Mode: NODES";
    [SerializeField] private string connectionsPersistentLabel = "Selection Mode: CONNECTIONS";

    [Header("Flash Labels")]
    [SerializeField] private string bothFlashLabel = "→ Node + Connection selection enabled";
    [SerializeField] private string nodesFlashLabel = "→ Node selection enabled";
    [SerializeField] private string connectionsFlashLabel = "→ Connection selection enabled";

    [Header("Colors (optional)")]
    [SerializeField] private Color bothColor = Color.white;
    [SerializeField] private Color nodesColor = Color.cyan;
    [SerializeField] private Color connectionsColor = new Color(1f, 0.6f, 0.1f);

    [Header("Flash Timing")]
    [SerializeField] private float flashVisibleTime = 0.6f;
    [SerializeField] private float flashFadeOutTime = 0.25f;

    [Header("Optional: Follow Camera (HUD)")]
    [Tooltip("If true, this object will follow the main camera and act like a HUD.")]
    [SerializeField] private bool followCamera = false;

    [Tooltip("If followCamera is true and this is empty, it will auto-use Camera.main.")]
    [SerializeField] private Transform cameraTransform;

    [Tooltip("Offset in camera-local space (x=right, y=up, z=forward).")]
    [SerializeField] private Vector3 localOffset = new Vector3(0f, -0.2f, 1.2f);

    [Tooltip("If true, rotates the HUD to face the camera.")]
    [SerializeField] private bool faceCamera = true;

    private Coroutine flashRoutine;
    private SelectMode currentMode = SelectMode.Both;

    private void Awake()
    {
        HideFlashImmediate();

        if (followCamera && cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;
    }

    private void LateUpdate()
    {
        if (!followCamera || cameraTransform == null) return;

        // Position in front of camera using localOffset in camera space
        transform.position = cameraTransform.TransformPoint(localOffset);

        if (faceCamera)
        {
            // Face the camera (billboard). Keeps text readable.
            Vector3 lookDir = transform.position - cameraTransform.position;
            if (lookDir.sqrMagnitude > 0.0001f)
                transform.rotation = Quaternion.LookRotation(lookDir);
        }
    }

    /// <summary>
    /// Update the persistent label, and optionally flash a toast message.
    /// Call this from your RayLayerToggle.ApplyMode().
    /// </summary>
    public void SetMode(SelectMode mode, bool showFlash = true)
    {
        currentMode = mode;

        string persistent;
        string flash;
        Color color;

        switch (mode)
        {
            case SelectMode.Nodes:
                persistent = nodesPersistentLabel;
                flash = nodesFlashLabel;
                color = nodesColor;
                break;

            case SelectMode.Connections:
                persistent = connectionsPersistentLabel;
                flash = connectionsFlashLabel;
                color = connectionsColor;
                break;

            default:
                persistent = bothPersistentLabel;
                flash = bothFlashLabel;
                color = bothColor;
                break;
        }

        // Persistent label (always visible)
        if (persistentText != null)
        {
            persistentText.text = persistent;
            persistentText.color = color;
        }

        // Flash/toast (brief)
        if (showFlash)
        {
            Flash(flash, color);
        }
        else
        {
            HideFlashImmediate();
        }
    }

    /// <summary>
    /// Optional helper if you want to toggle cycle in UI (not required for your setup).
    /// </summary>
    public void CycleMode(bool showFlash = true)
    {
        SelectMode next = currentMode switch
        {
            SelectMode.Both => SelectMode.Nodes,
            SelectMode.Nodes => SelectMode.Connections,
            _ => SelectMode.Both
        };

        SetMode(next, showFlash);
    }

    private void Flash(string message, Color color)
    {
        if (flashText == null) return;

        if (flashRoutine != null)
            StopCoroutine(flashRoutine);

        flashRoutine = StartCoroutine(FlashRoutine(message, color));
    }

    private IEnumerator FlashRoutine(string message, Color color)
    {
        // Set text and show immediately
        flashText.text = message;
        color.a = 1f;
        flashText.color = color;

        yield return new WaitForSeconds(flashVisibleTime);

        // Fade out
        float t = 0f;
        float startA = flashText.color.a;

        while (t < flashFadeOutTime)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(startA, 0f, t / flashFadeOutTime);

            Color c = flashText.color;
            c.a = a;
            flashText.color = c;

            yield return null;
        }

        HideFlashImmediate();
        flashRoutine = null;
    }

    private void HideFlashImmediate()
    {
        if (flashText == null) return;

        Color c = flashText.color;
        c.a = 0f;
        flashText.color = c;
        flashText.text = "";
    }
}
