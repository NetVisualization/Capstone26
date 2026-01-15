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

    [Header("Input")]
    public InputActionReference toggleRayModeAction; // <-- drag your ToggleRayMode here

    public enum Mode { Both, NodesOnly, ConnectionsOnly }
    public Mode startMode = Mode.Both;

    private Mode mode;

    void Awake()
    {
        if (!ray) ray = GetComponentInChildren<XRRayInteractor>();
        mode = startMode;
        ApplyMode();
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

        ApplyMode();
        Debug.Log($"Ray Mode: {mode}");
    }

    private void ApplyMode()
    {
        if (!ray) return;

        ray.interactionLayers = mode switch
        {
            Mode.NodesOnly => nodesMask,
            Mode.ConnectionsOnly => connectionsMask,
            _ => bothMask
        };
    }
}
