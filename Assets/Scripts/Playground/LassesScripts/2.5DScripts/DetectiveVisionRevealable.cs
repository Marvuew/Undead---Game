using System.Collections.Generic;
using UnityEngine;

public class DetectiveVisionRevealable : MonoBehaviour
{
    private static readonly List<DetectiveVisionRevealable> AllRevealables = new List<DetectiveVisionRevealable>();

    [Header("Reveal Origin")]
    [SerializeField] private Transform revealCheckPoint;

    [Header("Objects To Show / Hide")]
    [SerializeField] private GameObject[] objectsToToggle;

    [Header("Sprite Renderers To Fade")]
    [SerializeField] private SpriteRenderer[] spriteRenderersToToggle;

    [Header("Particle Systems To Fade")]
    [SerializeField] private ParticleSystem[] particleSystemsToToggle;

    [Header("Range")]
    [SerializeField] private bool useCustomRevealRadius = false;
    [SerializeField] private float customRevealRadius = 3f;

    [Header("Fade")]
    [SerializeField] private bool fadeAsVisionEnds = true;
    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] private float minVisibleAlpha = 0f;

    [Header("Optional")]
    [SerializeField] private bool hideOnStart = true;
    [SerializeField] private bool includeChildrenParticleSystems = false;
    [SerializeField] private bool includeChildrenSpriteRenderers = false;

    private readonly Dictionary<SpriteRenderer, Color> originalSpriteColors = new Dictionary<SpriteRenderer, Color>();
    private readonly Dictionary<ParticleSystem, float> originalParticleAlpha = new Dictionary<ParticleSystem, float>();

    private void Awake()
    {
        if (!AllRevealables.Contains(this))
            AllRevealables.Add(this);

        if (includeChildrenParticleSystems && (particleSystemsToToggle == null || particleSystemsToToggle.Length == 0))
            particleSystemsToToggle = GetComponentsInChildren<ParticleSystem>(true);

        if (includeChildrenSpriteRenderers && (spriteRenderersToToggle == null || spriteRenderersToToggle.Length == 0))
            spriteRenderersToToggle = GetComponentsInChildren<SpriteRenderer>(true);

        CacheOriginalVisualState();
    }

    private void OnEnable()
    {
        ApplyCurrentState();
    }

    private void OnDisable()
    {
        AllRevealables.Remove(this);
    }

    private void Start()
    {
        if (hideOnStart)
            ApplyCurrentState();
    }

    private void Update()
    {
        ApplyCurrentState();
    }

    public static void RefreshAll()
    {
        for (int i = 0; i < AllRevealables.Count; i++)
        {
            if (AllRevealables[i] != null)
                AllRevealables[i].ApplyCurrentState();
        }
    }

    private void CacheOriginalVisualState()
    {
        if (spriteRenderersToToggle != null)
        {
            foreach (SpriteRenderer sr in spriteRenderersToToggle)
            {
                if (sr != null && !originalSpriteColors.ContainsKey(sr))
                    originalSpriteColors[sr] = sr.color;
            }
        }

        if (particleSystemsToToggle != null)
        {
            foreach (ParticleSystem ps in particleSystemsToToggle)
            {
                if (ps == null || originalParticleAlpha.ContainsKey(ps))
                    continue;

                ParticleSystem.MainModule main = ps.main;
                Color c = main.startColor.color;
                originalParticleAlpha[ps] = c.a;
            }
        }
    }

    public void ApplyCurrentState()
    {
        float visibility = GetVisibilityAmount();
        ApplyVisibility(visibility);
    }

    private float GetVisibilityAmount()
    {
        if (DetectiveVision.Instance == null)
            return 0f;

        if (!DetectiveVision.Instance.AbilityActive)
            return 0f;

        Transform player = DetectiveVision.Instance.transform;
        if (player == null)
            return 0f;

        Vector3 checkPosition = revealCheckPoint != null ? revealCheckPoint.position : transform.position;

        float revealRadius = useCustomRevealRadius
            ? customRevealRadius
            : DetectiveVision.Instance.RevealRadius;

        float sqrDistance = (checkPosition - player.position).sqrMagnitude;
        if (sqrDistance > revealRadius * revealRadius)
            return 0f;

        float timeFactor = 1f;

        if (fadeAsVisionEnds)
            timeFactor = fadeCurve.Evaluate(DetectiveVision.Instance.ActiveNormalizedRemaining);

        return Mathf.Clamp01(Mathf.Lerp(minVisibleAlpha, 1f, timeFactor));
    }

    private void ApplyVisibility(float visibility)
    {
        bool shouldExist = visibility > 0.001f;

        if (objectsToToggle != null)
        {
            foreach (GameObject obj in objectsToToggle)
            {
                if (obj != null && obj.activeSelf != shouldExist)
                    obj.SetActive(shouldExist);
            }
        }

        if (spriteRenderersToToggle != null)
        {
            foreach (SpriteRenderer sr in spriteRenderersToToggle)
            {
                if (sr == null) continue;

                if (!originalSpriteColors.TryGetValue(sr, out Color baseColor))
                    baseColor = sr.color;

                sr.enabled = shouldExist;

                if (shouldExist)
                {
                    Color c = baseColor;
                    c.a = baseColor.a * visibility;
                    sr.color = c;
                }
            }
        }

        if (particleSystemsToToggle != null)
        {
            foreach (ParticleSystem ps in particleSystemsToToggle)
            {
                if (ps == null) continue;

                var emission = ps.emission;
                emission.enabled = shouldExist;

                var main = ps.main;
                Color startColor = main.startColor.color;

                float baseAlpha = originalParticleAlpha.TryGetValue(ps, out float cachedAlpha)
                    ? cachedAlpha
                    : startColor.a;

                startColor.a = baseAlpha * visibility;
                main.startColor = startColor;

                if (shouldExist)
                {
                    if (!ps.isPlaying)
                        ps.Play();
                }
                else
                {
                    if (ps.isPlaying)
                        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (DetectiveVision.Instance == null)
            return;

        float radius = useCustomRevealRadius ? customRevealRadius : DetectiveVision.Instance.RevealRadius;
        Vector3 center = revealCheckPoint != null ? revealCheckPoint.position : transform.position;

        Gizmos.color = new Color(0f, 1f, 1f, 0.35f);
        Gizmos.DrawWireSphere(center, 0.15f);
    }
}