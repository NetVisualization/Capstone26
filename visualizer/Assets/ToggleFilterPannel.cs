using UnityEngine;
using UnityEngine.InputSystem;

public class ToggleFilterPannel : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject panelRoot;   // Recommend: FilterPanelWorldRoot

    [Header("Input (Input System)")]
    [SerializeField] private InputActionReference toggleAction;

    [Header("Behavior")]
    [SerializeField] private bool startHidden = true;
    [SerializeField] private bool toggleOnPress = true; // true = press toggles once. false = hold to show.

    [Header("Panel Placement")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float distanceFromCamera = 2.0f;
    [SerializeField] private Vector3 positionOffset = Vector3.zero;
    [SerializeField] private bool lockVerticalRotation = true;

    [Header("Ray Mode Control")]
    [SerializeField] private RayLayerToggle rayToggle;   // drag your RayLayerToggle here
    [SerializeField] private bool lockRayToUIWhileOpen = true;

    private void Awake()
    {
        if (panelRoot != null && startHidden)
        {
            panelRoot.SetActive(false);
        }
        else if (panelRoot != null && panelRoot.activeSelf)
        {
            // If starting visible, ensure UI mode is applied
            EnterUIModeIfNeeded();
        }
    }

    private void OnEnable()
    {
        if (toggleAction == null) return;

        toggleAction.action.Enable();

        if (toggleOnPress)
            toggleAction.action.performed += OnTogglePerformed;
    }

    private void OnDisable()
    {
        if (toggleAction == null) return;

        if (toggleOnPress)
            toggleAction.action.performed -= OnTogglePerformed;

        toggleAction.action.Disable();
    }

    private void Update()
    {
        if (toggleAction == null || panelRoot == null) return;

        // Hold-to-show mode
        if (!toggleOnPress)
        {
            bool held = toggleAction.action.IsPressed();

            if (held && !panelRoot.activeSelf)
            {
                ShowPanel();
            }
            else if (!held && panelRoot.activeSelf)
            {
                HidePanel();
            }
        }
    }

    private void OnTogglePerformed(InputAction.CallbackContext ctx)
    {
        if (panelRoot == null) return;

        bool show = !panelRoot.activeSelf;

        if (show) ShowPanel();
        else HidePanel();
    }

    private void ShowPanel()
    {
        panelRoot.SetActive(true);

        // IMPORTANT: switch ray to UI-only so buttons can be clicked
        EnterUIModeIfNeeded();

        if (cameraTransform == null) return;

        Vector3 forward = cameraTransform.forward;

        if (lockVerticalRotation)
            forward.y = 0f;

        if (forward.sqrMagnitude < 0.0001f)
            forward = cameraTransform.forward;

        panelRoot.transform.position =
            cameraTransform.position +
            forward.normalized * distanceFromCamera +
            positionOffset;

        Vector3 lookDir = panelRoot.transform.position - cameraTransform.position;

        if (lockVerticalRotation)
            lookDir.y = 0f;

        if (lookDir.sqrMagnitude > 0.0001f)
            panelRoot.transform.rotation = Quaternion.LookRotation(lookDir.normalized, Vector3.up);
    }

    private void HidePanel()
    {
        panelRoot.SetActive(false);

        // Restore node/connection clicking
        ExitUIModeIfNeeded();
    }

    private void EnterUIModeIfNeeded()
    {
        if (!lockRayToUIWhileOpen) return;
        if (rayToggle == null) return;

        rayToggle.EnterUIMode(showFlash: false);
    }

    private void ExitUIModeIfNeeded()
    {
        if (!lockRayToUIWhileOpen) return;
        if (rayToggle == null) return;

        rayToggle.ExitUIMode(showFlash: false);
    }
}
