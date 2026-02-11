using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class RayLayerToggle : MonoBehaviour
{
    [Header("Ray")]
    public XRRayInteractor ray;

    [Header("Interaction Layer Masks")]
    public InteractionLayerMask nodesMask;
    public InteractionLayerMask connectionsMask;
    public InteractionLayerMask bothMask;

    [Header("Physics Raycast Masks")]
    public LayerMask nodesRaycastMask;
    public LayerMask connectionsRaycastMask;
    public LayerMask bothRaycastMask;

    [Header("UI")]
    public InteractionLayerMask uiMask;
    public LayerMask uiRaycastMask;

    [Header("Input")]
    public InputActionReference toggleRayModeAction; // drag your ToggleRayMode here

    [Header("HUD")]
    public SelectionModeUI modeUI; // drag Toggle Switch HUD here (optional)

    public enum Mode { Both, NodesOnly, ConnectionsOnly, UIOnly }
    public Mode startMode = Mode.Both;

    private Mode mode;
    private Mode lastWorldMode;   // remembers Both/Nodes/Connections before going UI-only

    void Awake()
    {
        if (!ray) ray = GetComponentInChildren<XRRayInteractor>();

        // start in a world mode, not UI
        mode = startMode == Mode.UIOnly ? Mode.Both : startMode;
        lastWorldMode = mode;

        ApplyMode(showFlash: false);
    }

    void OnEnable()
    {
        if (toggleRayModeAction?.action == null) return;
        toggleRayModeAction.action.performed += OnTogglePerformed;
        toggleRayModeAction.action.Enable();
    }

    void OnDisable()
    {
        if (toggleRayModeAction?.action == null) return;
        toggleRayModeAction.action.performed -= OnTogglePerformed;
        toggleRayModeAction.action.Disable();
    }

    private void OnTogglePerformed(InputAction.CallbackContext ctx)
    {
        // Don’t allow cycling modes while UI-only (keeps it “modal”)
        if (mode == Mode.UIOnly) return;

        ToggleMode();
    }

    public void ToggleMode()
    {
        mode = mode switch
        {
            Mode.Both => Mode.NodesOnly,
            Mode.NodesOnly => Mode.ConnectionsOnly,
            _ => Mode.Both
        };

        lastWorldMode = mode; // remember last world mode
        ApplyMode(showFlash: true);
        Debug.Log($"Ray Mode: {mode}");
    }

    /// <summary>
    /// Call this when your panel opens. Ray can only hit UI.
    /// </summary>
    public void EnterUIMode(bool showFlash = false)
    {
        if (mode != Mode.UIOnly)
            lastWorldMode = mode; // save current world mode before switching

        mode = Mode.UIOnly;
        ApplyMode(showFlash);
    }

    /// <summary>
    /// Call this when your panel closes. Restores previous world mode.
    /// </summary>
    public void ExitUIMode(bool showFlash = false)
    {
        mode = lastWorldMode;
        ApplyMode(showFlash);
    }

    private void ApplyMode(bool showFlash)
    {
        if (!ray) return;

        switch (mode)
        {
            case Mode.NodesOnly:
                ray.interactionLayers = nodesMask;
                ray.raycastMask = nodesRaycastMask;
                modeUI?.SetMode(SelectionModeUI.SelectMode.Nodes, showFlash);
                break;

            case Mode.ConnectionsOnly:
                ray.interactionLayers = connectionsMask;
                ray.raycastMask = connectionsRaycastMask;
                modeUI?.SetMode(SelectionModeUI.SelectMode.Connections, showFlash);
                break;

            case Mode.UIOnly:
                ray.interactionLayers = uiMask;
                ray.raycastMask = uiRaycastMask;
                // Optional: you can add a UI state to your HUD if you want
                // modeUI?.SetMode(SelectionModeUI.SelectMode.UI, showFlash);
                break;

            default: // Both
                ray.interactionLayers = bothMask;
                ray.raycastMask = bothRaycastMask;
                modeUI?.SetMode(SelectionModeUI.SelectMode.Both, showFlash);
                break;
        }
    }

    void Update()
    {
        if (!ray) return;

        // Helpful debug: 3D hits (won’t show UI hits)
        if (ray.TryGetCurrent3DRaycastHit(out RaycastHit hit))
        {
            Debug.Log($"Ray hit: {hit.collider.name} (layer {LayerMask.LayerToName(hit.collider.gameObject.layer)})");
        }
    }
}
