using UnityEngine;

public class StayStatic : MonoBehaviour
{
    private Vector3 initialWorldPosition;

    void Start()
    {
        // Remember where I am supposed to be in the world/canvas
        initialWorldPosition = transform.position;
    }

    void LateUpdate()
    {
        // Force the clue back to its original spot every frame
        // even if the parent (Lens) moves!
        transform.position = initialWorldPosition;
    }
}