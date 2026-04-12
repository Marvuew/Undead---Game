using Assets.Scripts.GameScripts;
using TMPro;
using UnityEngine;

public class interactable : MonoBehaviour
{
    public RuntimeDialogueGraph dialogueGraph;
    [SerializeField] private Dialogue dialogue;
    [SerializeField] private GameObject interactText;
    public Clue clue;
    bool found;

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
        DialougeManager.Instance.StartDialogue(dialogueGraph);
        if (!found)
        {
            CaseManager.Instance.OnClueFound(clue);
            Debug.Log("New clue");
        }
        else Debug.Log("NOT new clue");
        found = true;
    }

}
