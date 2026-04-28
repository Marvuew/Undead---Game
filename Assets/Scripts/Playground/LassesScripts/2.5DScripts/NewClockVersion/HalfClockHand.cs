using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HalfClockHand : MonoBehaviour
{
    [Header("Assign these in Inspector")]
    [SerializeField] private RectTransform pivotPoint;
    [SerializeField] private RectTransform handVisual;

    [Header("Clock Progress")]
    [Min(0.01f)]
    [SerializeField] private float distanceForFullDay = 100f;
    [SerializeField] private bool stopAtEndOfDay = true;

    [Header("Half Clock Range")]
    [SerializeField] private float minAngle = -90f;
    [SerializeField] private float maxAngle = 90f;

    [Header("Scene Start Behavior")]
    [SerializeField] private bool resetSavedClockOnSceneStart = false;

    [SerializeField] private float currentAngle = -90f;

    [Header("Runtime")]
    [SerializeField] private float walkedDistance;

    [Header("Debug")]
    [SerializeField] private bool resetDistanceNow;

    [Header("Time Up Sequence")]
    [SerializeField] private bool triggerEndingWhenTimeIsUp = true;

    [TextArea]
    [SerializeField] private string timeUpMessage = "Time is up. We have to figure out who could have done this.";

    [Header("Scene Transition")]
    [SerializeField] private string loadingSceneName = "LoadingScene";
    [SerializeField] private float fadeToBlackSeconds = 1.5f;

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

    public bool IsAtEndOfDay => currentAngle >= maxAngle - 0.01f;
    public bool IsEndingTriggered => endingTriggered;

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

        if (resetSavedClockOnSceneStart)
        {
            ResetClockToStart();
        }
        else
        {
            LoadFromMemory();
            UpdateAngleFromDistance();
        }
    }

    private void Update()
    {
        HandleDebugReset();

        if (endingTriggered)
            return;

        LoadFromMemory();
        UpdateAngleFromDistance();

        if (triggerEndingWhenTimeIsUp && IsAtEndOfDay)
            TryStartTimeUpSequence();
    }

    private void LoadFromMemory()
    {
        if (ClockStateMemory.Instance == null || !ClockStateMemory.Instance.hasSavedState)
        {
            walkedDistance = 0f;
            currentAngle = minAngle;
            SaveToMemory();
            return;
        }

        walkedDistance = ClockStateMemory.Instance.savedWalkedDistance;
        walkedDistance = Mathf.Clamp(walkedDistance, 0f, distanceForFullDay);
    }

    private void SaveToMemory()
    {
        if (ClockStateMemory.Instance == null)
            return;

        ClockStateMemory.Instance.SaveClock(walkedDistance, currentAngle);
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
        SaveToMemory();
    }

    private void ApplyRotation()
    {
        if (pivotPoint == null)
            return;

        pivotPoint.localRotation = Quaternion.Euler(0f, 0f, currentAngle);
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

        if (ClockStateMemory.Instance != null)
            ClockStateMemory.Instance.ResetClock(minAngle);

        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            ClockPlayerMovement2D pm = player.GetComponent<ClockPlayerMovement2D>();

            if (pm != null)
                pm.UnlockMovement();
        }
    }

    private void TryStartTimeUpSequence()
    {
        if (!triggerEndingWhenTimeIsUp || endingTriggered)
            return;

        endingTriggered = true;

        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            ClockPlayerMovement2D pm = player.GetComponent<ClockPlayerMovement2D>();

            if (pm != null)
                pm.LockMovement();
        }

        endingRoutine = StartCoroutine(TimeUpSequenceRoutine());
    }

    private IEnumerator TimeUpSequenceRoutine()
    {
        if (PersistentScreenFader.Instance != null)
        {
            PersistentScreenFader.Instance.FadeToBlackAndLoadScene(
                loadingSceneName,
                fadeToBlackSeconds,
                timeUpMessage
            );
        }
        else
        {
            Debug.LogError("HalfClockHand: No PersistentScreenFader found.");
            SceneManager.LoadScene(loadingSceneName);
        }

        yield break;
    }

    public void PauseClockTracking(bool pause)
    {
        // Kept so old door/scene scripts do not break.
        // Distance is no longer tracked from transform position,
        // so this no longer needs to do anything.
    }

    public void ResyncPlayerTracking()
    {
        // Kept so old door/scene scripts do not break.
        // Distance is no longer tracked from transform position.
    }
}