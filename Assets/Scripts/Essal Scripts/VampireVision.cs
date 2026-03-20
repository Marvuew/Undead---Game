using UnityEngine;

public class VampireVision : MonoBehaviour
{
    public Material material;

    void Update()
    {
        Vector2 mouse = Input.mousePosition;
        Vector2 normalized = new Vector2(mouse.x / Screen.width, mouse.y / Screen.height);

        material.SetVector("_MousePos", normalized);

        Debug.Log("Mouse moving: " + normalized);
    }
}
