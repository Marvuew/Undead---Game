using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance;

    public int CurrentDay = 1;
    // We use realTimeHour as the single source of truth for the clock's position
    [SerializeField] private float realTimeHour;

    public bool ClockIsAnimating = false;
    public bool RealTimeMode = false;
    public float RealTimeSpeed = 1f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Update()
    {
        if (RealTimeMode && !ClockIsAnimating)
        {
            // Pass time continuously
            realTimeHour += (Time.deltaTime * RealTimeSpeed) / 3600f; // Assuming speed is in real-seconds per hour

            if (realTimeHour >= 24f)
            {
                realTimeHour -= 24f;
                CurrentDay++;
                GameEvents.EndDay(CurrentDay);
            }

            // Tell UI to update its hands without animating
            GameEvents.ChangeRealtimeClock(realTimeHour);
        }
    }

    // This handles "skipping" time (e.g., clicking a button to pass 1 hour)
    public void AdvanceTick(float hoursToAdvance = 1f)
    {
        if (ClockIsAnimating) return;

        float targetHour = realTimeHour + hoursToAdvance;

        // If we cross midnight during this skip
        if (targetHour >= 24f)
        {
            // Logic for day end can trigger here or after animation
            // For now, let's keep it simple:
            targetHour %= 24f;
            CurrentDay++;
            GameEvents.EndDay(CurrentDay);
        }

        realTimeHour = targetHour;

        // Use the event that ClockUI.AnimateClock is listening to
        // We pass the new target time for the hands to spin toward
        GameEvents.ChangeTime(realTimeHour);
    }  
}