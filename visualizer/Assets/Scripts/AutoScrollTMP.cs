using TMPro;
using UnityEngine;

public class AutoScrollHorizontalTMP : MonoBehaviour
{
    [SerializeField] private RectTransform textRect;
    [SerializeField] private RectTransform viewportRect;
    [SerializeField] private TextMeshProUGUI tmpText;

    [SerializeField] private float scrollSpeed = 25f;
    [SerializeField] private float startX = 0f;
    [SerializeField] private float endPadding = 20f;
    [SerializeField] private float pauseAtStart = 1.0f;

    private float pauseTimer;

    private void Awake()
    {
        if (tmpText == null) tmpText = GetComponent<TextMeshProUGUI>();
        if (textRect == null) textRect = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        ResetToStart();
    }

    public void ResetToStart()
    {
        if (tmpText == null || textRect == null || viewportRect == null) return;

        tmpText.ForceMeshUpdate();
        float preferredWidth = tmpText.preferredWidth;
        textRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, preferredWidth);

        textRect.anchoredPosition = new Vector2(startX, textRect.anchoredPosition.y);
        pauseTimer = pauseAtStart;
    }

    private void Update()
    {
        if (tmpText == null || textRect == null || viewportRect == null) return;

        tmpText.ForceMeshUpdate();

        float textWidth = tmpText.preferredWidth;
        float viewportWidth = viewportRect.rect.width;

        textRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, textWidth);

        if (textWidth <= viewportWidth)
        {
            textRect.anchoredPosition = new Vector2(startX, textRect.anchoredPosition.y);
            return;
        }

        if (pauseTimer > 0f)
        {
            pauseTimer -= Time.deltaTime;
            return;
        }

        Vector2 pos = textRect.anchoredPosition;
        pos.x -= scrollSpeed * Time.deltaTime;
        textRect.anchoredPosition = pos;

        float resetPoint = -(textWidth - viewportWidth + endPadding);

        if (pos.x <= resetPoint)
        {
            textRect.anchoredPosition = new Vector2(startX, pos.y);
            pauseTimer = pauseAtStart;
        }
    }
}