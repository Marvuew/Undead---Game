using System.Linq;
using UnityEngine;
public abstract class DialogueCondition : ScriptableObject
{
    public abstract bool IsMet();
}

[CreateAssetMenu(menuName = "Dialogue/New Item Requirement")]
public class ItemCondition : DialogueCondition
{
    public Item RequiredItem;
    public override bool IsMet()
    {
        return InventoryManager.Instance.Items.Contains(RequiredItem) ? true : false;
    }
}

[CreateAssetMenu(menuName = "Dialogue/New Clue Requirement")]
public class ClueCondition : DialogueCondition
{
    public override bool IsMet()
    {
        return true;
    }
}
