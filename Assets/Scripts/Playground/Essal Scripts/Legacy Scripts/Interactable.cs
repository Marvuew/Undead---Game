using Assets.Scripts.GameScripts;
using TMPro;
using UnityEngine;

public class interactable : MonoBehaviour
{
    [SerializeField] private Dialogue dialogue;
    [SerializeField] private GameObject interactText;
    private void Awake()
    {
        interactText.SetActive(false);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            //CurrentInteractable isnt on the player class?
            //collision.GetComponent<Player>().currentInteractable = this;
            interactText.SetActive(true);
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) 
        {
            //CurrentInteractable isnt on the player class?
            //collision.GetComponent<Player>().currentInteractable = null;
            interactText.SetActive(false);
        }
    }
    public void startInteraction() 
    {
        DialougeManager.instance.StartDialogue(dialogue);
    }
}
