using UnityEngine;
using UnityEngine.Events;

public class GameEvents : MonoBehaviour
{
    public static UnityEvent<RuntimeDialogueGraph> Dialogue = new UnityEvent<RuntimeDialogueGraph>();
    public static void StartDialogue(RuntimeDialogueGraph dialogue)
    {
        Dialogue.Invoke(dialogue);
    }

   

    public static UnityEvent<int> Humanity = new UnityEvent<int>();
    public static void ChangeHumanity(int humanity)
    {
        Humanity.Invoke(humanity);
    }

    public static UnityEvent<float> OnTimeChanged = new UnityEvent<float>();
    public static void ChangeTime(float ticks)
    {
        OnTimeChanged.Invoke(ticks);
    }

    public static UnityEvent<int> OnDayBegin = new UnityEvent<int>();
    public static void BeginDay(int day)
    {
        OnDayBegin.Invoke(day);
    }

    public static UnityEvent<int> OnDayEnd = new UnityEvent<int>();
    public static void EndDay(int day)
    {
        OnDayEnd.Invoke(day);
    }

    public static UnityEvent<float> OnRealtimeClockChanged = new UnityEvent<float>();
    public static void ChangeRealtimeClock(float time)
    {
        OnRealtimeClockChanged.Invoke(time);
    }


}
