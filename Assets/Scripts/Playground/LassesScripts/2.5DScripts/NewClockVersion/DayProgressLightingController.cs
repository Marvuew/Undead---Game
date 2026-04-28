using UnityEngine;
using UnityEngine.Rendering.Universal;

[DefaultExecutionOrder(100)]
public class DayProgressLightingController : MonoBehaviour
{
    [Header("Main Light")]
    [SerializeField] private Light2D globalLight;

    [Header("Camera Background")]
    [SerializeField] private Camera targetCamera;
    [SerializeField] private bool controlCameraBackground = true;

    [Header("Vibrant Day Colors")]
    [SerializeField] private Color morningLightColor = new Color(1f, 0.94f, 0.55f, 1f);
    [SerializeField] private float morningLightIntensity = 1.2f;
    [SerializeField] private Color morningSkyColor = new Color(0.45f, 0.78f, 1f, 1f);

    [SerializeField] private Color noonLightColor = new Color(1f, 1f, 0.9f, 1f);
    [SerializeField] private float noonLightIntensity = 1.15f;
    [SerializeField] private Color noonSkyColor = new Color(0.22f, 0.68f, 1f, 1f);

    [SerializeField] private Color sunsetLightColor = new Color(1f, 0.5f, 0.18f, 1f);
    [SerializeField] private float sunsetLightIntensity = 0.95f;
    [SerializeField] private Color sunsetSkyColor = new Color(1f, 0.3f, 0.42f, 1f);

    [SerializeField] private Color eveningLightColor = new Color(0.32f, 0.38f, 1f, 1f);
    [SerializeField] private float eveningLightIntensity = 0.5f;
    [SerializeField] private Color eveningSkyColor = new Color(0.08f, 0.12f, 0.45f, 1f);

    [Header("Transition")]
    [SerializeField] private float lightColorLerpSpeed = 2f;
    [SerializeField] private float lightIntensityLerpSpeed = 2f;
    [SerializeField] private float skyColorLerpSpeed = 2f;
    [SerializeField] private bool applyInstantlyOnStart = true;

    [Header("Late Lights")]
    [SerializeField] private Light2D[] late2DLights;
    [SerializeField] private Light[] late3DLights;

    [Range(0f, 1f)]
    [SerializeField] private float lateLightsStartProgress = 0.65f;

    [SerializeField] private float lateLightsFadeSpeed = 2f;

    private float[] base2DIntensities;
    private float[] base3DIntensities;

    private void Awake()
    {
        ResolveMainLight();

        if (targetCamera == null)
            targetCamera = Camera.main;

        CacheLateLightBaseData();

        if (applyInstantlyOnStart)
            ApplyInstant();
    }

    private void Start()
    {
        ResolveMainLight();

        if (applyInstantlyOnStart)
            ApplyInstant();
    }

    private void Update()
    {
        if (ClockMemory.Instance == null)
            return;

        if (globalLight == null)
            ResolveMainLight();

        float progress = ClockMemory.Instance.GetNormalizedProgress();

        UpdateGlobalLight(progress);
        UpdateSkyColor(progress);
        UpdateLateLights(progress);
    }

    private void ResolveMainLight()
    {
        if (globalLight != null && globalLight.lightType == Light2D.LightType.Global)
            return;

        Light2D[] allLights = FindObjectsByType<Light2D>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        for (int i = 0; i < allLights.Length; i++)
        {
            if (allLights[i] != null && allLights[i].lightType == Light2D.LightType.Global)
            {
                globalLight = allLights[i];
                return;
            }
        }
    }

    private void ApplyInstant()
    {
        if (ClockMemory.Instance == null)
            return;

        float progress = ClockMemory.Instance.GetNormalizedProgress();

        Color lightColor = EvaluateLightColor(progress);
        float lightIntensity = EvaluateLightIntensity(progress);
        Color skyColor = EvaluateSkyColor(progress);

        if (globalLight != null)
        {
            globalLight.color = lightColor;
            globalLight.intensity = lightIntensity;
        }

        if (controlCameraBackground && targetCamera != null)
            targetCamera.backgroundColor = skyColor;
    }

