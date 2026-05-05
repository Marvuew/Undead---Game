using System.Collections;
using UnityEngine;

public class LOGO_Animation : MonoBehaviour
{
    RectTransform rect;
    public float ratio = 100f;
    public float duration = 5f;

    private void Start()
    {
        rect = GetComponent<RectTransform>();
        StartCoroutine(ScaleOverTime());
    }
    public float easeInCirc(float x)
    {
        x = Mathf.Clamp01(x);
        return 1 - Mathf.Sqrt(1 - Mathf.Pow(x, 2));
    }

    public float easeinQuart(float x)
    {
        return x * x * x * x;
    }

    IEnumerator ScaleOverTime()
    {
        Vector2 startTransform = rect.sizeDelta;
        Vector2 endTransform = rect.sizeDelta * ratio;
        float elapsed = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float normalizedTime = elapsed / duration;

            float easedTime = easeinQuart(normalizedTime);

            rect.sizeDelta = Vector2.Lerp(startTransform, endTransform, easedTime);
            yield return null;
        }
        rect.sizeDelta = endTransform;
    }
}
