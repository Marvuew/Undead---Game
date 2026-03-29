using Assets.Scripts.GameScripts;
using Assets.Scripts.Playground.GameScripts;
using TMPro;
using UnityEngine;

public class interactable : MonoBehaviour
{
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
        if (!found)
        {
            DialougeManager.instance.StartDialogue(dialogue);
            CaseManager.instance.ClueFound(clue);
        }
        found = true;
    }
}
