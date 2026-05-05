using System.Collections;
using UnityEngine;
public class WaitAction : DialogueAction
{
    public float waitTime = 2f;

    public override void DoAction()
    {
        WorldFade.Instance.StartCoroutine(WaitRoutine());
    }

    private IEnumerator WaitRoutine()
    {
        yield return new WaitForSeconds(waitTime);
    }
}