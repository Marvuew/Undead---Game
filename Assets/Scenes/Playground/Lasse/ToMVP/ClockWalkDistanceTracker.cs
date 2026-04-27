using UnityEngine;

public class ClockWalkDistanceTracker : MonoBehaviour
{
    [SerializeField] private float maxAllowedDistancePerFrame = 1.5f;

    private Vector3 lastPosition;
    private bool hasLastPosition;

    private void OnEnable()
    {
        ResetTrackingPosition();
    }

    private void Update()
    {
        if (ClockStateMemory.Instance == null)
            return;

        if (!hasLastPosition)
        {
            ResetTrackingPosition();
            return;
        }

        bool playerIsPressingMovement =
            Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.01f ||
            Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.01f;

        Vector3 currentPosition = transform.position;
        float distance = Vector3.Distance(currentPosition, lastPosition);

        lastPosition = currentPosition;

        if (!playerIsPressingMovement)
            return;

        if (distance <= 0.0001f)
            return;

        if (distance > maxAllowedDistancePerFrame)
            return;

        ClockStateMemory.Instance.AddWalkedDistance(distance);
    }

    public void ResetTrackingPosition()
    {
        lastPosition = transform.position;
        hasLastPosition = true;
    }
}