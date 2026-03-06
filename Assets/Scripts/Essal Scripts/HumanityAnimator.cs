using UnityEngine;
using UnityEngine.UI;
using System.Collections;
public class HumanityAnimator : MonoBehaviour
{
    public Slider alignmentSlider;
    public Image sliderFill;
    public Gradient alignmentGradient;

    public float animationDuration = 2f;

    private void OnEnable()
    {
        Player.OnHumanityChanged += SetAlignment;
    }

    private void OnDisable()
    {
        Player.OnHumanityChanged -= SetAlignment;
    }
    private void Update()
    {
        sliderFill.color = alignmentGradient.Evaluate(alignmentSlider.value);
    }

    public void SetAlignment(float value)
    {
        StopAllCoroutines();
        StartCoroutine(AlignmentAnimation(value));
    }
    IEnumerator AlignmentAnimation(float value)
    {
        float time = 0;

        float startValue = alignmentSlider.value;
        float targetValue = startValue + value;

        //targetValue = Mathf.Clamp01(targetValue);

        while (time < animationDuration)
        {
            float t = time / animationDuration;
            t = Mathf.SmoothStep(0, 1, t);
            alignmentSlider.value = Mathf.Lerp(startValue, targetValue, t);
            time += Time.deltaTime;
            yield return null;
        }

        alignmentSlider.value = targetValue;
    }
}
