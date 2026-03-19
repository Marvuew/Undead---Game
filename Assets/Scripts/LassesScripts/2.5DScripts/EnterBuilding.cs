using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class EnterBuilding : MonoBehaviour
{
    [Header("Player")]
    public string playerTag = "Player";

    [Header("Scene")]
    public string sceneToLoad;

    [Header("Visual")]
    public SpriteRenderer spriteRenderer;
    public Color normalColor = Color.white;
    public Color highlightColor = new Color(1.2f, 1.2f, 1.2f); // slightly brighter

    private bool playerInRange = false;

    void Start()
    {
        // Make sure collider is trigger
        GetComponent<Collider2D>().isTrigger = true;

        if (spriteRenderer != null)
            spriteRenderer.color = normalColor;
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            SceneManager.LoadScene(sceneToLoad);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = true;

            if (spriteRenderer != null)
                spriteRenderer.color = highlightColor;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = false;

            if (spriteRenderer != null)
                spriteRenderer.color = normalColor;
        }
    }
}