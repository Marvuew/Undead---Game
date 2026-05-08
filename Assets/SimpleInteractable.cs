using Assets.Scripts.GameScripts;
using UnityEngine;

public class SimpleInteractable : MonoBehaviour
{
        public RuntimeDialogueGraph dialogueGraph;
        [SerializeField] private GameObject interactText;

        private void Awake()
        {
            interactText.SetActive(false);
        }
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
            {
                collision.gameObject.GetComponent<Player>().currentSimpleInteractable = this;
                interactText.SetActive(true);
            }
        }
        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
            {
                collision.gameObject.GetComponent<Player>().currentSimpleInteractable = null;
                interactText.SetActive(false);
            }
        }
        public void startInteraction()
        {
            DialogueGraphManager.instance.StartDialogue(dialogueGraph);
        }
}
