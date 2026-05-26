using Assets.Scripts.GameScripts;
using UnityEngine;

public class BorderLimit : MonoBehaviour
{
    public RuntimeDialogueGraph nodeToStart;



    private void OnCollisionEnter2D(Collision2D other)
    {

        if (other.gameObject.CompareTag("Player"))
        {
            DialogueGraphManager.instance.StartDialogue(nodeToStart);
        }
    }
}

