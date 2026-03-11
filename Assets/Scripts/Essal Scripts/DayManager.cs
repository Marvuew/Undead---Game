using TMPro;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class DayManager : MonoBehaviour
{
    public TextMeshProUGUI text;
    public Animator animator;
    public Image image;

    private void OnEnable()
    {
        GameEvents.OnDayEnd.AddListener(EndDay);
    }

    private void OnDisable()
    {
        GameEvents.OnDayEnd.RemoveListener(EndDay);
    }
    
    public void EndDay(int day)
    {
        StartCoroutine(EndDaySequence(day));
    }

    IEnumerator EndDaySequence(int day)
    {
        if (TimeManager.Instance.RealTimeMode)
        {
            TimeManager.Instance.RealTimeMode = false;
        }

        image.raycastTarget = true;
        animator.SetBool("EndDay", true);
        yield return new WaitForSeconds(2);
        text.enabled = true;
        text.text = $"Day {day} ended. You succeded in finding the culprit. Skipping to day {day + 1}";    
    }

    public void StartDay(int day)
    {
        StartCoroutine(StartDaySequence(day));
    }

    IEnumerator StartDaySequence(int day)
    {
        image.raycastTarget = true;
        animator.SetBool("EndDay", false);
        yield return new WaitForSeconds(2);
        text.enabled = false;
        text.text = $"Its a new day. Birds are chirping. Time to get to work";
    }
}
