using UnityEngine;
using UnityEngine.UI;
using System.Collections;
public class HumanityAnimator : MonoBehaviour
{
    public Slider alignmentSlider;
    public Image sliderFill;
    //public Gradient alignmentGradient;

    private Color fillColor;

    public float animationDuration = 2f;

    private void OnEnable()
    {
        GameEvents.Humanity.AddListener(SetHumanity);
        fillColor = sliderFill.color;
    }

    private void OnDisable()
    {
        GameEvents.Humanity.RemoveListener(SetHumanity);
    }
    private void Update()
    {
        //sliderFill.color = alignmentGradient.Evaluate(alignmentSlider.normalizedValue);
    }

    public void SetHumanity(int humanity)
    {
        StopAllCoroutines();
        StartCoroutine(AlignmentAnimation(humanity));
    }
    IEnumerator AlignmentAnimation(int humanity)
    {
        float time = 0;

        float startValue = alignmentSlider.value;
        float targetValue = startValue + humanity;

        //targetValue = Mathf.Clamp01(targetValue);

        sliderFill.color = Color.white;

        while (time < animationDuration)
        {
            float t = time / animationDuration;
            t = Mathf.SmoothStep(0, 1, t);
            alignmentSlider.value = Mathf.Lerp(startValue, targetValue, t);
            time += Time.deltaTime;
            yield return null;
        }
        alignmentSlider.value = targetValue;
        sliderFill.color = fillColor;
        //sliderFill.color = alignmentGradient.Evaluate(alignmentSlider.normalizedValue);
    }
}
