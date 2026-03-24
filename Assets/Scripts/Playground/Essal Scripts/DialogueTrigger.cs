using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public Dialogue3 dialogue;

    public void TriggerDialogue()
    {
        DialogueManager.instance.StartDialogue(dialogue);
    }
}
