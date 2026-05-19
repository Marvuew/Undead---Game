using Assets.Scripts.GameScripts;
using UnityEngine;

public class BorderLimit : MonoBehaviour
{
    public RuntimeDialogueGraph nodeToStart;



    private void OnTriggerEnter2D(Collider2D other)
    {

        if (other.CompareTag("Player"))
        {
            DialogueGraphManager.instance.StartDialogue(nodeToStart);
        }
    }
}

