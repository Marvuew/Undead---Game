using JetBrains.Annotations;
using UnityEngine;

public class FollowMouse2D : MonoBehaviour
{
    public void Update()
    {
        transform.position = Input.mousePosition;
    }
}
