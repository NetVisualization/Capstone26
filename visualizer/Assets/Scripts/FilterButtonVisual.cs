using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FilterButtonVisual : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Button button;
    [SerializeField] private Image background;     // button Image
    [SerializeField] private TMP_Text label;       // TMP text

    [Header("System")]
    [SerializeField] private FilterSystem filterSystem;   // Calls SetFilterState()

    [Header("Filter Key")]
    [SerializeField] private string filterKey;

    [Header("Active Look")]
    [SerializeField] private float activeBgMultiplier = 1.35f; // brighten when active
    [SerializeField] private bool boldWhenActive = true;

    [Header("Inactive Look")]
    [Tooltip("How grey the background gets when inactive (0 = black, 1 = original color).")]
    [Range(0f, 1f)]
    [SerializeField] private float inactiveBgGreyAmount = 0.25f;

    [Tooltip("How grey the text gets when inactive (0 = black, 1 = original color).")]
    [Range(0f, 1f)]
    [SerializeField] private float inactiveTextGreyAmount = 0.35f;
    [Header("Startup")]
    [SerializeField] private bool startActive = true;

    public bool IsActive { get; private set; }

    // cached “original/inactive” visuals
    private Color _originalBg;
    private Color _originalText;
    private FontStyles _originalFontStyle;

    private void Reset()
    {
        button = GetComponent<Button>();
        background = GetComponent<Image>();
        label = GetComponentInChildren<TMP_Text>();
    }

    private void Awake()
{
    if (!button) button = GetComponent<Button>();
    if (!background) background = GetComponent<Image>();
    if (!label) label = GetComponentInChildren<TMP_Text>();
    if (!filterSystem) filterSystem = FindFirstObjectByType<FilterSystem>();

    // Cache original look
    if (background) _originalBg = background.color;
    if (label)
    {
        _originalText = label.color;
        _originalFontStyle = label.fontStyle;
    }

    if (button)
        button.onClick.AddListener(Toggle);

    IsActive = startActive;
    ApplyVisual();

    if (filterSystem != null && !string.IsNullOrWhiteSpace(filterKey))
    {
        // hide = !IsActive
        filterSystem.SetFilterState(filterKey, !IsActive);
    }
}

    private void OnDestroy()
    {
        if (button)
            button.onClick.RemoveListener(Toggle);
    }

    private void Toggle()
    {
        IsActive = !IsActive;
        ApplyVisual();

        // FilterSystem expects: filterOn == true means "hide"
        // Our UX expects: IsActive == true means "visible/selected"
        // So send: hide = !IsActive
        if (filterSystem != null && !string.IsNullOrWhiteSpace(filterKey))
        {
            filterSystem.SetFilterState(filterKey, !IsActive);
        }
        else
        {
            Debug.LogWarning($"[FilterButtonVisual] Missing FilterSystem or filterKey on {name}");
        }
    }

    private void ApplyVisual()
    {
        if (background)
        {
            if (IsActive)
            {
                // Brighten original color
                background.color = MultiplyRGB(_originalBg, activeBgMultiplier);
            }
            else
            {
                // Greyed version of original
                background.color = LerpToGrey(_originalBg, inactiveBgGreyAmount);
            }
        }

        if (label)
        {
            if (IsActive)
            {
                label.color = _originalText;
                label.fontStyle = boldWhenActive
                    ? (_originalFontStyle | FontStyles.Bold)
                    : _originalFontStyle;
            }
            else
            {
                label.color = LerpToGrey(_originalText, inactiveTextGreyAmount);
                label.fontStyle = _originalFontStyle; // remove bold when inactive
            }
        }
    }

    // --- Helpers ---

    private static Color MultiplyRGB(Color c, float m)
    {
        return new Color(c.r * m, c.g * m, c.b * m, c.a);
    }

    // amount: 0 = full grey, 1 = original color
    private static Color LerpToGrey(Color c, float amount)
    {
        float grey = (c.r + c.g + c.b) / 3f;
        Color g = new Color(grey, grey, grey, c.a);
        return Color.Lerp(g, c, amount);
    }
}