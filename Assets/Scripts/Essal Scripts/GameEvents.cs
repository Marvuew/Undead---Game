using UnityEngine;
using UnityEngine.Events;

public class GameEvents : MonoBehaviour
{
    public static UnityEvent<RuntimeDialogueGraph> Dialogue = new UnityEvent<RuntimeDialogueGraph>();
    public static void StartDialogue(RuntimeDialogueGraph dialogue)
    {
        Dialogue.Invoke(dialogue);
    }

   

    public static UnityEvent<int> Humanity = new UnityEvent<int>();
    public static void ChangeHumanity(int humanity)
    {
        Humanity.Invoke(humanity);
    }
}
