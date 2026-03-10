using TMPro;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEditor.Experimental.GraphView;
using Unity.Mathematics;

public class ClockUI : MonoBehaviour
{
    private Transform clockHourHandTransform;
    private Transform clockMinuteHandTransform;
    private TextMeshProUGUI timeText;
    private float day;

    private const float REAL_SECONDS_PER_INGAME_DAY = 60f;

    public float HandAnimationDuration = 2f;

    private void Awake()
    {
        clockHourHandTransform = transform.Find("hourHand");
        clockMinuteHandTransform = transform.Find("minuteHand");
        timeText = transform.Find("TimeText").GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        GameEvents.OnTimeChanged.AddListener(AdvanceTime);
    }

    private void OnDisable()
    {
        GameEvents.OnTimeChanged.RemoveListener(AdvanceTime);
    }


    /*private void Update()
    {
        day += Time.deltaTime / REAL_SECONDS_PER_INGAME_DAY;

        float dayNormalized = day % 1f;

        float rotationDegreesPerDay = 360f;

        clockHourHandTransform.eulerAngles = new Vector3(0, 0, -dayNormalized * rotationDegreesPerDay);

        float hoursPerDay = 24f;
        clockMinuteHandTransform.eulerAngles = new Vector3(0, 0, - dayNormalized * rotationDegreesPerDay * hoursPerDay);

        string hourString = Mathf.Floor(dayNormalized * 24f).ToString("00");

        float minutesPerHour = 60f;
        string minuteString = Mathf.Floor(((dayNormalized * hoursPerDay) % 1f) * minutesPerHour).ToString("00");

        timeText.text = hourString + ":" + minuteString;
    }*/
    public void AdvanceTime(int tick)
    {
        if (!TimeManager.Instance.ClockIsAnimating)
        {
            StopAllCoroutines();
            StartCoroutine(AnimateClock(tick));
        }
        else
        {
            Debug.Log("Clock is still Animating");
        }
    }

    IEnumerator AnimateClock(int tick)
    {
        TimeManager.Instance.ClockIsAnimating = true;
        float time = 0;

        // 1. Get current positions
        float startHourZ = clockHourHandTransform.localEulerAngles.z;
        float startMinuteZ = clockMinuteHandTransform.localEulerAngles.z;

        // 2. ABSOLUTE HOUR CALCULATION
        // (tick / 24) * 720 degrees. 
        // We use Mathf.MoveTowardsAngle logic or just ensure the target 
        // is the "closest" version of that angle to avoid the 360-degree flip.
        float dayNormalized = (float)tick / 24f;
        float targetHourZ = -dayNormalized * 720f;

        // 3. RELATIVE MINUTE CALCULATION
        // The minute hand always just adds -360 to its current position
        float targetMinuteZ = startMinuteZ - 360f;

        // 4. THE FIX FOR THE HOUR SPIN
        // If the hour hand is at -690 and the new target is 0, Lerp will spin it backwards.
        // We use Repeat to keep the start and end values within a predictable range.
        startHourZ = Mathf.MoveTowardsAngle(startHourZ, startHourZ, 0);

        while (time < HandAnimationDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / HandAnimationDuration);
            float smoothT = Mathf.SmoothStep(0, 1, t);

            // We use LerpAngle for the Hour to prevent the "long way around" spin
            float currentHourZ = Mathf.LerpAngle(startHourZ, targetHourZ, smoothT);

            // We use standard Lerp for the Minute to FORCE the full 360 spin
            float currentMinuteZ = Mathf.Lerp(startMinuteZ, targetMinuteZ, smoothT);

            clockHourHandTransform.localEulerAngles = new Vector3(0, 0, currentHourZ);
            clockMinuteHandTransform.localEulerAngles = new Vector3(0, 0, currentMinuteZ);

            // TextAnimation

            int animatedMinutes = Mathf.RoundToInt(Mathf.RoundToInt(Mathf.Lerp(0, 60, smoothT)));

            string displayMinutes = (animatedMinutes % 60).ToString("00");

            int displayHour = (tick - 1 + 24) % 24;

            timeText.text = displayHour.ToString("00") + ":" + displayMinutes;

            yield return null;
        }

        // Final Snap
        clockHourHandTransform.localEulerAngles = new Vector3(0, 0, targetHourZ);
        clockMinuteHandTransform.localEulerAngles = new Vector3(0, 0, targetMinuteZ);
        timeText.text = tick.ToString("00") + ":" + "00";
        TimeManager.Instance.ClockIsAnimating = false;
    }
}
