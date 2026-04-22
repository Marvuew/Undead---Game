using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightControl : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera targetCamera;
    [SerializeField] private HalfClockHand clock;

    [Header("Sky")]
    [SerializeField] private bool controlCameraBackground = true;

    [Header("Morning")]
    [SerializeField] private Color morningColor = new Color(1f, 0.95f, 0.75f);
    [SerializeField] private float morningIntensity = 1.2f;
    [SerializeField] private Color morningSky = new Color(0.6f, 0.8f, 1f);

    [Header("Sunset")]
    [SerializeField] private Color sunsetColor = new Color(1f, 0.55f, 0.25f);
    [SerializeField] private float sunsetIntensity = 0.9f;
    [SerializeField] private Color sunsetSky = new Color(1f, 0.45f, 0.2f);

    [Header("Evening")]
    [SerializeField] private Color eveningColor = new Color(0.4f, 0.5f, 0.8f);
    [SerializeField] private float eveningIntensity = 0.5f;
    [SerializeField] private Color eveningSky = new Color(0.2f, 0.3f, 0.6f);

    [Header("Night")]
    [SerializeField] private Color nightColor = new Color(0.15f, 0.2f, 0.4f);
    [SerializeField] private float nightIntensity = 0.25f;
    [SerializeField] private Color nightSky = new Color(0.05f, 0.08f, 0.15f);

    [Header("Transition Speeds")]
    //[SerializeField] private float colorSpeed = 2f; //Not used yet reduce warnings uncomment if needed
    //[SerializeField] private float intensitySpeed = 2f; //Not used yet reduce warnings uncomment if needed
    [SerializeField] private float skySpeed = 2f;

    private void Awake()
    {
        // Use the persistent global light instead of finding one in the scene
        if (PersistentGlobalLight.Instance != null)
        {
            Debug.Log("LightControl found PersistentGlobalLight");
        }
        else
        {
            Debug.LogError("LightControl: No PersistentGlobalLight found!");
        }

        if (targetCamera == null)
            targetCamera = Camera.main;

        if (clock == null)
            clock = FindFirstObjectByType<HalfClockHand>();
    }

    private void Update()
    {
        if (clock == null)
            return;

        float t = clock.NormalizedDayProgress;

        UpdateLighting(t);
    }

    private void UpdateLighting(float t)
    {
        Color targetColor;
        float targetIntensity;
        Color targetSky;

        if (t < 0.33f)
        {
            float p = t / 0.33f;
            targetColor = Color.Lerp(morningColor, sunsetColor, p);
            targetIntensity = Mathf.Lerp(morningIntensity, sunsetIntensity, p);
            targetSky = Color.Lerp(morningSky, sunsetSky, p);
        }
        else if (t < 0.66f)
        {
            float p = (t - 0.33f) / 0.33f;
            targetColor = Color.Lerp(sunsetColor, eveningColor, p);
            targetIntensity = Mathf.Lerp(sunsetIntensity, eveningIntensity, p);
            targetSky = Color.Lerp(sunsetSky, eveningSky, p);
        }
        else
        {
            float p = (t - 0.66f) / 0.34f;
            targetColor = Color.Lerp(eveningColor, nightColor, p);
            targetIntensity = Mathf.Lerp(eveningIntensity, nightIntensity, p);
            targetSky = Color.Lerp(eveningSky, nightSky, p);
        }

        // Update the persistent global light
        if (PersistentGlobalLight.Instance != null)
        {
            PersistentGlobalLight.Instance.UpdateLightSettings(targetColor, targetIntensity);
        }

        if (controlCameraBackground && targetCamera != null)
        {
            targetCamera.backgroundColor = Color.Lerp(
                targetCamera.backgroundColor,
                targetSky,
                Time.deltaTime * skySpeed
            );
        }
    }
}