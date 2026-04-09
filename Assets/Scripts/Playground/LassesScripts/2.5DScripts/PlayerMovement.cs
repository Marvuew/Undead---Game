using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement2D : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Control")]
    [Tooltip("If false, the player cannot move at all.")]
    [SerializeField] private bool movementEnabled = true;

    private Rigidbody2D rb;
    private Vector2 input;
    private bool wasMovingLastFrame;
    private int movementLockCount = 0;

    public bool MovementEnabled => movementEnabled;
    public bool IsLocked => movementLockCount > 0;
    public bool CanMove => movementEnabled && !IsLocked;
    public bool IsMoving => CanMove && input.sqrMagnitude > 0.0001f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        UpdateClockMovementState(force: true);
    }

    private void Update()
    {
        if (CanMove)
        {
            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");
            input = input.normalized;
        }
        else
        {
            input = Vector2.zero;
        }

        if (spriteRenderer != null && input.x != 0f)
            spriteRenderer.flipX = input.x < 0f;

        UpdateClockMovementState();
    }

    private void FixedUpdate()
    {
        if (rb == null)
            return;

        rb.linearVelocity = CanMove ? input * moveSpeed : Vector2.zero;
    }

    public void SetMovementEnabled(bool enabled)
    {
        movementEnabled = enabled;

        if (!CanMove)
        {
            input = Vector2.zero;

            if (rb != null)
                rb.linearVelocity = Vector2.zero;
        }

        UpdateClockMovementState(force: true);
    }

    public void LockMovement()
    {
        movementLockCount++;
        input = Vector2.zero;

        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        UpdateClockMovementState(force: true);
    }

    public void UnlockMovement()
    {
        movementLockCount = Mathf.Max(0, movementLockCount - 1);

        if (!CanMove)
        {
            input = Vector2.zero;

            if (rb != null)
                rb.linearVelocity = Vector2.zero;
        }

        UpdateClockMovementState(force: true);
    }

    public void ForceStop()
    {
        input = Vector2.zero;

        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        UpdateClockMovementState(force: true);
    }

    public Rigidbody2D GetRigidbody2D()
    {
        return rb;
    }

    public void SetFacingDirection(Vector2 direction)
    {
        if (spriteRenderer == null)
            return;

        if (direction.x > 0f)
            spriteRenderer.flipX = false;
        else if (direction.x < 0f)
            spriteRenderer.flipX = true;
    }

    private void UpdateClockMovementState(bool force = false)
    {
        bool isMovingNow = IsMoving;

        if (!force && isMovingNow == wasMovingLastFrame)
            return;

        wasMovingLastFrame = isMovingNow;

        if (GlobalTimeSystem.Instance != null)
            GlobalTimeSystem.Instance.SetPlayerMoving(isMovingNow);
    }

    private void OnDisable()
    {
        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        if (GlobalTimeSystem.Instance != null)
            GlobalTimeSystem.Instance.SetPlayerMoving(false);
    }
}