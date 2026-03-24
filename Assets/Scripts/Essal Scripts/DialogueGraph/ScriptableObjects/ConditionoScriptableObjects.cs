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
