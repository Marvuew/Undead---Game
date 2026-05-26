using Assets.Scripts.GameScripts;
using UnityEngine;

public class NoOneIsHomeInteractable : MonoBehaviour
{
    [SerializeField] private GameObject interactText;

    private void Awake()
    {
        interactText.SetActive(false);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            interactText.SetActive(true);
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            interactText.SetActive(false);
        }
    }
}
