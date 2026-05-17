using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/Actions/Skip Tutorial")]
public class SkipTutorialActionScriptableObject : DialogueAction
{
    public override void DoAction()
    {
        var script = FindAnyObjectByType<HouseIntroController>();
        if (script != null)
        {
            script.SkipTutorial();
            Debug.Log("Tutorial Skipped with dialogue action");
        }
        else Debug.LogWarning("Couldnt call the skip dialogue function cause the HouseIntroController script was null");
    }
}
