using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PersistentGlobalLight : MonoBehaviour
{
    public static PersistentGlobalLight Instance;

    [Header("Light Settings")]
    [SerializeField] private Color lightColor = Color.white;
    [SerializeField] private float intensity = 1f;
    [SerializeField] private int blendStyleIndex = 0;

    private Light2D globalLight;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Create the persistent light
        CreatePersistentLight();

        Debug.Log("PersistentGlobalLight created with intensity: " + intensity);
    }

    private void CreatePersistentLight()
    {
        // Create a new GameObject for the light if it doesn't exist
        if (globalLight == null)
        {
            GameObject lightObj = new GameObject("PersistentGlobalLight");
            lightObj.transform.SetParent(transform);

            globalLight = lightObj.AddComponent<Light2D>();
            globalLight.lightType = Light2D.LightType.Global;
            globalLight.color = lightColor;
            globalLight.intensity = intensity;
            globalLight.blendStyleIndex = blendStyleIndex;

            // Target all existing sorting layers explicitly
            globalLight.targetSortingLayers = SortingLayer.layers.Select(layer => layer.id).ToArray();

            // Make sure the light is enabled
            globalLight.enabled = true;

            Debug.Log($"Created persistent light: Color={lightColor}, Intensity={intensity}, BlendStyle={blendStyleIndex}, Layers={globalLight.targetSortingLayers.Length}");
        }
    }

    public void UpdateLightSettings(Color color, float newIntensity)
    {
        lightColor = color;
        intensity = newIntensity;

        if (globalLight != null)
        {
            globalLight.color = color;
            globalLight.intensity = newIntensity;
            Debug.Log($"Updated persistent light: Color={color}, Intensity={newIntensity}");
        }
        else
        {
            Debug.LogError("PersistentGlobalLight: globalLight is null!");
        }
    }

    public void SetBlendStyle(int styleIndex)
    {
        blendStyleIndex = styleIndex;
        if (globalLight != null)
        {
            globalLight.blendStyleIndex = styleIndex;
        }
    }
}