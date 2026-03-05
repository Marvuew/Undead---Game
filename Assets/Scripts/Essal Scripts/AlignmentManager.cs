using UnityEngine;
using UnityEngine.UI;
using System.Collections;
public class AlignmentManager : MonoBehaviour
{
    public Slider alignmentSlider;
    public Image sliderFill;
    public Gradient alignmentGradient;

    public float animationDuration = 2f;

    private void Awake()
    {
        UpdateColor();
    }

    public void SetAlignment(float value)
    {
        StopAllCoroutines();
        StartCoroutine(AlignmentAnimation(value));
    }
    IEnumerator AlignmentAnimation(float value)
    {
        float time = 0;

        float startValue = alignmentSlider.normalizedValue;
        float targetValue = startValue + value;

        targetValue = Mathf.Clamp01(targetValue);

        while (time < animationDuration)
        {
            float t = time / animationDuration;
            t = Mathf.SmoothStep(0, 1, t);
            alignmentSlider.normalizedValue = Mathf.Lerp(startValue, targetValue, t);
            time += Time.deltaTime;
            yield return null;
        }

        alignmentSlider.normalizedValue = targetValue;
        UpdateColor();
    }

    void UpdateColor()
    {
        sliderFill.color = alignmentGradient.Evaluate(alignmentSlider.normalizedValue);
    }
}
