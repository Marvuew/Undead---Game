using System.Collections;
using TMPro;
using UnityEngine;

public class AlignmentPointPopupUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private TMP_Text label;
    [SerializeField] private CanvasGroup group;
    [SerializeField] private RectTransform rect;

    [Header("Motion")]
    [SerializeField] private Vector2 floatUpPixelsPerSecond = new Vector2(0f, 90f);
    [SerializeField] private float lifetimeSeconds = 1.4f;

    [Header("Fade")]
    [SerializeField] private float fadeOutSeconds = 0.6f;

    private void Awake()
    {
        if (label == null) label = GetComponentInChildren<TMP_Text>(true);
        if (group == null) group = GetComponent<CanvasGroup>();
        if (rect == null) rect = transform as RectTransform;

        if (group != null)
        {
            group.alpha = 0f;
            group.blocksRaycasts = false;
            group.interactable = false;
        }
    }

    public void Init(string text, Color color, Vector2 anchoredStartPos)
    {
        if (label == null || group == null || rect == null)
        {
            Destroy(gameObject);
            return;
        }

        label.text = text;
        label.color = color;
        rect.anchoredPosition = anchoredStartPos;

        StopAllCoroutines();
        StartCoroutine(LifeRoutine());
    }

    private IEnumerator LifeRoutine()
    {
        group.alpha = 1f;

        float t = 0f;
        float fadeStartAt = Mathf.Max(0f, lifetimeSeconds - fadeOutSeconds);

        while (t < lifetimeSeconds)
        {
            t += Time.unscaledDeltaTime;

            rect.anchoredPosition += floatUpPixelsPerSecond * Time.unscaledDeltaTime;

            if (t >= fadeStartAt && fadeOutSeconds > 0f)
            {
                float k = Mathf.Clamp01((t - fadeStartAt) / fadeOutSeconds);
                group.alpha = 1f - k;
            }

            yield return null;
        }

        Destroy(gameObject);
    }
}