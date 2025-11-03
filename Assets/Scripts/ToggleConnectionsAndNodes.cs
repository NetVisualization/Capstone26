using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class ToggleConnectionsAndNodes : MonoBehaviour
{
    public XRRayInteractor ray;

    [Header("Physics LayerMasks (Inspector)")]
    public LayerMask nodesPhysicsMask;       // set to "Nodes"
    public LayerMask connectionsPhysicsMask; // set to "Connections"
    public LayerMask uiPhysicsMask;          // optional: "UI" if you want UI hits

    [Header("XRI Interaction Layers (names)")]
    public string[] nodeInteractionLayers = { "NodeInteractable" };
    public string[] connectionInteractionLayers = { "ConnectionInteractable" };

    [Header("Input")]
    public InputActionReference toggleAction; // bind to a controller button

    enum Mode { Nodes, Connections, Both }
    [SerializeField] Mode mode = Mode.Nodes;

    void OnEnable()
    {
        if (toggleAction != null)
        {
            toggleAction.action.performed += OnToggle;
            toggleAction.action.Enable();
        }
        Apply();
    }

    void OnDisable()
    {
        if (toggleAction != null)
        {
            toggleAction.action.performed -= OnToggle;
            toggleAction.action.Disable();
        }
    }

    void OnToggle(InputAction.CallbackContext _)
    {
        mode = (Mode)(((int)mode + 1) % 3);
        Apply();
    }

    void Apply()
    {
        // ---- Physics raycast mask (what colliders the ray can hit) ----
        LayerMask physicsMask = uiPhysicsMask; // keep UI always targetable if you want
        switch (mode)
        {
            case Mode.Nodes: physicsMask |= nodesPhysicsMask; break;
            case Mode.Connections: physicsMask |= connectionsPhysicsMask; break;
            case Mode.Both: physicsMask |= nodesPhysicsMask | connectionsPhysicsMask; break;
        }
        ray.raycastMask = physicsMask;

        // ---- XRI interaction layers (which interactables are valid) ----
        InteractionLayerMask xriMask = default;
        switch (mode)
        {
            case Mode.Nodes:
                xriMask = InteractionLayerMask.GetMask(nodeInteractionLayers);
                break;
            case Mode.Connections:
                xriMask = InteractionLayerMask.GetMask(connectionInteractionLayers);
                break;
            case Mode.Both:
                // combine both sets
                xriMask = InteractionLayerMask.GetMask(nodeInteractionLayers)
                         | InteractionLayerMask.GetMask(connectionInteractionLayers);
                break;
        }
        ray.interactionLayers = xriMask;

        // Optional: make triggers invisible to the ray if your connections use trigger colliders
        ray.raycastTriggerInteraction = QueryTriggerInteraction.Ignore;
    }
}
