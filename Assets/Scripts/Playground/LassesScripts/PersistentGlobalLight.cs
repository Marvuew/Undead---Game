using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[DefaultExecutionOrder(-100)]
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

        CreatePersistentLight();
    }

    private void CreatePersistentLight()
    {
        if (globalLight != null)
            return;

        Color startupColor = lightColor;
        float startupIntensity = intensity;
        int startupBlendStyle = blendStyleIndex;

        // Try to match an already-existing scene Global Light so there is no startup flash.
        Light2D[] sceneLights = FindObjectsByType<Light2D>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        for (int i = 0; i < sceneLights.Length; i++)
        {
            Light2D candidate = sceneLights[i];

            if (candidate == null)
                continue;

            if (candidate.lightType != Light2D.LightType.Global)
                continue;

            if (candidate.transform.IsChildOf(transform))
                continue;

            startupColor = candidate.color;
            startupIntensity = candidate.intensity;
            startupBlendStyle = candidate.blendStyleIndex;
            break;
        }

        GameObject lightObj = new GameObject("PersistentGlobalLight");
        lightObj.transform.SetParent(transform, false);

        globalLight = lightObj.AddComponent<Light2D>();
        globalLight.lightType = Light2D.LightType.Global;
        globalLight.color = startupColor;
        globalLight.intensity = startupIntensity;
        globalLight.blendStyleIndex = startupBlendStyle;
        globalLight.targetSortingLayers = SortingLayer.layers.Select(layer => layer.id).ToArray();
        globalLight.enabled = true;

        // Sync stored values so future updates continue from the correct startup state.
        lightColor = startupColor;
        intensity = startupIntensity;
        blendStyleIndex = startupBlendStyle;
    }

    public void UpdateLightSettings(Color color, float newIntensity)
    {
        lightColor = color;
        intensity = newIntensity;

        if (globalLight != null)
        {
            globalLight.color = color;
            globalLight.intensity = newIntensity;
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