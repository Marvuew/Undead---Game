using TMPro;
using UnityEngine;
using System.Collections;

public class ClockUI : MonoBehaviour
{
    [SerializeField] private Transform clockHourHandTransform;
    [SerializeField] private Transform clockMinuteHandTransform;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private TextMeshProUGUI dayText;

    public float HandAnimationDuration = 1.5f;

    // This keeps track of the "current" time the UI is showing
    private float _currentHour;
    private Coroutine _animationCoroutine;

    private void OnEnable()
    {
        // Listeners remain the same, but we ensure the logic handles floats
        GameEvents.OnTimeChanged.AddListener(AdvanceTime);
        GameEvents.OnRealtimeClockChanged.AddListener(UpdateRealtimeClock);
    }

    private void OnDisable()
    {
        GameEvents.OnTimeChanged.RemoveListener(AdvanceTime);
        GameEvents.OnRealtimeClockChanged.RemoveListener(UpdateRealtimeClock);
    }

    // Use this for "Skips" (e.g., sleeping, fast travel)
    public void AdvanceTime(float targetHour)
    {
        if (_animationCoroutine != null) StopCoroutine(_animationCoroutine);
        _animationCoroutine = StartCoroutine(AnimateClock(targetHour));
    }

    // Use this for "Tick-by-Tick" or "Continuous" flow
    public void UpdateRealtimeClock(float hour)
    {
        // If an animation is running, we might want to let it finish 
        // or snap to the new real time. Usually, snapping is safer for real-time.
        if (TimeManager.Instance.ClockIsAnimating) return;

        UpdateClockUI(hour);
        _currentHour = hour;
    }

    private void UpdateClockUI(float hour)
    {
        // 12-hour clock rotations (Hour hand does 2 full circles in 24h)
        float hourRotation = (hour % 12f) * (360f / 12f);
        // Minute hand (60 minutes = 360 degrees)
        float minuteRotation = (hour % 1f) * 360f;

        // Using negative for clockwise rotation in Unity
        clockHourHandTransform.localEulerAngles = new Vector3(0, 0, -hourRotation);
        clockMinuteHandTransform.localEulerAngles = new Vector3(0, 0, -minuteRotation);

        // Text Formatting
        int h = Mathf.FloorToInt(hour) % 24;
        int m = Mathf.FloorToInt((hour % 1f) * 60f);
        timeText.text = $"{h:00}:{m:00}";
        dayText.text = $"Day: {TimeManager.Instance.CurrentDay}";
    }

    IEnumerator AnimateClock(float targetHour)
    {
        TimeManager.Instance.ClockIsAnimating = true;
        float elapsed = 0;
        float startHour = _currentHour;

        // Handle day-wrapping (e.g., moving from 23:00 to 01:00)
        // If target is less than start, we assume it's the next day
        float totalHourDistance = targetHour - startHour;
        if (totalHourDistance < 0) totalHourDistance += 24f;

        while (elapsed < HandAnimationDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsed / HandAnimationDuration);

            float frameHour = startHour + (totalHourDistance * t);
            UpdateClockUI(frameHour % 24f);

            yield return null;
        }

        _currentHour = targetHour % 24f;
        UpdateClockUI(_currentHour);
        TimeManager.Instance.ClockIsAnimating = false;
        _animationCoroutine = null;
    }
}