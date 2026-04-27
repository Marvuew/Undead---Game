using UnityEngine;
using UnityEngine.SceneManagement;

public class SimpleClockHand : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform pivotPoint;

    [Header("Player Auto Find")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private Transform player;

    [Header("Clock Range")]
    [SerializeField] private float minAngle = -90f;
    [SerializeField] private float maxAngle = 90f;

    private Vector3 lastPlayerPosition;
    private bool hasLastPosition;

    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        FindPlayer();
        ApplyRotationFromMemory();
        ResetPlayerTracking();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindPlayer();
        ApplyRotationFromMemory();
        ResetPlayerTracking();
    }

    private void Update()
    {
        if (pivotPoint == null)
            return;

        if (ClockMemory.Instance == null)
            return;

        if (player == null)
        {
            FindPlayer();
            return;
        }

        if (!hasLastPosition)
        {
            ResetPlayerTracking();
            return;
        }

        Vector3 currentPosition = player.position;
        float distanceThisFrame = Vector3.Distance(currentPosition, lastPlayerPosition);
        lastPlayerPosition = currentPosition;

        if (distanceThisFrame > 0.0001f)
            ClockMemory.Instance.AddDistance(distanceThisFrame);

        ApplyRotationFromMemory();
    }

    private void FindPlayer()
    {
        GameObject foundPlayer = GameObject.FindGameObjectWithTag(playerTag);

        if (foundPlayer != null)
            player = foundPlayer.transform;
    }

    private void ResetPlayerTracking()
    {
        if (player == null)
        {
            hasLastPosition = false;
            return;
        }

        lastPlayerPosition = player.position;
        hasLastPosition = true;
    }

    private void ApplyRotationFromMemory()
    {
        if (pivotPoint == null || ClockMemory.Instance == null)
            return;

        float t = ClockMemory.Instance.GetNormalizedProgress();
        float angle = Mathf.Lerp(minAngle, maxAngle, t);

        pivotPoint.localRotation = Quaternion.Euler(0f, 0f, angle);
    }
}