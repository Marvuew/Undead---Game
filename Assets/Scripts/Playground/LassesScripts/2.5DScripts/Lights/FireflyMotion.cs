using UnityEngine;

public class FireflyMotion : MonoBehaviour
{
    public float radius = 2f;       // Max distance from origin
    public float speed = 0.5f;      // Overall movement speed
    public float noiseScale = 0.8f; // How "tight" or "loose" motion is
    public float verticalAmount = 0.5f; // Reduce if you want less up/down

    private Vector3 origin;
    private Vector3 noiseOffset;

    void Start()
    {
        origin = transform.position;

        // Random starting point so multiple fireflies don't sync
        noiseOffset = new Vector3(
            Random.value * 100f,
            Random.value * 100f,
            Random.value * 100f
        );
    }

    void Update()
    {
        float time = Time.time * speed;

        // Smooth Perlin noise for each axis
        float x = Mathf.PerlinNoise(noiseOffset.x, time) - 0.5f;
        float y = (Mathf.PerlinNoise(noiseOffset.y, time) - 0.5f) * verticalAmount;
        float z = Mathf.PerlinNoise(noiseOffset.z, time) - 0.5f;

        Vector3 offset = new Vector3(x, y, z) * radius * 2f;

        transform.position = origin + offset;
    }
}