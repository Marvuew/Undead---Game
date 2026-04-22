using UnityEngine;

public class FollowMouse2DWorldPos : MonoBehaviour
{
    Vector3 worldPos;

    void Update()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Mathf.Abs(Camera.main.transform.position.z);

        worldPos = Camera.main.ScreenToWorldPoint(mousePos);
        transform.position = worldPos;
    }
}