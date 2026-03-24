using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class DialogueInteractable : MonoBehaviour
{
    [Header("Dialogue to start")]
    [SerializeField] private MessageChainDialogue dialogue;

    [Header("Detection")]
    [SerializeField] private Camera cam;                 // kept for compatibility
    [SerializeField] private Collider2D targetCollider;  // if null -> GetComponent<Collider2D>()
    [SerializeField] private bool requireLeftClick = true; // kept for compatibility, no longer used for interaction

    [Header("Player Proximity Interaction")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private Key interactKey = Key.E;
    [SerializeField] private bool requirePlayerInRange = true;

    [Header("Hover highlight (2D)")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private float hoverBrightnessMultiplier = 1.25f;

    [Header("Optional")]
    [SerializeField] private bool disableAfterUse = false;
    [SerializeField] private bool lockPlayerMovementDuringDialogue = true;

    private bool isHovering;
    private bool playerInRange;
    private bool interactionStarted;
    private PlayerMovement2D currentPlayerMovement;

    private void Awake()
    {
        if (cam == null) cam = Camera.main;
        if (targetCollider == null) targetCollider = GetComponent<Collider2D>();
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        SetHover(false);

        if (targetCollider == null)
            Debug.LogError($"DialogueInteractable on {name}: Missing Collider2D. Add BoxCollider2D or PolygonCollider2D.");

        if (cam == null)
            Debug.LogError($"DialogueInteractable on {name}: No camera found. Assign a Camera or tag your camera as MainCamera.");

        if (dialogue == null)
            Debug.LogWarning($"DialogueInteractable on {name}: dialogue reference is missing.");
    }

    private void Update()
    {
        if (targetCollider == null) return;

        // If clicking UI, ignore
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            if (isHovering) SetHover(false);
            return;
        }

        bool shouldHighlight = requirePlayerInRange ? playerInRange : false;

        // Don't highlight while dialogue is already active
        if (interactionStarted)
            shouldHighlight = false;

        if (shouldHighlight != isHovering)
            SetHover(shouldHighlight);

        // Start interaction
        if (!interactionStarted && shouldHighlight)
        {
            if (Keyboard.current != null && Keyboard.current[interactKey].wasPressedThisFrame)
                Interact();
        }

        // Unlock when dialogue ends
        if (interactionStarted && dialogue != null && !dialogue.IsConversationRunning)
        {
            interactionStarted = false;

            if (lockPlayerMovementDuringDialogue && currentPlayerMovement != null)
                currentPlayerMovement.UnlockMovement();

            if (disableAfterUse)
                enabled = false;
        }
    }

    private void Interact()
    {
        if (dialogue == null)
        {
            Debug.LogWarning($"DialogueInteractable on {name}: dialogue reference missing.");
            return;
        }

        SetHover(false);
        interactionStarted = true;

        if (lockPlayerMovementDuringDialogue && currentPlayerMovement != null)
            currentPlayerMovement.LockMovement();

        dialogue.StartConversationFromInteraction();
    }

    private void SetHover(bool on)
    {
        isHovering = on;

        if (spriteRenderer == null) return;

        if (!on)
        {
            spriteRenderer.color = normalColor;
            return;
        }

        Color c = normalColor;
        c.r *= hoverBrightnessMultiplier;
        c.g *= hoverBrightnessMultiplier;
        c.b *= hoverBrightnessMultiplier;
        c.a = normalColor.a;

        spriteRenderer.color = c;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        playerInRange = true;
        currentPlayerMovement = other.GetComponent<PlayerMovement2D>();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        playerInRange = false;
        SetHover(false);

        currentPlayerMovement = null;
    }
}