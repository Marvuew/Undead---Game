using UnityEngine;

public class ClockStateMemory : MonoBehaviour
{
    public static ClockStateMemory Instance { get; private set; }

    [Header("Saved Clock State")]
    public float savedWalkedDistance = 0f;
    public float savedCurrentAngle = -90f;
    public float savedPivotAngle = -90f;
    public bool hasSavedState = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SaveClock(float walkedDistance, float currentAngle)
    {
        savedWalkedDistance = Mathf.Max(0f, walkedDistance);
        savedCurrentAngle = currentAngle;
        savedPivotAngle = currentAngle;
        hasSavedState = true;
    }

    public void AddWalkedDistance(float amount)
    {
        if (amount <= 0f)
            return;

        savedWalkedDistance += amount;
        hasSavedState = true;
    }

    public void ResetClock(float startAngle)
    {
        savedWalkedDistance = 0f;
        savedCurrentAngle = startAngle;
        savedPivotAngle = startAngle;
        hasSavedState = true;
    }
}