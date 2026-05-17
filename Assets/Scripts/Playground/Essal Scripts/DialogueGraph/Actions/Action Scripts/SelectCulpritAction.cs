using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/Actions/Select Culprit Action")]
public class SelectCulpritAction : DialogueAction
{
    public override void DoAction()
    {
        Debug.Log("Doing Selecting Culprit Action");
        var script = FindAnyObjectByType<SelectionHandler>();
        if (script != null)
        {
            script.SetupCorkBoard(GameManager.instance.undeadDatabase);
        }
        else
        {
            Debug.LogWarning("The Selection Handler couldnt be found in the corkboard Dialogue???");
        }

    }
}
