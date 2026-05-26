using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/Conditions/Clues Found Condition (min 3)")]
public class CluesFoundCondition : DialogueCondition
{
    public override bool IsMet()
    {
        return CaseManager.Instance.cluesfound.Count >= 3;
    }
}
