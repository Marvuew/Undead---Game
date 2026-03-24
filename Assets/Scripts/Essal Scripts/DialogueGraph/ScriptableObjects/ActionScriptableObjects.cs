using UnityEngine;

public abstract class DialogueAction : ScriptableObject
{
    public abstract void DoAction();
}

[CreateAssetMenu(menuName = "Dialogue/Actions/New Kill Action")]
public class KillAction : DialogueAction
{
    public override void DoAction()
    {
        Debug.Log("KILL");
    }
}

[CreateAssetMenu(menuName = "Dialogue/Actions/New Resolve Action")]
public class ResolveAction : DialogueAction
{
    public override void DoAction()
    {
        Debug.Log("RESOLVE");
    }
}
