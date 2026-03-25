using UnityEngine;

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
    [SerializeField] private bool useCurrentPivotRotationOnStart = true;
    [SerializeField] private float currentAngle = 0f;

    [Header("Runtime")]
    [SerializeField] private float walkedDistance;
    [SerializeField] private bool autoFindPlayer = true;

    [Header("Debug")]
    [Tooltip("Check this to reset walked distance (auto unchecks).")]
    [SerializeField] private bool resetDistanceNow;

    private Vector3 lastPlayerPosition;
    private bool hasPlayerPosition;

    public float NormalizedDayProgress =>
        Mathf.InverseLerp(minAngle, maxAngle, currentAngle);

    public bool IsAtEndOfDay => Mathf.Approximately(currentAngle, maxAngle);

    private void Awake()
    {
        if (autoFindPlayer && playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerTransform = player.transform;
        }
    }

    private void Start()
    {
        if (pivotPoint == null || handVisual == null)
        {
            Debug.LogError("HalfClockHand: Missing references.");
            enabled = false;
            return;
        }

        if (useCurrentPivotRotationOnStart)
        {
            currentAngle = NormalizeAngle(pivotPoint.localEulerAngles.z);
        }

        currentAngle = Mathf.Clamp(currentAngle, minAngle, maxAngle);

        walkedDistance = Mathf.InverseLerp(minAngle, maxAngle, currentAngle) * distanceForFullDay;

        ApplyRotation();

        if (playerTransform != null)
        {
            lastPlayerPosition = playerTransform.position;
            hasPlayerPosition = true;
        }
    }

    private void Update()
    {
        HandleDebugReset();

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
            return;

        walkedDistance += movedDistance;
        UpdateAngleFromDistance();
    }

    private void HandleDebugReset()
    {
        if (!resetDistanceNow)
            return;

        ResetClockToStart();

        // auto turn off so it behaves like a button
        resetDistanceNow = false;
    }

    private void UpdateAngleFromDistance()
    {
        float normalized = Mathf.Clamp01(walkedDistance / Mathf.Max(0.01f, distanceForFullDay));
        currentAngle = Mathf.Lerp(minAngle, maxAngle, normalized);
        ApplyRotation();
    }

    private void ApplyRotation()
    {
        pivotPoint.localRotation = Quaternion.Euler(0f, 0f, currentAngle);
    }

    public void ResetClockToStart()
    {
        walkedDistance = 0f;
        currentAngle = minAngle;
        ApplyRotation();

        if (playerTransform != null)
        {
            lastPlayerPosition = playerTransform.position;
            hasPlayerPosition = true;
        }
    }

    private float NormalizeAngle(float angle)
    {
        while (angle > 180f) angle -= 360f;
        while (angle < -180f) angle += 360f;
        return angle;
    }
}