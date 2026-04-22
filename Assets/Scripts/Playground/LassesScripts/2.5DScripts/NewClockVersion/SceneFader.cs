using System.Collections;
using UnityEngine;

public class SceneFadeInOnLoad : MonoBehaviour
{
    [SerializeField] private CanvasGroup fadeGroup;
    [SerializeField] private float fadeInSeconds = 1.5f;

    private void Awake()
    {
        if (fadeGroup != null)
        {
            fadeGroup.alpha = 1f;
            fadeGroup.blocksRaycasts = true;
            fadeGroup.interactable = false;
        }
    }

    private void Start()
    {
        if (fadeGroup != null)
            StartCoroutine(FadeInRoutine());
    }

    private IEnumerator FadeInRoutine()
    {
        float duration = Mathf.Max(0.01f, fadeInSeconds);
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / duration);
            fadeGroup.alpha = Mathf.Lerp(1f, 0f, p);
            yield return null;
        }

        fadeGroup.alpha = 0f;
        fadeGroup.blocksRaycasts = false;
        fadeGroup.interactable = false;
    }
}