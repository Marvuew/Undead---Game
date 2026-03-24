using System.Collections;
using TMPro;
using UnityEngine;

public class ClueUpdatePopup : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private CanvasGroup group;
    [SerializeField] private TMP_Text popupText;

    [Header("Timing")]
    [SerializeField] private float showSeconds = 1.0f;
    [SerializeField] private float fadeOutSeconds = 0.6f;

    private Coroutine routine;

    private void Awake()
    {
        if (group == null)
            Debug.LogWarning("ClueUpdatePopup: Assign CanvasGroup (group).");

        if (popupText == null)
            Debug.LogWarning("ClueUpdatePopup: Assign popupText.");

        SetVisible(false);
    }

    public void Show(string message)
    {
        if (group == null || popupText == null)
        {
            Debug.LogWarning("ClueUpdatePopup: Missing CanvasGroup or popupText.");
            return;
        }

        popupText.text = message;

        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(ShowRoutine());
    }

    private IEnumerator ShowRoutine()
    {
        SetVisible(true);

        yield return new WaitForSecondsRealtime(showSeconds);

        float t = 0f;
        float startAlpha = group.alpha;

        while (t < fadeOutSeconds)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / fadeOutSeconds);
            group.alpha = Mathf.Lerp(startAlpha, 0f, p);
            yield return null;
        }

        SetVisible(false);
        routine = null;
    }

    private void SetVisible(bool visible)
    {
        if (group == null) return;

        group.alpha = visible ? 1f : 0f;
        group.blocksRaycasts = visible;
        group.interactable = visible;
    }
}