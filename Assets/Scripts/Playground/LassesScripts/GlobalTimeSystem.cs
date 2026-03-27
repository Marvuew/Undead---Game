using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GlobalTimeSystem : MonoBehaviour
{
    public static GlobalTimeSystem Instance;

    [Header("Day Settings")]
    [SerializeField] private int startHour = 8;
    [SerializeField] private int endHour = 16;

    [Header("Passive Clock Speed")]
    [Tooltip("If true, passive time only moves while the player is walking around.")]
    [SerializeField] private bool onlyAdvancePassiveTimeWhilePlayerMoves = true;

    [Tooltip("Real seconds per in-game minute while standing still. Ignored if onlyAdvancePassiveTimeWhilePlayerMoves is true.")]
    [Range(0.05f, 30f)]
    [SerializeField] private float idleSecondsPerMinute = 999f;

    [Tooltip("Real seconds per in-game minute while the player is moving. Lower = faster clock.")]
    [Range(0.05f, 30f)]
    [SerializeField] private float movingSecondsPerMinute = 0.5f;

    [Header("Interaction Fast Forward")]
    [Tooltip("Speed used when fast-forwarding time from interactions.")]
    [Range(0.01f, 5f)]
    [SerializeField] private float fastForwardMinuteSpeed = 0.05f;

    [Header("UI")]
    [SerializeField] private TMP_Text timeText;

    [Header("Normal Minute Flash")]
    [SerializeField] private bool flashOnTimeChange = true;
    [SerializeField] private float flashDuration = 0.12f;
    [SerializeField] private float flashScaleMultiplier = 1.12f;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color flashColor = Color.yellow;

    [Header("New Hour Flash")]
    [SerializeField] private bool specialFlashOnNewHour = true;
    [SerializeField] private float newHourFlashDuration = 0.06f;
    [SerializeField] private float newHourFlashScaleMultiplier = 1.2f;
    [SerializeField] private Color newHourFlashColor = Color.white;
    [SerializeField] private int newHourFlashBlinks = 3;

    [Header("New Hour Pause")]
    [Tooltip("When a new hour is reached, pause all clock progression for this many real seconds.")]
    [SerializeField] private bool pauseOnNewHour = true;
    [SerializeField] private float newHourPauseSeconds = 5f;

    [Header("Time Up Sequence")]
    [SerializeField] private bool triggerEndingWhenTimeIsUp = true;

    [TextArea]
    [SerializeField] private string timeUpMessage = "Time to figure out who did this.";

    [SerializeField] private TMP_Text timeUpMessageText;
    [SerializeField] private CanvasGroup timeUpMessageGroup;
    [SerializeField] private float timeUpMessageSeconds = 2.5f;

    [SerializeField] private CanvasGroup fadeToBlackGroup;
    [SerializeField] private float fadeToBlackSeconds = 1.5f;

    [SerializeField] private string loadingSceneName = "LoadingScene";
    [SerializeField] private float loadingSceneSeconds = 5f;
    [SerializeField] private string culpritPickingSceneName = "CulpritPickingScene";

    private int minutes;
    private float timer;

    private Coroutine advanceRoutine;
    private Coroutine flashRoutine;
    private Coroutine newHourPauseRoutine;
    private Coroutine endingRoutine;

    private Vector3 originalScale = Vector3.one;

    private int StartMinutes => startHour * 60;
    private int EndMinutes => endHour * 60;

    private bool isPausedForNewHour;
    private bool isEndingSequenceRunning;
    private int protectedInteractionCount;
    private bool playerIsMoving;

    public bool IsAdvancingTime => advanceRoutine != null;
    public bool IsPausedForHourChange => isPausedForNewHour;
    public bool IsTimeUp => minutes >= EndMinutes;
    public bool IsEndingSequenceRunning => isEndingSequenceRunning;
    public bool IsInteractionProtected => protectedInteractionCount > 0;
    public bool IsPlayerMoving => playerIsMoving;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
       

        minutes = StartMinutes;

        if (timeText != null)
        {
            originalScale = timeText.rectTransform.localScale;
            timeText.color = normalColor;
        }

        if (timeUpMessageGroup != null)
        {
            timeUpMessageGroup.alpha = 0f;
            timeUpMessageGroup.blocksRaycasts = false;
            timeUpMessageGroup.interactable = false;
        }

        if (fadeToBlackGroup != null)
        {
            fadeToBlackGroup.alpha = 0f;
            fadeToBlackGroup.blocksRaycasts = false;
            fadeToBlackGroup.interactable = false;
        }

        UpdateUI(triggerFlash: false, isNewHour: false);
    }

    private void Update()
    {
        if (isEndingSequenceRunning)
            return;

        if (isPausedForNewHour)
            return;

        if (advanceRoutine != null)
            return;

        if (minutes >= EndMinutes)
        {
            TryStartTimeUpSequence();
            return;
        }

        if (!ShouldAdvancePassiveTime())
            return;

        float secondsPerMinute = GetCurrentPassiveSecondsPerMinute();
        timer += Time.deltaTime;

        if (timer >= secondsPerMinute)
        {
            timer = 0f;
            AddMinuteAndRefresh();
        }
    }

    public void SetPlayerMoving(bool moving)
    {
        playerIsMoving = moving;
        timer = 0f;
    }

    public void BeginProtectedInteraction()
    {
        protectedInteractionCount++;
    }

    public void EndProtectedInteraction()
    {
        protectedInteractionCount = Mathf.Max(0, protectedInteractionCount - 1);

        if (protectedInteractionCount == 0 && minutes >= EndMinutes)
            TryStartTimeUpSequence();
    }

    public bool CanSpend(int amount)
    {
        amount = Mathf.Max(0, amount);

        if (IsInteractionProtected)
            return true;

        return minutes + amount <= EndMinutes;
    }

    public void Spend(int amount)
    {
        amount = Mathf.Max(0, amount);

        if (amount <= 0 || isEndingSequenceRunning)
            return;

        if (advanceRoutine != null)
            StopCoroutine(advanceRoutine);

        advanceRoutine = StartCoroutine(AdvanceTimeRoutine(amount));
    }

    public void SetTime(int hour, int minute)
    {
        if (isEndingSequenceRunning)
            return;

        minutes = Mathf.Clamp(hour * 60 + minute, StartMinutes, EndMinutes);
        timer = 0f;
        UpdateUI(triggerFlash: false, isNewHour: false);

        if (minutes >= EndMinutes && !IsInteractionProtected)
            TryStartTimeUpSequence();
    }

    public void ResetToStartTime()
    {
        if (advanceRoutine != null)
        {
            StopCoroutine(advanceRoutine);
            advanceRoutine = null;
        }

        if (newHourPauseRoutine != null)
        {
            StopCoroutine(newHourPauseRoutine);
            newHourPauseRoutine = null;
        }

        if (endingRoutine != null)
        {
            StopCoroutine(endingRoutine);
            endingRoutine = null;
        }

        isPausedForNewHour = false;
        isEndingSequenceRunning = false;
        protectedInteractionCount = 0;
        playerIsMoving = false;

        minutes = StartMinutes;
        timer = 0f;

        ResetVisualState();

        if (timeUpMessageGroup != null)
        {
            timeUpMessageGroup.alpha = 0f;
            timeUpMessageGroup.blocksRaycasts = false;
            timeUpMessageGroup.interactable = false;
        }

        if (fadeToBlackGroup != null)
        {
            fadeToBlackGroup.alpha = 0f;
            fadeToBlackGroup.blocksRaycasts = false;
            fadeToBlackGroup.interactable = false;
        }

        UpdateUI(triggerFlash: false, isNewHour: false);
    }

    public int GetCurrentMinutes()
    {
        return minutes;
    }

    public string GetFormattedTime()
    {
        int h = minutes / 60;
        int m = minutes % 60;
        return $"{h:00}:{m:00}";
    }

    private bool ShouldAdvancePassiveTime()
    {
        if (onlyAdvancePassiveTimeWhilePlayerMoves)
            return playerIsMoving;

        return true;
    }

    private float GetCurrentPassiveSecondsPerMinute()
    {
        if (playerIsMoving)
            return Mathf.Max(0.05f, movingSecondsPerMinute);

        return Mathf.Max(0.05f, idleSecondsPerMinute);
    }

    private IEnumerator AdvanceTimeRoutine(int minutesToAdvance)
    {
        for (int i = 0; i < minutesToAdvance; i++)
        {
            if (isEndingSequenceRunning)
                break;

            if (minutes >= EndMinutes && !IsInteractionProtected)
                break;

            while (isPausedForNewHour && !isEndingSequenceRunning)
                yield return null;

            if (isEndingSequenceRunning)
                break;

            AddMinuteAndRefresh();

            if (minutes >= EndMinutes && !IsInteractionProtected)
                break;

            while (isPausedForNewHour && !isEndingSequenceRunning)
                yield return null;

            if (isEndingSequenceRunning)
                break;

            yield return new WaitForSeconds(fastForwardMinuteSpeed);
        }

        advanceRoutine = null;

        if (minutes >= EndMinutes && !IsInteractionProtected)
            TryStartTimeUpSequence();
    }

    private void AddMinuteAndRefresh()
    {
        minutes++;

        bool isNewHour = minutes % 60 == 0;
        UpdateUI(triggerFlash: true, isNewHour: isNewHour);

        if (minutes >= EndMinutes && !IsInteractionProtected)
        {
            TryStartTimeUpSequence();
            return;
        }

        if (isNewHour && pauseOnNewHour)
        {
            if (newHourPauseRoutine != null)
                StopCoroutine(newHourPauseRoutine);

            newHourPauseRoutine = StartCoroutine(NewHourPauseRoutine());
        }
    }

    private IEnumerator NewHourPauseRoutine()
    {
        isPausedForNewHour = true;
        yield return new WaitForSeconds(newHourPauseSeconds);
        isPausedForNewHour = false;
        newHourPauseRoutine = null;
    }

    private void TryStartTimeUpSequence()
    {
        if (!triggerEndingWhenTimeIsUp || isEndingSequenceRunning || IsInteractionProtected)
            return;

        isEndingSequenceRunning = true;

        if (advanceRoutine != null)
        {
            StopCoroutine(advanceRoutine);
            advanceRoutine = null;
        }

        if (newHourPauseRoutine != null)
        {
            StopCoroutine(newHourPauseRoutine);
            newHourPauseRoutine = null;
        }

        isPausedForNewHour = false;
        endingRoutine = StartCoroutine(TimeUpSequenceRoutine());
    }

    private IEnumerator TimeUpSequenceRoutine()
    {
        if (timeUpMessageText != null)
            timeUpMessageText.text = timeUpMessage;

        if (timeUpMessageGroup != null)
        {
            timeUpMessageGroup.alpha = 1f;
            timeUpMessageGroup.blocksRaycasts = true;
            timeUpMessageGroup.interactable = false;
        }
        else if (timeUpMessageText != null)
        {
            timeUpMessageText.alpha = 1f;
        }

        yield return new WaitForSeconds(timeUpMessageSeconds);

        if (fadeToBlackGroup != null)
            yield return StartCoroutine(FadeCanvasGroup(fadeToBlackGroup, fadeToBlackGroup.alpha, 1f, fadeToBlackSeconds));

        if (timeUpMessageGroup != null)
        {
            timeUpMessageGroup.alpha = 0f;
            timeUpMessageGroup.blocksRaycasts = false;
            timeUpMessageGroup.interactable = false;
        }
        else if (timeUpMessageText != null)
        {
            timeUpMessageText.alpha = 0f;
        }

        if (!string.IsNullOrWhiteSpace(loadingSceneName))
            SceneManager.LoadScene(loadingSceneName);

        yield return new WaitForSeconds(loadingSceneSeconds);

        if (!string.IsNullOrWhiteSpace(culpritPickingSceneName))
            SceneManager.LoadScene(culpritPickingSceneName);
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup group, float from, float to, float duration)
    {
        if (group == null)
            yield break;

        group.blocksRaycasts = true;
        group.interactable = false;

        float safeDuration = Mathf.Max(0.01f, duration);
        float t = 0f;

        while (t < safeDuration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / safeDuration);
            group.alpha = Mathf.Lerp(from, to, p);
            yield return null;
        }

        group.alpha = to;
    }

    private void UpdateUI(bool triggerFlash, bool isNewHour)
    {
        if (timeText == null)
            return;

        timeText.text = GetFormattedTime();

        if (triggerFlash && flashOnTimeChange)
        {
            if (flashRoutine != null)
                StopCoroutine(flashRoutine);

            flashRoutine = isNewHour && specialFlashOnNewHour
                ? StartCoroutine(NewHourFlashRoutine())
                : StartCoroutine(NormalFlashRoutine());
        }
        else if (!triggerFlash)
        {
            ResetVisualState();
        }
    }

    private IEnumerator NormalFlashRoutine()
    {
        if (timeText == null)
            yield break;

        RectTransform rect = timeText.rectTransform;
        Vector3 targetScale = originalScale * flashScaleMultiplier;
        float duration = Mathf.Max(0.01f, flashDuration);

        timeText.color = flashColor;
        rect.localScale = targetScale;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / duration);

            rect.localScale = Vector3.Lerp(targetScale, originalScale, p);
            timeText.color = Color.Lerp(flashColor, normalColor, p);

            yield return null;
        }

        ResetVisualState();
        flashRoutine = null;
    }

    private IEnumerator NewHourFlashRoutine()
    {
        if (timeText == null)
            yield break;

        RectTransform rect = timeText.rectTransform;
        float blinkDuration = Mathf.Max(0.01f, newHourFlashDuration);
        int blinkCount = Mathf.Max(1, newHourFlashBlinks);
        Vector3 brightScale = originalScale * newHourFlashScaleMultiplier;

        for (int i = 0; i < blinkCount; i++)
        {
            float tOn = 0f;
            while (tOn < blinkDuration)
            {
                tOn += Time.deltaTime;
                float p = Mathf.Clamp01(tOn / blinkDuration);

                rect.localScale = Vector3.Lerp(originalScale, brightScale, p);
                timeText.color = Color.Lerp(normalColor, newHourFlashColor, p);

                yield return null;
            }

            float tOff = 0f;
            while (tOff < blinkDuration)
            {
                tOff += Time.deltaTime;
                float p = Mathf.Clamp01(tOff / blinkDuration);

                rect.localScale = Vector3.Lerp(brightScale, originalScale, p);
                timeText.color = Color.Lerp(newHourFlashColor, normalColor, p);

                yield return null;
            }
        }

        ResetVisualState();
        flashRoutine = null;
    }

    private void ResetVisualState()
    {
        if (timeText == null)
            return;

        timeText.color = normalColor;
        timeText.rectTransform.localScale = originalScale;
    }
}