using Assets.Scripts.GameScripts;
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
    public Clue Clue;

    public override bool IsMet()
    {
        return CaseManager.Instance.cluesfound.Contains(Clue);
    }
}

[CreateAssetMenu(menuName = "Dialogue/Conditions/New isWillingToTalk Requirement", fileName = "TalkWillingness")]
public class isWillingToTalkCondition : DialogueCondition
{
    public DialogueSpeaker Speaker;
    public override bool IsMet()
    {
        return DialogueGraphManager.instance.speakersNotWillingToTalk.Contains(Speaker) ? false : true;
    }
}

[CreateAssetMenu(menuName = "Dialogue/Conditions/New Alignment Condition")]
public class AlignmentCondition : DialogueCondition
{
    [Header("Set the minimum requirement")]
    public int humanity;
    public int undead;

    public override bool IsMet()
    {
        if (humanity <= Player.Instance.humanity && undead <= Player.Instance.undead)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}

/*[CreateAssetMenu(menuName = "Dialogue/Conditions/New Callback Condition")]
public class CallBackCondition : DialogueCondition
{
    CallBackAction
    public override bool IsMet()
    {
        CallbackManager.instance.Callbacks.Add(CallbackTarget);
    }
}*/