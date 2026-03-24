using System.Linq;
using UnityEngine;
public abstract class DialogueCondition : ScriptableObject
{
    public abstract bool IsMet();
}

[CreateAssetMenu(menuName = "Dialogue/Conditions/New Item Requirement")]
public class ItemCondition : DialogueCondition
{
    public Item RequiredItem;
    public override bool IsMet()
    {
        return InventoryManager.Instance.Items.Contains(RequiredItem) ? true : false;
    }
}

[CreateAssetMenu(menuName = "Dialogue/Conditions/New Clue Requirement")]
public class ClueCondition : DialogueCondition
{
    public override bool IsMet()
    {
        return true;
    }
}

[CreateAssetMenu(menuName = "Dialogue/Conditions/New isWillingToTalk Requirement")]
public class isWillingToTalkCondition : DialogueCondition
{
    public DialogueSpeaker Speaker;
    public override bool IsMet()
    {
        return isWillingToTalkManager.instance.isSpeakerWillingToTalk[Speaker] ? true : false;
    }
}