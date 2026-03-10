using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [Header("Dialogue")]
    [SerializeField] private RuntimeDialogueGraph dialogue;

    public RuntimeDialogueGraph Dialogue => dialogue;

    public void TriggerDialogue()
    {
        if (!dialogue)
        {
            Debug.LogWarning($"{name} has no dialogue assigned.");
            return;
        }

        GameEvents.StartDialogue(dialogue);
    }
}