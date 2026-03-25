using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DayProgressLightingController : MonoBehaviour
{
    [Header("Clock Source")]
    [SerializeField] private HalfClockHand clockHand;

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

    [Header("Fire Flicker")]
    [SerializeField] private bool enableFireFlicker = true;
    [SerializeField] private Color fireYellow = new Color(1f, 0.9f, 0.25f, 1f);
    [SerializeField] private Color fireOrange = new Color(1f, 0.45f, 0.05f, 1f);
    [SerializeField] private Color fireRed = new Color(0.95f, 0.12f, 0.04f, 1f);
    [SerializeField] private float fireColorFlickerSpeed = 6f;
    [SerializeField] private float fireIntensityFlickerSpeed = 9f;

    [Range(0f, 1f)]
    [SerializeField] private float fireIntensityVariation = 0.22f;

    [SerializeField] private bool randomizeEachLight = true;

    private float[] base2DIntensities;
    private float[] base3DIntensities;
    private Color[] base2DColors;
    private Color[] base3DColors;
    private float[] seed2D;
    private float[] seed3D;

    private void Awake()
    {
        if (clockHand == null)
            clockHand = FindFirstObjectByType<HalfClockHand>();

        if (globalLight == null)
            globalLight = FindFirstObjectByType<Light2D>();

        if (targetCamera == null)
            targetCamera = Camera.main;

        CacheLateLightBaseData();
        BuildFlickerSeeds();

        if (applyInstantlyOnStart)
            ApplyInstant();
    }

    private void Update()
    {
        if (clockHand == null)
            return;

        float progress = clockHand.NormalizedDayProgress;

        UpdateGlobalLight(progress);
        UpdateSkyColor(progress);
        UpdateLateLights(progress);
    }

    private void ApplyInstant()
    {
        if (clockHand == null)
            return;

        float progress = clockHand.NormalizedDayProgress;

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
        {
            float t = progress / 0.33f;
            return Color.Lerp(morningLightColor, noonLightColor, t);
        }

        if (progress <= 0.66f)
        {
            float t = (progress - 0.33f) / 0.33f;
            return Color.Lerp(noonLightColor, sunsetLightColor, t);
        }

        float u = (progress - 0.66f) / 0.34f;
        return Color.Lerp(sunsetLightColor, eveningLightColor, u);
    }

    private float EvaluateLightIntensity(float progress)
    {
        if (progress <= 0.33f)
        {
            float t = progress / 0.33f;
            return Mathf.Lerp(morningLightIntensity, noonLightIntensity, t);
        }

        if (progress <= 0.66f)
        {
            float t = (progress - 0.33f) / 0.33f;
            return Mathf.Lerp(noonLightIntensity, sunsetLightIntensity, t);
        }

        float u = (progress - 0.66f) / 0.34f;
        return Mathf.Lerp(sunsetLightIntensity, eveningLightIntensity, u);
    }

    private Color EvaluateSkyColor(float progress)
    {
        if (progress <= 0.33f)
        {
            float t = progress / 0.33f;
            return Color.Lerp(morningSkyColor, noonSkyColor, t);
        }

        if (progress <= 0.66f)
        {
            float t = (progress - 0.33f) / 0.33f;
            return Color.Lerp(noonSkyColor, sunsetSkyColor, t);
        }

        float u = (progress - 0.66f) / 0.34f;
        return Color.Lerp(sunsetSkyColor, eveningSkyColor, u);
    }

    private void CacheLateLightBaseData()
    {
        if (late2DLights != null)
        {
            base2DIntensities = new float[late2DLights.Length];
            base2DColors = new Color[late2DLights.Length];

            for (int i = 0; i < late2DLights.Length; i++)
            {
                if (late2DLights[i] == null)
                    continue;

                base2DIntensities[i] = late2DLights[i].intensity;
                base2DColors[i] = late2DLights[i].color;
                late2DLights[i].intensity = 0f;
            }
        }

        if (late3DLights != null)
        {
            base3DIntensities = new float[late3DLights.Length];
            base3DColors = new Color[late3DLights.Length];

            for (int i = 0; i < late3DLights.Length; i++)
            {
                if (late3DLights[i] == null)
                    continue;

                base3DIntensities[i] = late3DLights[i].intensity;
                base3DColors[i] = late3DLights[i].color;
                late3DLights[i].intensity = 0f;
            }
        }
    }

    private void BuildFlickerSeeds()
    {
        if (late2DLights != null)
        {
            seed2D = new float[late2DLights.Length];
            for (int i = 0; i < late2DLights.Length; i++)
                seed2D[i] = randomizeEachLight ? Random.Range(0f, 1000f) : 0f;
        }

        if (late3DLights != null)
        {
            seed3D = new float[late3DLights.Length];
            for (int i = 0; i < late3DLights.Length; i++)
                seed3D[i] = randomizeEachLight ? Random.Range(0f, 1000f) : 0f;
        }
    }

    private void UpdateLateLights(float progress)
    {
        float lateBlend = Mathf.InverseLerp(lateLightsStartProgress, 1f, progress);

        if (late2DLights != null)
        {
            for (int i = 0; i < late2DLights.Length; i++)
            {
                Light2D light = late2DLights[i];
                if (light == null)
                    continue;

                float baseIntensity = Get2DBaseIntensity(i) * lateBlend;
                float flickerIntensity = enableFireFlicker
                    ? ApplyFireIntensityFlicker(baseIntensity, GetSeed2D(i))
                    : baseIntensity;

                light.intensity = Mathf.Lerp(light.intensity, flickerIntensity, Time.deltaTime * lateLightsFadeSpeed);

                Color targetColor = enableFireFlicker && lateBlend > 0f
                    ? GetFireFlickerColor(GetSeed2D(i))
                    : Get2DBaseColor(i);

                light.color = Color.Lerp(light.color, targetColor, Time.deltaTime * fireColorFlickerSpeed);
            }
        }

        if (late3DLights != null)
        {
            for (int i = 0; i < late3DLights.Length; i++)
            {
                Light light = late3DLights[i];
                if (light == null)
                    continue;

                float baseIntensity = Get3DBaseIntensity(i) * lateBlend;
                float flickerIntensity = enableFireFlicker
                    ? ApplyFireIntensityFlicker(baseIntensity, GetSeed3D(i))
                    : baseIntensity;

                light.intensity = Mathf.Lerp(light.intensity, flickerIntensity, Time.deltaTime * lateLightsFadeSpeed);

                Color targetColor = enableFireFlicker && lateBlend > 0f
                    ? GetFireFlickerColor(GetSeed3D(i))
                    : Get3DBaseColor(i);

                light.color = Color.Lerp(light.color, targetColor, Time.deltaTime * fireColorFlickerSpeed);
            }
        }
    }

    private float ApplyFireIntensityFlicker(float baseIntensity, float seed)
    {
        float t = Time.time * fireIntensityFlickerSpeed + seed;

        float noiseA = Mathf.PerlinNoise(t, 0.123f);
        float noiseB = Mathf.PerlinNoise(0.456f, t * 0.73f);
        float noise = Mathf.Lerp(noiseA, noiseB, 0.5f);

        float multiplier = Mathf.Lerp(1f - fireIntensityVariation, 1f, noise);
        return baseIntensity * multiplier;
    }

    private Color GetFireFlickerColor(float seed)
    {
        float t = Time.time * fireColorFlickerSpeed + seed;

        float a = Mathf.PerlinNoise(t, 1.731f);
        float b = Mathf.PerlinNoise(4.921f, t * 0.61f);
        float mix = Mathf.Lerp(a, b, 0.5f);

        if (mix < 0.4f)
        {
            float p = Mathf.InverseLerp(0f, 0.4f, mix);
            return Color.Lerp(fireRed, fireOrange, p);
        }

        float q = Mathf.InverseLerp(0.4f, 1f, mix);
        return Color.Lerp(fireOrange, fireYellow, q);
    }

    private float Get2DBaseIntensity(int index)
    {
        if (base2DIntensities == null || index < 0 || index >= base2DIntensities.Length)
            return 1f;

        return base2DIntensities[index];
    }

    private float Get3DBaseIntensity(int index)
    {
        if (base3DIntensities == null || index < 0 || index >= base3DIntensities.Length)
            return 1f;

        return base3DIntensities[index];
    }

    private Color Get2DBaseColor(int index)
    {
        if (base2DColors == null || index < 0 || index >= base2DColors.Length)
            return Color.white;

        return base2DColors[index];
    }

    private Color Get3DBaseColor(int index)
    {
        if (base3DColors == null || index < 0 || index >= base3DColors.Length)
            return Color.white;

        return base3DColors[index];
    }

    private float GetSeed2D(int index)
    {
        if (seed2D == null || index < 0 || index >= seed2D.Length)
            return 0f;

        return seed2D[index];
    }

    private float GetSeed3D(int index)
    {
        if (seed3D == null || index < 0 || index >= seed3D.Length)
            return 0f;

        return seed3D[index];
    }
}