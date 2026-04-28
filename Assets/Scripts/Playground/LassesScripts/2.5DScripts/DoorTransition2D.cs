using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class DoorTransition2D : MonoBehaviour
{
    [Header("Scene")]
    public SceneNames sceneName;
    public SceneNames sceneID;

    [Header("Interaction")]
    public KeyCode interactKey = KeyCode.E;
    public string playerTag = "Player";

    [Header("Door Type")]
    public bool Enter = true;
    public bool Exit = false;

    [Header("Fake Walk After Entering Scene")]
    public Vector2 autoWalkDirection = Vector2.down;
    public float autoWalkDistance = 1.0f;

    [Header("Fade")]
    public float fadeDuration = 1.0f;
    public Color fadeColor = Color.black;

    [Header("Prompt")]
    public Vector2 promptOffset = new Vector2(0, -40);
    public int fontSize = 24;

    private bool playerInRange = false;
    private bool isTransitioning = false;

    void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    void Update()
    {
        if (isTransitioning) return;

        if (playerInRange && Input.GetKeyDown(interactKey))
        {
            isTransitioning = true;

            TransitionState2D.SetTransition(
                sceneName.ToString(),
                autoWalkDirection,
                autoWalkDistance
            );

            if (WorldFade.Instance != null)
            {
                WorldFade.Instance.StartSceneTransition(sceneName.ToString(), fadeDuration, fadeColor);
            }
            else
            {
                SceneManager.LoadScene(sceneName.ToString());
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
            playerInRange = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
            playerInRange = false;
    }

    void OnGUI()
    {
        if (!playerInRange || isTransitioning) return;

        string actionText = Enter ? "Enter" : Exit ? "Exit" : "Use";
        string prompt = $"Press {interactKey} to {actionText}";

        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = fontSize;
        style.alignment = TextAnchor.MiddleCenter;
        style.normal.textColor = Color.white;
        style.fontStyle = FontStyle.Bold;

        Vector2 size = style.CalcSize(new GUIContent(prompt));
        Rect rect = new Rect(
            (Screen.width - size.x) * 0.5f + promptOffset.x,
            Screen.height - 100 + promptOffset.y,
            size.x + 20,
            size.y + 10
        );

        GUI.color = new Color(0, 0, 0, 0.6f);
        GUI.Box(rect, "");
        GUI.color = Color.white;
        GUI.Label(rect, prompt, style);
    }
}