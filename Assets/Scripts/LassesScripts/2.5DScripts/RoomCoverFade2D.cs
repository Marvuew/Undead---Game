using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class RoomCoverFade2D : MonoBehaviour
{
    public float fadeDuration = 0.5f;

    private SpriteRenderer sr;
    private Coroutine fadeRoutine;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public void SetBlackInstant()
    {
        Color c = sr.color;
        c.a = 1f;
        sr.color = c;
    }

    public void FadeOut()
    {
        StartFade(0f);
    }

    public void FadeIn()
    {
        StartFade(1f);
    }

    private void StartFade(float targetAlpha)
    {
        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadeRoutine(targetAlpha));
    }

    private IEnumerator FadeRoutine(float targetAlpha)
    {
        float startAlpha = sr.color.a;
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            float t = time / fadeDuration;

            Color c = sr.color;
            c.a = Mathf.Lerp(startAlpha, targetAlpha, t);
            sr.color = c;

            yield return null;
        }

        Color final = sr.color;
        final.a = targetAlpha;
        sr.color = final;
    }
}