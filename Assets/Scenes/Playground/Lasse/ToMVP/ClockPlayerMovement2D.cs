using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ClockPlayerMovement2D : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Control")]
    [SerializeField] private bool movementEnabled = true;

    [Header("Clock Distance")]
    [SerializeField] private bool reportWalkDistanceToClock = true;

    private Rigidbody2D rb;
    private Vector2 input;
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
    }

    private void FixedUpdate()
    {
        if (rb == null)
            return;

        Vector2 velocity = CanMove ? input * moveSpeed : Vector2.zero;
        rb.linearVelocity = velocity;

        if (reportWalkDistanceToClock && IsMoving && ClockStateMemory.Instance != null)
        {
            float distanceThisFrame = velocity.magnitude * Time.fixedDeltaTime;
            ClockStateMemory.Instance.AddWalkedDistance(distanceThisFrame);
        }
    }

    public void SetMovementEnabled(bool enabled)
    {
        movementEnabled = enabled;

        if (!CanMove)
            ForceStop();
    }

    public void LockMovement()
    {
        movementLockCount++;
        ForceStop();
    }

    public void UnlockMovement()
    {
        movementLockCount = Mathf.Max(0, movementLockCount - 1);

        if (!CanMove)
            ForceStop();
    }

    public void ForceStop()
    {
        input = Vector2.zero;

        if (rb != null)
            rb.linearVelocity = Vector2.zero;
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
}