    private void UpdateGlobalLight(float progress)
    {
        if (globalLight == null)
            return;

        Color targetColor = EvaluateLightColor(progress);
        float targetIntensity = EvaluateLightIntensity(progress);

        globalLight.color = Color.Lerp(globalLight.color, targetColor, Time.deltaTime * lightColorLerpSpeed);
        globalLight.intensity = Mathf.Lerp(globalLight.intensity, targetIntensity, Time.deltaTime * lightIntensityLerpSpeed);
    }

    private void UpdateSkyColor(float progress)
    {
        if (!controlCameraBackground || targetCamera == null)
            return;

        Color targetSky = EvaluateSkyColor(progress);
        targetCamera.backgroundColor = Color.Lerp(targetCamera.backgroundColor, targetSky, Time.deltaTime * skyColorLerpSpeed);
    }

    private Color EvaluateLightColor(float progress)
    {
        if (progress <= 0.33f)
            return Color.Lerp(morningLightColor, noonLightColor, progress / 0.33f);

        if (progress <= 0.66f)
            return Color.Lerp(noonLightColor, sunsetLightColor, (progress - 0.33f) / 0.33f);

        return Color.Lerp(sunsetLightColor, eveningLightColor, (progress - 0.66f) / 0.34f);
    }

    private float EvaluateLightIntensity(float progress)
    {
        if (progress <= 0.33f)
            return Mathf.Lerp(morningLightIntensity, noonLightIntensity, progress / 0.33f);

        if (progress <= 0.66f)
            return Mathf.Lerp(noonLightIntensity, sunsetLightIntensity, (progress - 0.33f) / 0.33f);

        return Mathf.Lerp(sunsetLightIntensity, eveningLightIntensity, (progress - 0.66f) / 0.34f);
    }

    private Color EvaluateSkyColor(float progress)
    {
        if (progress <= 0.33f)
            return Color.Lerp(morningSkyColor, noonSkyColor, progress / 0.33f);

        if (progress <= 0.66f)
            return Color.Lerp(noonSkyColor, sunsetSkyColor, (progress - 0.33f) / 0.33f);

        return Color.Lerp(sunsetSkyColor, eveningSkyColor, (progress - 0.66f) / 0.34f);
    }

    private void CacheLateLightBaseData()
    {
        if (late2DLights != null)
        {
            base2DIntensities = new float[late2DLights.Length];

            for (int i = 0; i < late2DLights.Length; i++)
            {
                if (late2DLights[i] == null)
                    continue;

                base2DIntensities[i] = late2DLights[i].intensity;
                late2DLights[i].intensity = 0f;
            }
        }

        if (late3DLights != null)
        {
            base3DIntensities = new float[late3DLights.Length];

            for (int i = 0; i < late3DLights.Length; i++)
            {
                if (late3DLights[i] == null)
                    continue;

                base3DIntensities[i] = late3DLights[i].intensity;
                late3DLights[i].intensity = 0f;
            }
        }
    }

    private void UpdateLateLights(float progress)
    {
        float lateBlend = Mathf.InverseLerp(lateLightsStartProgress, 1f, progress);

        if (late2DLights != null)
        {
            for (int i = 0; i < late2DLights.Length; i++)
            {
                if (late2DLights[i] == null)
                    continue;

                float target = base2DIntensities[i] * lateBlend;
                late2DLights[i].intensity = Mathf.Lerp(late2DLights[i].intensity, target, Time.deltaTime * lateLightsFadeSpeed);
            }
        }

        if (late3DLights != null)
        {
            for (int i = 0; i < late3DLights.Length; i++)
            {
                if (late3DLights[i] == null)
                    continue;

                float target = base3DIntensities[i] * lateBlend;
                late3DLights[i].intensity = Mathf.Lerp(late3DLights[i].intensity, target, Time.deltaTime * lateLightsFadeSpeed);
            }
        }
    }
}