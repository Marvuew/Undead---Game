using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class DoorTransition2D : MonoBehaviour
{
    [Header("Scene")]
    public string sceneToLoad;
    public string targetSpawnPointId;

    [Header("Interaction")]
    public KeyCode interactKey = KeyCode.E;
    public string playerTag = "Player";

    [Header("Fake Walk After Entering Scene")]
    public Vector2 autoWalkDirection = Vector2.down;
    public float autoWalkDistance = 1.0f;

    private bool playerInRange = false;

    void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(interactKey))
        {
            TransitionState2D.SetTransition(
                targetSpawnPointId,
                autoWalkDirection,
                autoWalkDistance
            );

            SceneManager.LoadScene(sceneToLoad);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = false;
        }
    }
}