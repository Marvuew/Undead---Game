using Assets.Scripts.GameScripts;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D))]
public class NecrolexiconPickup : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HouseIntroController houseIntroController;
    [SerializeField] private RuntimeDialogueGraph bookDialogueGraph;

    [Header("Interaction")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    [Header("Prompt")]
    [SerializeField] private bool showPrompt = true;
    [SerializeField] private string promptText = "Pick up";
    [SerializeField] private Vector3 worldPromptOffset = new Vector3(0f, 1.2f, 0f);
    [SerializeField] private int fontSize = 22;

    private bool playerInRange;
    private bool pickedUp;

    private Camera mainCamera;

    private void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (pickedUp || !playerInRange)
            return;

        if (houseIntroController != null && !houseIntroController.CanPickUpBook())
            return;

        if (DialogueGraphManager.instance != null && DialogueGraphManager.instance.isDialogueRunning)
            return;

        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            pickedUp = true;

            StartBookDialogue();

            if (houseIntroController != null)
                houseIntroController.BookPickedUp();
        }
    }

    private void StartBookDialogue()
    {
        if (DialogueGraphManager.instance == null || bookDialogueGraph == null)
            return;

        if (Player.Instance != null)
            Player.Instance.interacting = true;

        DialogueGraphManager.instance.gameObject.SetActive(true);

        if (DialogueGraphManager.instance.DialoguePanel != null)
            DialogueGraphManager.instance.DialoguePanel.SetActive(true);

        DialogueGraphManager.instance.StartDialogue(bookDialogueGraph);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
            playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
            playerInRange = false;
    }

    private void OnGUI()
    {
        if (!showPrompt)
            return;

        if (!playerInRange || pickedUp)
            return;

        if (houseIntroController != null && !houseIntroController.CanPickUpBook())
            return;

        if (DialogueGraphManager.instance != null && DialogueGraphManager.instance.isDialogueRunning)
            return;

        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera == null)
            return;

        Vector3 worldPosition = transform.position + worldPromptOffset;
        Vector3 screenPosition = mainCamera.WorldToScreenPoint(worldPosition);

        if (screenPosition.z < 0)
            return;

        string prompt = $"Press {interactKey} to {promptText}";

        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = fontSize;
        style.alignment = TextAnchor.MiddleCenter;
        style.normal.textColor = Color.white;
        style.fontStyle = FontStyle.Bold;

        Vector2 size = style.CalcSize(new GUIContent(prompt));

        Rect rect = new Rect(
            screenPosition.x - (size.x + 20) / 2f,
            Screen.height - screenPosition.y - (size.y + 10) / 2f,
            size.x + 20,
            size.y + 10
        );

        GUI.color = new Color(0, 0, 0, 0.6f);
        GUI.Box(rect, "");
        GUI.color = Color.white;
        GUI.Label(rect, prompt, style);
    }
}