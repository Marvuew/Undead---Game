using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class AlignmentSlider : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Slider slider;
    [SerializeField] private RectTransform barArea;
    [SerializeField] private RectTransform leftSide;
    [SerializeField] private RectTransform rightSide;
    [SerializeField] private RectTransform handle;

    [Header("Config")]
    [SerializeField] private float maxAbs = 100f;
    [SerializeField] private float smoothSeconds = 0.25f;

    [Header("Reset")]
    [SerializeField] private bool enableResetShortcut = true;
    [SerializeField] private Key resetKey = Key.Backspace;

    [Header("State (read-only)")]
    [SerializeField] private float currentValue;
    [SerializeField] private float targetValue;

    private Coroutine animRoutine;

    private void Awake()
    {
        if (slider != null)
        {
            slider.interactable = false;
            slider.minValue = -maxAbs;
            slider.maxValue = maxAbs;
        }

        currentValue = targetValue = AlignmentSave.Get();
    }

    private void OnEnable()
    {
        RefreshFromSave();
    }

    private void Update()
    {
        if (!enableResetShortcut) return;
        if (Keyboard.current == null) return;

        bool shift =
            Keyboard.current.leftShiftKey.isPressed ||
            Keyboard.current.rightShiftKey.isPressed;

        if (shift && Keyboard.current[resetKey].wasPressedThisFrame)
        {
            ResetBar();
        }
    }

    public void ResetBar()
    {
        AlignmentSave.ResetToCenter();

        currentValue = targetValue = 0;

        if (animRoutine != null)
            StopCoroutine(animRoutine);

        ApplyVisuals(currentValue);
    }

    public void RefreshFromSave()
    {
        SetTarget(AlignmentSave.Get());
    }

    public void SetTarget(float value)
    {
        targetValue = Mathf.Clamp(value, -maxAbs, maxAbs);

        if (animRoutine != null)
            StopCoroutine(animRoutine);

        animRoutine = StartCoroutine(Animate());
    }

    private IEnumerator Animate()
    {
        float start = currentValue;
        float end = targetValue;

        float t = 0;

        while (t < smoothSeconds)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / smoothSeconds);

            currentValue = Mathf.Lerp(start, end, p);
            ApplyVisuals(currentValue);

            yield return null;
        }

        currentValue = end;
        ApplyVisuals(currentValue);
    }

    private void ApplyVisuals(float value)
    {
        if (slider != null)
            slider.value = value;

        float normalized = Mathf.InverseLerp(-maxAbs, maxAbs, value);

        if (handle != null)
        {
            float width = barArea.rect.width;
            float x = (normalized - 0.5f) * width;
            handle.anchoredPosition = new Vector2(x, handle.anchoredPosition.y);
        }

        if (leftSide != null && rightSide != null)
        {
            float split = normalized;

            leftSide.anchorMax = new Vector2(split, 1);
            rightSide.anchorMin = new Vector2(split, 0);
        }
    }
}