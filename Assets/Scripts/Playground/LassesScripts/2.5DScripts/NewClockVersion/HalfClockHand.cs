using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HalfClockHand : MonoBehaviour
{
    [Header("Assign these in Inspector")]
    [SerializeField] private RectTransform pivotPoint;
    [SerializeField] private RectTransform handVisual;
    [SerializeField] private Transform playerTransform;

    [Header("Clock Progress")]
    [Min(0.01f)]
    [SerializeField] private float distanceForFullDay = 100f;

    [SerializeField] private bool stopAtEndOfDay = true;

    [Header("Half Clock Range")]
    [SerializeField] private float minAngle = -90f;
    [SerializeField] private float maxAngle = 90f;

    [Header("Start Angle")]
    [SerializeField] private bool useSharedCase1DayState = true;
    [SerializeField] private bool useCurrentPivotRotationOnStart = true;
    [SerializeField] private float currentAngle = 0f;

    [Header("Runtime")]
    [SerializeField] private float walkedDistance;
    [SerializeField] private bool autoFindPlayer = true;

    [Header("Debug")]
    [Tooltip("Check this to reset walked distance once. It auto-unticks.")]
    [SerializeField] private bool resetDistanceNow;

    [Header("Time Up Sequence")]
    [SerializeField] private bool triggerEndingWhenTimeIsUp = true;

    [TextArea]
    [SerializeField] private string timeUpMessage = "Time is up. We have to figure out who could have done this.";

    [SerializeField] private TMP_Text timeUpMessageText;
    [SerializeField] private CanvasGroup timeUpMessageGroup;
    [SerializeField] private float timeUpMessageSeconds = 2.5f;
    [SerializeField] private float textFadeOutSeconds = 0.8f;

    [Header("Scene Transition")]
    [SerializeField] private string loadingSceneName = "LoadingScene";
    [SerializeField] private float fadeToBlackSeconds = 1.5f;

    [Tooltip("Uses your DontDestroyOnLoad PersistentScreenFader if present.")]
    [SerializeField] private bool usePersistentScreenFader = true;

    private Vector3 lastPlayerPosition;
    private bool hasPlayerPosition;
    private bool endingTriggered;
    private Coroutine endingRoutine;

    public float CurrentAngle => currentAngle;
    public float WalkedDistance => walkedDistance;

    public float NormalizedDayProgress
    {
        get
        {
            if (Mathf.Approximately(minAngle, maxAngle))
                return 0f;

            return Mathf.InverseLerp(minAngle, maxAngle, currentAngle);
        }
    }

    public bool IsAtEndOfDay => Mathf.Approximately(currentAngle, maxAngle);
    public bool IsEndingTriggered => endingTriggered;

    private void Awake()
    {
        if (autoFindPlayer && playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerTransform = player.transform;
        }

        if (timeUpMessageGroup != null)
        {
            timeUpMessageGroup.alpha = 0f;
            timeUpMessageGroup.blocksRaycasts = false;
            timeUpMessageGroup.interactable = false;
        }
    }

    private void Start()
    {
        if (pivotPoint == null)
        {
            Debug.LogError("HalfClockHand: Pivot Point is not assigned.");
            enabled = false;
            return;
        }

        if (handVisual == null)
        {
            Debug.LogError("HalfClockHand: Hand Visual is not assigned.");
            enabled = false;
            return;
        }

        if (handVisual.parent != pivotPoint)
            Debug.LogWarning("HalfClockHand: Hand Visual should be a child of Pivot Point.");

        bool loadedFromSharedState = false;

        if (useSharedCase1DayState &&
            Case1DayState.Instance != null &&
            Case1DayState.Instance.IsCurrentScenePartOfCase1())
        {
            float savedProgress = Case1DayState.Instance.NormalizedDayProgress;
            walkedDistance = savedProgress * distanceForFullDay;
            currentAngle = Mathf.Lerp(minAngle, maxAngle, savedProgress);
            loadedFromSharedState = true;
        }

        if (!loadedFromSharedState)
        {
            if (useCurrentPivotRotationOnStart)
                currentAngle = NormalizeAngle(pivotPoint.localEulerAngles.z);

            currentAngle = Mathf.Clamp(currentAngle, minAngle, maxAngle);
            walkedDistance = Mathf.InverseLerp(minAngle, maxAngle, currentAngle) * distanceForFullDay;
        }

        ApplyRotation();

        if (playerTransform != null)
        {
            lastPlayerPosition = playerTransform.position;
            hasPlayerPosition = true;
        }

        if (triggerEndingWhenTimeIsUp && IsAtEndOfDay)
            TryStartTimeUpSequence();
    }

    private void Update()
    {
        HandleDebugReset();

        if (endingTriggered)
            return;

        if (playerTransform == null)
            return;

        if (!hasPlayerPosition)
        {
            lastPlayerPosition = playerTransform.position;
            hasPlayerPosition = true;
            return;
        }

        Vector3 currentPlayerPosition = playerTransform.position;
        float movedDistance = Vector3.Distance(currentPlayerPosition, lastPlayerPosition);
        lastPlayerPosition = currentPlayerPosition;

        if (movedDistance <= 0.0001f)
            return;

        if (stopAtEndOfDay && IsAtEndOfDay)
        {
            if (triggerEndingWhenTimeIsUp)
                TryStartTimeUpSequence();

            return;
        }

        walkedDistance += movedDistance;
        UpdateAngleFromDistance();

        if (triggerEndingWhenTimeIsUp && IsAtEndOfDay)
            TryStartTimeUpSequence();
    }

    private void HandleDebugReset()
    {
        if (!resetDistanceNow)
            return;

        ResetClockToStart();
        resetDistanceNow = false;
    }

    private void UpdateAngleFromDistance()
    {
        float normalized = Mathf.Clamp01(walkedDistance / Mathf.Max(0.01f, distanceForFullDay));
        currentAngle = Mathf.Lerp(minAngle, maxAngle, normalized);
        ApplyRotation();
        SaveSharedState();
    }

    private void ApplyRotation()
    {
        pivotPoint.localRotation = Quaternion.Euler(0f, 0f, currentAngle);
    }

    private void SaveSharedState()
    {
        if (!useSharedCase1DayState)
            return;

        if (Case1DayState.Instance == null)
            return;

        if (!Case1DayState.Instance.IsCurrentScenePartOfCase1())
            return;

        Case1DayState.Instance.NormalizedDayProgress = NormalizedDayProgress;
    }

    public void SetAngle(float angle)
    {
        currentAngle = Mathf.Clamp(angle, minAngle, maxAngle);
        walkedDistance = Mathf.InverseLerp(minAngle, maxAngle, currentAngle) * distanceForFullDay;
        ApplyRotation();
        SaveSharedState();

        if (triggerEndingWhenTimeIsUp && IsAtEndOfDay)
            TryStartTimeUpSequence();
    }

    public void SetProgress(float normalizedProgress)
    {
        normalizedProgress = Mathf.Clamp01(normalizedProgress);
        walkedDistance = normalizedProgress * distanceForFullDay;
        currentAngle = Mathf.Lerp(minAngle, maxAngle, normalizedProgress);
        ApplyRotation();
        SaveSharedState();

        if (triggerEndingWhenTimeIsUp && IsAtEndOfDay)
            TryStartTimeUpSequence();
    }

    public void AddDistance(float amount)
    {
        if (amount <= 0f || endingTriggered)
            return;

        walkedDistance += amount;
        UpdateAngleFromDistance();

        if (triggerEndingWhenTimeIsUp && IsAtEndOfDay)
            TryStartTimeUpSequence();
    }

    public void ResetClockToStart()
    {
        if (endingRoutine != null)
        {
            StopCoroutine(endingRoutine);
            endingRoutine = null;
        }

        endingTriggered = false;
        walkedDistance = 0f;
        currentAngle = minAngle;
        ApplyRotation();
        SaveSharedState();

        if (timeUpMessageGroup != null)
        {
            timeUpMessageGroup.alpha = 0f;
            timeUpMessageGroup.blocksRaycasts = false;
            timeUpMessageGroup.interactable = false;
        }

        if (timeUpMessageText != null)
        {
            Color c = timeUpMessageText.color;
            c.a = 1f;
            timeUpMessageText.color = c;
        }

        if (playerTransform != null)
        {
            lastPlayerPosition = playerTransform.position;
            hasPlayerPosition = true;
        }
        // After the existing reset logic, before the end of the method:
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerMovement2D pm = player.GetComponent<PlayerMovement2D>();
            if (pm != null) pm.UnlockMovement();
        }

    }

    private void TryStartTimeUpSequence()
{
    if (!triggerEndingWhenTimeIsUp || endingTriggered)
        return;

    endingTriggered = true;

    // Lock player movement
    GameObject player = GameObject.FindGameObjectWithTag("Player");
    if (player != null)
    {
        PlayerMovement2D pm = player.GetComponent<PlayerMovement2D>();
        if (pm != null) pm.LockMovement();
    }

    endingRoutine = StartCoroutine(TimeUpSequenceRoutine());
}


    private IEnumerator TimeUpSequenceRoutine()
{
    if (timeUpMessageText != null)
        timeUpMessageText.text = "";

    if (timeUpMessageGroup != null)
    {
        timeUpMessageGroup.alpha = 1f;
        timeUpMessageGroup.blocksRaycasts = true;
        timeUpMessageGroup.interactable = false;
    }

    if (timeUpMessageText != null)
        yield return StartCoroutine(TypewriterByWord(timeUpMessageText, timeUpMessage, 0.15f));

    yield return new WaitForSeconds(timeUpMessageSeconds);

    if (timeUpMessageGroup != null)
    {
        yield return StartCoroutine(FadeCanvasGroup(
            timeUpMessageGroup,
            timeUpMessageGroup.alpha,
            0f,
            textFadeOutSeconds
        ));

        timeUpMessageGroup.blocksRaycasts = false;
        timeUpMessageGroup.interactable = false;
    }
    else if (timeUpMessageText != null)
    {
        yield return StartCoroutine(FadeTMPText(
            timeUpMessageText,
            timeUpMessageText.alpha,
            0f,
            textFadeOutSeconds
        ));
    }

    if (usePersistentScreenFader && PersistentScreenFader.Instance != null)
        PersistentScreenFader.Instance.FadeToBlackAndLoadScene(loadingSceneName, fadeToBlackSeconds);
    else
        SceneManager.LoadScene(loadingSceneName);
}

