using Assets.Scripts.GameScripts;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class RuntimeInteractable : MonoBehaviour
{
    public RuntimeDialogueGraph dialogueGraph;
    //[SerializeField] private Dialogue dialogue;
    [SerializeField] private GameObject interactText;
    public InteractableScriptableObject interactableData;
    public InteractableType interactableType;

    private void Awake()
    {
        interactText.SetActive(false);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            collision.GetComponent<Player>().currentInteractable = this;
            interactText.SetActive(true);
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            collision.GetComponent<Player>().currentInteractable = null;
            interactText.SetActive(false);
        }
    }
    public void startInteraction()
    {
        DialogueGraphManager.instance.currentInteractable = interactableData;
        DialogueGraphManager.instance.StartDialogue(dialogueGraph);
        if (interactableType == InteractableType.Human) AudioManager.instance.PlaySFX("InteractableHuman");
    }

}

public enum InteractableType
{
    Human,
    Item
}
