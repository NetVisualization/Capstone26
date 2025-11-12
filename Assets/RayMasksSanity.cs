using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(XRRayInteractor))]
public class RayMasksSanity : MonoBehaviour
{
    public XRRayInteractor ray;
    public InputActionReference toggleAction; // same action you use to toggle

    void Awake() { if (!ray) ray = GetComponent<XRRayInteractor>(); }

    void OnEnable()
    {
        if (toggleAction?.action != null)
        {
            toggleAction.action.performed += _ => Dump();
            if (!toggleAction.action.enabled) toggleAction.action.Enable();
        }
        Dump();
    }
    void OnDisable() { if (toggleAction?.action != null) toggleAction.action.performed -= _ => Dump(); }

    void Dump()
    {
        string maskNames = MaskToNames(ray.raycastMask);
        string xriNames = XriMaskToNames(ray.interactionLayers);
        Debug.Log($"[RAY] PhysicsMask={ray.raycastMask.value} ({maskNames})  XRI={ray.interactionLayers.value} ({xriNames})");
    }

    string MaskToNames(LayerMask m)
    {
        System.Text.StringBuilder sb = new();
        for (int i = 0; i < 32; i++) if ((m.value & (1 << i)) != 0) sb.Append(LayerMask.LayerToName(i)).Append(" ");
        return sb.ToString();
    }
    string XriMaskToNames(InteractionLayerMask m)
    {
        // XRI stores custom names; we can at least show the raw bits.
        return m.value.ToString();
    }

    void Update()
    {
        var ray = GetComponent<UnityEngine.XR.Interaction.Toolkit.XRRayInteractor>();
        if (!ray) return;
        if (ray.TryGetCurrent3DRaycastHit(out RaycastHit hit))
        {
            Debug.Log($"NODES MODE HIT: {hit.collider.name}  layer={LayerMask.LayerToName(hit.collider.gameObject.layer)}  trigger={hit.collider.isTrigger}");
        }
        else
        {
            Debug.Log("NODES MODE: no physics hit");
        }
    }

}
