using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class UIButtonHoverEffect : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler,
    ISelectHandler, IDeselectHandler
{
    [Header("Targets (assign if auto-find fails)")]
    [SerializeField] private Image targetImage;
    [SerializeField] private TMP_Text targetText;

    [Header("Scale")]
    [SerializeField] private float hoverScale = 1.08f;

    [Header("Brightness")]
    [Range(0f, 1f)]
    [SerializeField] private float brightenToWhite = 0.35f;

    [Header("Animation speed")]
    [SerializeField] private float speed = 12f;

    private Vector3 baseScale;
    private Vector3 wantedScale;

    private Color baseImgColor;
    private Color hoverImgColor;

    private Color baseTextColor;
    private Color hoverTextColor;

    private bool highlighted;

    private void Awake()
    {
        // Auto-find
        if (targetImage == null) targetImage = GetComponent<Image>();
        if (targetText == null) targetText = GetComponentInChildren<TMP_Text>(true);

        baseScale = transform.localScale;
        wantedScale = baseScale;

        if (targetImage != null)
        {
            baseImgColor = targetImage.color;
            hoverImgColor = Color.Lerp(baseImgColor, Color.white, brightenToWhite);
        }

        if (targetText != null)
        {
            baseTextColor = targetText.color;
            hoverTextColor = Color.Lerp(baseTextColor, Color.white, brightenToWhite);
        }

        ApplyInstant(false);
    }

    private void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, wantedScale, Time.deltaTime * speed);

        if (targetImage != null)
            targetImage.color = Color.Lerp(targetImage.color, highlighted ? hoverImgColor : baseImgColor, Time.deltaTime * speed);

        if (targetText != null)
            targetText.color = Color.Lerp(targetText.color, highlighted ? hoverTextColor : baseTextColor, Time.deltaTime * speed);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        SetHighlight(true, "PointerEnter");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SetHighlight(false, "PointerExit");
    }

    public void OnSelect(BaseEventData eventData)
    {
        SetHighlight(true, "Select");
    }

    public void OnDeselect(BaseEventData eventData)
    {
        SetHighlight(false, "Deselect");
    }

    private void SetHighlight(bool on, string reason)
    {
        highlighted = on;
        wantedScale = on ? baseScale * hoverScale : baseScale;

        Debug.Log($"{name} highlight={on} via {reason}");
    }

    private void ApplyInstant(bool on)
    {
        highlighted = on;
        wantedScale = on ? baseScale * hoverScale : baseScale;

        if (targetImage != null) targetImage.color = on ? hoverImgColor : baseImgColor;
        if (targetText != null) targetText.color = on ? hoverTextColor : baseTextColor;

        transform.localScale = wantedScale;
    }
}