private IEnumerator TypewriterByWord(TMP_Text text, string fullText, float delayBetweenWords)
{
    string[] words = fullText.Split(' ');
    string current = "";

    for (int i = 0; i < words.Length; i++)
    {
        current += (i == 0 ? "" : " ") + words[i];
        text.text = current;
        yield return new WaitForSeconds(delayBetweenWords);
    }
}

    private IEnumerator FadeCanvasGroup(CanvasGroup group, float from, float to, float duration)
    {
        float t = 0f;
        float safeDuration = Mathf.Max(0.01f, duration);

        while (t < safeDuration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / safeDuration);
            group.alpha = Mathf.Lerp(from, to, p);
            yield return null;
        }

        group.alpha = to;
    }

    private IEnumerator FadeTMPText(TMP_Text text, float from, float to, float duration)
    {
        float t = 0f;
        float safeDuration = Mathf.Max(0.01f, duration);

        while (t < safeDuration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / safeDuration);

            Color c = text.color;
            c.a = Mathf.Lerp(from, to, p);
            text.color = c;

            yield return null;
        }

        Color final = text.color;
        final.a = to;
        text.color = final;
    }

    private float NormalizeAngle(float angle)
    {
        while (angle > 180f)
            angle -= 360f;

        while (angle < -180f)
            angle += 360f;

        return angle;
    }
}