using UnityEngine;

public class MagnifyingGlassScreenPos : MonoBehaviour
{
    [SerializeField] Material material;

    private void Update()
    {
        Vector2 screenPixels  = transform.position;

        material.SetVector("_ObjectScreenPosition", screenPixels);
    }
}
