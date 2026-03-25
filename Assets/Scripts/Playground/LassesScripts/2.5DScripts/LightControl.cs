using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ClockPhaseLightingController : MonoBehaviour
{
    [Header("Main Global Light 2D")]
    [SerializeField] private Light2D globalLight;

    [Header("Camera / Sky Color")]
    [SerializeField] private Camera targetCamera;
    [SerializeField] private bool controlCameraBackground = true;

    [Header("Phase 0 - Morning")]
    [SerializeField] private Color morningLightColor = new Color(1f, 0.98f, 0.92f, 1f);
    [SerializeField] private float morningLightIntensity = 1.1f;
    [SerializeField] private Color morningSkyColor = new Color(0.72f, 0.84f, 1f, 1f);

    [Header("Phase 1 - Sunset (after 3 conversations)")]
    [SerializeField] private Color sunsetLightColor = new Color(1f, 0.72f, 0.45f, 1f);
    [SerializeField] private float sunsetLightIntensity = 0.85f;
    [SerializeField] private Color sunsetSkyColor = new Color(1f, 0.58f, 0.36f, 1f);

    [Header("Phase 2 - Evening (after 6 conversations)")]
    [SerializeField] private Color eveningLightColor = new Color(0.45f, 0.52f, 0.75f, 1f);
    [SerializeField] private float eveningLightIntensity = 0.45f;
    [SerializeField] private Color eveningSkyColor = new Color(0.23f, 0.32f, 0.52f, 1f);

    [Header("Phase 3 - Late Night / End")]
    [SerializeField] private Color lateNightLightColor = new Color(0.22f, 0.28f, 0.45f, 1f);
    [SerializeField] private float lateNightLightIntensity = 0.25f;
    [SerializeField] private Color lateNightSkyColor = new Color(0.05f, 0.08f, 0.16f, 1f);

    [Header("Transition")]
    [SerializeField] private float colorLerpSpeed = 1.5f;
    [SerializeField] private float intensityLerpSpeed = 1.5f;
    [SerializeField] private float skyColorLerpSpeed = 1.25f;
    [SerializeField] private bool applyInstantlyOnStart = true;

    [Header("Lights That Turn On Late")]
    [Tooltip("These will turn on once evening starts (phase 2 and beyond).")]
    [SerializeField] private Light2D[] late2DLights;

    [Tooltip("Optional 3D lights that should also turn on once evening starts.")]
    [SerializeField] private Light[] late3DLights;

    [Header("Late Light Fade")]
    [SerializeField] private bool fadeLateLights = true;
    [SerializeField] private float lateLightFadeSpeed = 2f;

    [Tooltip("If above 0, all late 2D lights fade to this intensity. If 0, each keeps its original inspector intensity.")]
    [SerializeField] private float late2DLightTargetIntensity = 0f;

    [Tooltip("If above 0, all late 3D lights fade to this intensity. If 0, each keeps its original inspector intensity.")]
    [SerializeField] private float late3DLightTargetIntensity = 0f;

    [Header("Fire Flicker")]
    [SerializeField] private bool enableFireFlicker = true;

    [Tooltip("Only works on late lights after evening starts.")]
    [SerializeField] private Color fireYellow = new Color(1f, 0.82f, 0.28f, 1f);

    [SerializeField] private Color fireOrange = new Color(1f, 0.45f, 0.08f, 1f);
    [SerializeField] private Color fireRed = new Color(0.9f, 0.16f, 0.05f, 1f);

    [Tooltip("How quickly the light color shifts.")]
    [SerializeField] private float fireColorFlickerSpeed = 6f;

    [Tooltip("How quickly the light intensity flickers.")]
    [SerializeField] private float fireIntensityFlickerSpeed = 9f;

    [Tooltip("How strong the flicker is. 0.15 = subtle, 0.4 = dramatic.")]
    [Range(0f, 1f)]
    [SerializeField] private float fireIntensityVariation = 0.22f;

    [Tooltip("If true, each light gets a slightly different flicker pattern.")]
    [SerializeField] private bool randomizeEachLight = true;

    private Color targetLightColor;
    private float targetLightIntensity;
    private Color targetSkyColor;
    private int lastKnownPhase = -1;

    private float[] base2DIntensities;
    private float[] base3DIntensities;

    private float[] seed2D;
    private float[] seed3D;

    private void Awake()
    {
        if (globalLight == null)
            globalLight = FindFirstObjectByType<Light2D>();

        if (targetCamera == null)
            targetCamera = Camera.main;

        CacheLateLightBaseIntensities();
        BuildFlickerSeeds();

        int startPhase = GetCurrentClockPhase();
        ApplyPhaseTargets(startPhase);

        if (applyInstantlyOnStart)
        {
            if (globalLight != null)
            {
                globalLight.color = targetLightColor;
                globalLight.intensity = targetLightIntensity;
            }

            if (controlCameraBackground && targetCamera != null)
                targetCamera.backgroundColor = targetSkyColor;
        }

        ApplyLateLightStateInstant(startPhase);
        lastKnownPhase = startPhase;
    }

    private void Update()
    {
        int currentPhase = GetCurrentClockPhase();

        if (currentPhase != lastKnownPhase)
        {
            ApplyPhaseTargets(currentPhase);
            lastKnownPhase = currentPhase;
        }

        UpdateGlobalLight();
        UpdateSkyColor();
        UpdateLateLights();
    }

    private int GetCurrentClockPhase()
    {
        if (TickClockSystem.Instance == null)
            return 0;

        return TickClockSystem.Instance.CurrentPhase;
    }

    private void ApplyPhaseTargets(int phase)
    {
        switch (phase)
        {
            default:
            case 0:
                targetLightColor = morningLightColor;
                targetLightIntensity = morningLightIntensity;
                targetSkyColor = morningSkyColor;
                break;

            case 1:
                targetLightColor = sunsetLightColor;
                targetLightIntensity = sunsetLightIntensity;
                targetSkyColor = sunsetSkyColor;
                break;

            case 2:
                targetLightColor = eveningLightColor;
                targetLightIntensity = eveningLightIntensity;
                targetSkyColor = eveningSkyColor;
                break;

            case 3:
                targetLightColor = lateNightLightColor;
                targetLightIntensity = lateNightLightIntensity;
                targetSkyColor = lateNightSkyColor;
                break;
        }
    }

    private void UpdateGlobalLight()
    {
        if (globalLight == null)
            return;

        globalLight.color = Color.Lerp(
            globalLight.color,
            targetLightColor,
            Time.deltaTime * colorLerpSpeed
        );

        globalLight.intensity = Mathf.Lerp(
            globalLight.intensity,
            targetLightIntensity,
            Time.deltaTime * intensityLerpSpeed
        );
    }

    private void UpdateSkyColor()
    {
        if (!controlCameraBackground || targetCamera == null)
            return;

        targetCamera.backgroundColor = Color.Lerp(
            targetCamera.backgroundColor,
            targetSkyColor,
            Time.deltaTime * skyColorLerpSpeed
        );
    }

    private void CacheLateLightBaseIntensities()
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
                late2DLights[i].enabled = true;
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
                late3DLights[i].enabled = true;
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

    private void ApplyLateLightStateInstant(int phase)
    {
        bool shouldBeOn = phase >= 2;

        if (late2DLights != null)
        {
            for (int i = 0; i < late2DLights.Length; i++)
            {
                if (late2DLights[i] == null)
                    continue;

                late2DLights[i].intensity = shouldBeOn ? Get2DTargetIntensity(i) : 0f;

                if (enableFireFlicker && shouldBeOn)
                    late2DLights[i].color = fireOrange;
            }
        }

        if (late3DLights != null)
        {
            for (int i = 0; i < late3DLights.Length; i++)
            {
                if (late3DLights[i] == null)
                    continue;

                late3DLights[i].intensity = shouldBeOn ? Get3DTargetIntensity(i) : 0f;

                if (enableFireFlicker && shouldBeOn)
                    late3DLights[i].color = fireOrange;
            }
        }
    }

    private void UpdateLateLights()
    {
        bool shouldBeOn = GetCurrentClockPhase() >= 2;

        if (late2DLights != null)
        {
            for (int i = 0; i < late2DLights.Length; i++)
            {
                if (late2DLights[i] == null)
                    continue;

                float baseTarget = shouldBeOn ? Get2DTargetIntensity(i) : 0f;
                float finalTarget = shouldBeOn ? ApplyFireIntensityFlicker(baseTarget, GetSeed2D(i)) : 0f;

                if (fadeLateLights)
                {
                    late2DLights[i].intensity = Mathf.Lerp(
                        late2DLights[i].intensity,
                        finalTarget,
                        Time.deltaTime * lateLightFadeSpeed
                    );
                }
                else
                {
                    late2DLights[i].intensity = finalTarget;
                }

                if (enableFireFlicker && shouldBeOn)
                {
                    Color flickerColor = GetFireFlickerColor(GetSeed2D(i));
                    late2DLights[i].color = Color.Lerp(
                        late2DLights[i].color,
                        flickerColor,
                        Time.deltaTime * fireColorFlickerSpeed
                    );
                }
            }
        }

        if (late3DLights != null)
        {
            for (int i = 0; i < late3DLights.Length; i++)
            {
                if (late3DLights[i] == null)
                    continue;

                float baseTarget = shouldBeOn ? Get3DTargetIntensity(i) : 0f;
                float finalTarget = shouldBeOn ? ApplyFireIntensityFlicker(baseTarget, GetSeed3D(i)) : 0f;

                if (fadeLateLights)
                {
                    late3DLights[i].intensity = Mathf.Lerp(
                        late3DLights[i].intensity,
                        finalTarget,
                        Time.deltaTime * lateLightFadeSpeed
                    );
                }
                else
                {
                    late3DLights[i].intensity = finalTarget;
                }

                if (enableFireFlicker && shouldBeOn)
                {
                    Color flickerColor = GetFireFlickerColor(GetSeed3D(i));
                    late3DLights[i].color = Color.Lerp(
                        late3DLights[i].color,
                        flickerColor,
                        Time.deltaTime * fireColorFlickerSpeed
                    );
                }
            }
        }
    }

    private float ApplyFireIntensityFlicker(float baseIntensity, float seed)
    {
        if (!enableFireFlicker)
            return baseIntensity;

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
        else
        {
            float p = Mathf.InverseLerp(0.4f, 1f, mix);
            return Color.Lerp(fireOrange, fireYellow, p);
        }
    }

    private float Get2DTargetIntensity(int index)
    {
        if (late2DLightTargetIntensity > 0f)
            return late2DLightTargetIntensity;

        if (base2DIntensities == null || index < 0 || index >= base2DIntensities.Length)
            return 1f;

        return base2DIntensities[index];
    }

    private float Get3DTargetIntensity(int index)
    {
        if (late3DLightTargetIntensity > 0f)
            return late3DLightTargetIntensity;

        if (base3DIntensities == null || index < 0 || index >= base3DIntensities.Length)
            return 1f;

        return base3DIntensities[index];
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