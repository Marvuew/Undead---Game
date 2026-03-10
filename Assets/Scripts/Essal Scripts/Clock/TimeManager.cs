using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance;

    public int CurrentDay = 1;
    public int CurrentTickOfDay = 0;

    public const int TICKS_PER_DAY = 24;

    public bool ClockIsAnimating = false;

    public Animator DayAnimation;

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        GameEvents.OnDayEnd.AddListener(EndDay);
    }

    private void OnDisable()
    {
        GameEvents.OnDayEnd.RemoveListener(EndDay);
    }


    public void AdvanceTick()
    {
        if (ClockIsAnimating)
        {
            Debug.Log("Clock is still animating");
            return;
        }
        CurrentTickOfDay++;

        if (CurrentTickOfDay >= TICKS_PER_DAY)
        {
            CurrentTickOfDay = 0;
            GameEvents.EndDay(CurrentDay);
            CurrentDay++;
        }

        GameEvents.ChangeTime(CurrentTickOfDay);
    }

    public void EndDay(int day)
    {
        DayAnimation.SetBool("EndDay", true);
    }
    
}