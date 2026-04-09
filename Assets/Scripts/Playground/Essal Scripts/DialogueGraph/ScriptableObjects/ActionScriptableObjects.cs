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

[CreateAssetMenu(menuName = "Dialogue/Actions/New TalkWillingness Action")]
public class TalkWillingnessAction : DialogueAction
{
    public DialogueSpeaker Speaker;
    public override void DoAction()
    {
        isWillingToTalkManager.instance.HandleVendetta(Speaker);
    }
}

[CreateAssetMenu(menuName = "Dialogue/Actions/New Give Item")]
public class GiveItemAction : DialogueAction
{
    public Item item;

    public override void DoAction()
    {
        InventoryManager.Instance.Items.Remove(item);
    }
}
