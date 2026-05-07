using UnityEngine;
using System.Linq;

[CreateAssetMenu(menuName = "Dialogue/Actions/Toggle GameObject Action")]
public class ToggleGameObjectAction : DialogueAction
{
    public string targetGameObject;
    public bool setActive;

    public override void DoAction()
    {
        RuntimeInteractable[] interactables = GameObject.FindObjectsByType<RuntimeInteractable>(FindObjectsInactive.Include);
        RuntimeInteractable target = null;
        foreach (var interactable in interactables)
        {
            if (interactable.gameObject.name == targetGameObject)
            {
                target = interactable;
                break;
            }
        }
        if (target != null)
        {
            target.gameObject.SetActive(setActive);
        }
        else
        {
            Debug.LogWarning("GameObject with name " + targetGameObject + " not found in the scene.");
        }
    }
}
