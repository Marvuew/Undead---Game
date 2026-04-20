using Assets.Scripts.GameScripts;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class interactable : MonoBehaviour
{
    public RuntimeDialogueGraph dialogueGraph;
    //[SerializeField] private Dialogue dialogue;
    [SerializeField] private GameObject interactText;
    public Clue clue;
    bool found;
    public ClueType clueType;

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
        DialogueGraphManager.instance.StartDialogue(dialogueGraph);
        if (clue.clueType == ClueType.Human) AudioManager.instance.PlaySFX("InteractableHuman");
        if (!found && clue != null)
            CaseManager.Instance.OnClueFound(clue);
        found = true;
    }

}
