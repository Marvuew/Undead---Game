using UnityEngine;
using UnityEngine.InputSystem;

public class MousePos : MonoBehaviour
{
    Vector3 screenPos;
    Vector3 worldPos;

    public float DistanceFromNearClipPlane = 1f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameObject.GetComponent<MousePos>().enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        screenPos = Mouse.current.position.ReadValue();
        screenPos.z = Camera.main.nearClipPlane + DistanceFromNearClipPlane;
        worldPos = Camera.main.ScreenToWorldPoint(screenPos);

        transform.position = worldPos;
    }
}
