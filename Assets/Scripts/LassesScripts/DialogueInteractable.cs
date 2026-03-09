using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class DialogueInteractable : MonoBehaviour
{
    [Header("Dialogue to start")]
    [SerializeField] private MessageChainDialogue dialogue;

    [Header("Detection")]
    [SerializeField] private Camera cam;                 // if null -> Camera.main
    [SerializeField] private Collider2D targetCollider;  // if null -> GetComponent<Collider2D>()
    [SerializeField] private bool requireLeftClick = true;

    [Header("Hover highlight (2D)")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private float hoverBrightnessMultiplier = 1.25f;

    [Header("Optional")]
    [SerializeField] private bool disableAfterUse = false;

    private bool isHovering;

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
        if (cam == null || targetCollider == null) return;

        // If clicking UI, ignore (prevents conflicts with book/map buttons)
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            if (isHovering) SetHover(false);
            return;
        }

        // Mouse position -> world point
        if (Mouse.current == null) return;

        Vector2 screen = Mouse.current.position.ReadValue();
        Vector3 world = cam.ScreenToWorldPoint(new Vector3(screen.x, screen.y, 0f));
        Vector2 world2D = new Vector2(world.x, world.y);

        bool hoveringNow = targetCollider.OverlapPoint(world2D);

        if (hoveringNow != isHovering)
            SetHover(hoveringNow);

        // Click to interact
        if (!hoveringNow) return;

        bool clicked = requireLeftClick
            ? Mouse.current.leftButton.wasPressedThisFrame
            : (Mouse.current.leftButton.isPressed);

        if (clicked)
            Interact();
    }

    private void Interact()
    {
        if (dialogue == null)
        {
            Debug.LogWarning($"DialogueInteractable on {name}: dialogue reference missing.");
            return;
        }

        SetHover(false);
        dialogue.StartConversationFromInteraction();

        if (disableAfterUse)
            enabled = false;
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

        // brighten without changing alpha
        Color c = normalColor;
        c.r *= hoverBrightnessMultiplier;
        c.g *= hoverBrightnessMultiplier;
        c.b *= hoverBrightnessMultiplier;
        c.a = normalColor.a;

        spriteRenderer.color = c;
    }
}