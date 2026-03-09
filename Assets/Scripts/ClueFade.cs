using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(TMP_Text))]
public class ClueRevealFade : MonoBehaviour
{
    [Header("Clue identity (must match dialogue script)")]
    [SerializeField] private string clueId = "cellar_wine_bottle";

    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 1.5f;

    [Tooltip("Use unscaled time so fade works when Time.timeScale = 0.")]
    [SerializeField] private bool useUnscaledTime = true;

    [Header("Visibility")]
    [Tooltip("If true, object stays invisible until unlocked.")]
    [SerializeField] private bool hideWhenLocked = true;

    [Tooltip("If true, object fades the first time it is revealed in the book, then stays visible permanently.")]
    [SerializeField] private bool rememberRevealState = true;

    [Header("Optional reliability mode")]
    [Tooltip("If true, while enabled this object will automatically check whether its clue became unlocked.")]
    [SerializeField] private bool autoRefreshWhileEnabled = false;

    [Tooltip("How often to check for unlock changes.")]
    [SerializeField] private float autoRefreshInterval = 0.25f;

    [Header("Debug")]
    [SerializeField] private bool enableShortcutReset = true;

    private TMP_Text tmpText;
    private Coroutine fadeRoutine;
    private Coroutine pollRoutine;

    private string RevealedKey => $"ClueRevealedInBook_{clueId}";

    private void Awake()
    {
        tmpText = GetComponent<TMP_Text>();

        if (tmpText == null)
        {
            Debug.LogError($"ClueRevealFade on '{gameObject.name}' could not find TMP_Text.");
            return;
        }

        if (hideWhenLocked)
            SetAlphaInstant(0f);
    }

    private void OnEnable()
    {
        RefreshImmediateOrFade();

        if (autoRefreshWhileEnabled)
            pollRoutine = StartCoroutine(PollUnlockRoutine());
    }

    private void OnDisable()
    {
        StopFadeIfRunning();

        if (pollRoutine != null)
        {
            StopCoroutine(pollRoutine);
            pollRoutine = null;
        }
    }

    private void Update()
    {
        if (!enableShortcutReset || Keyboard.current == null)
            return;

        bool shift = Keyboard.current.leftShiftKey.isPressed ||
                     Keyboard.current.rightShiftKey.isPressed;

        if (shift && Keyboard.current.enterKey.wasPressedThisFrame)
        {
            DebugResetRevealOnly();
            Refresh();
            Debug.Log($"Reset reveal state for clue '{clueId}' via Shift+Enter.");
        }
    }

    /// <summary>
    /// Call this when opening the book or switching to a page with clues.
    /// </summary>
    public void Refresh()
    {
        RefreshImmediateOrFade();
    }

    private IEnumerator PollUnlockRoutine()
    {
        while (true)
        {
            RefreshImmediateOrFade();

            float t = 0f;
            while (t < autoRefreshInterval)
            {
                t += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                yield return null;
            }
        }
    }

    private void RefreshImmediateOrFade()
    {
        if (tmpText == null)
            return;

        bool unlocked = ClueSaveSystem.IsUnlocked(clueId);
        bool alreadyRevealed = rememberRevealState && PlayerPrefs.GetInt(RevealedKey, 0) == 1;

        if (!unlocked)
        {
            StopFadeIfRunning();

            if (hideWhenLocked)
                SetAlphaInstant(0f);

            return;
        }

        if (!rememberRevealState)
        {
            StopFadeIfRunning();
            SetAlphaInstant(1f);
            return;
        }

        if (alreadyRevealed)
        {
            StopFadeIfRunning();
            SetAlphaInstant(1f);
            return;
        }

        if (fadeRoutine == null)
        {
            SetAlphaInstant(0f);
            fadeRoutine = StartCoroutine(FadeInThenMarkRevealed());
        }
    }

    private IEnumerator FadeInThenMarkRevealed()
    {
        yield return null;

        float t = 0f;
        float startAlpha = tmpText.color.a;

        while (t < fadeDuration)
        {
            float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            t += dt;

            float k = Mathf.Clamp01(t / fadeDuration);
            SetAlphaInstant(Mathf.Lerp(startAlpha, 1f, k));
            yield return null;
        }

        SetAlphaInstant(1f);

        if (rememberRevealState)
        {
            PlayerPrefs.SetInt(RevealedKey, 1);
            PlayerPrefs.Save();
        }

        fadeRoutine = null;
    }

    private void StopFadeIfRunning()
    {
        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
            fadeRoutine = null;
        }
    }

    private void SetAlphaInstant(float alpha)
    {
        if (tmpText == null)
            return;

        Color c = tmpText.color;
        c.a = alpha;
        tmpText.color = c;
    }

    public void DebugResetRevealOnly()
    {
        PlayerPrefs.DeleteKey(RevealedKey);
        PlayerPrefs.Save();

        StopFadeIfRunning();

        if (ClueSaveSystem.IsUnlocked(clueId))
            SetAlphaInstant(0f);
        else if (hideWhenLocked)
            SetAlphaInstant(0f);
    }

    public void DebugResetUnlockAndReveal()
    {
        ClueSaveSystem.Lock(clueId);
        PlayerPrefs.DeleteKey(RevealedKey);
        PlayerPrefs.Save();

        StopFadeIfRunning();

        if (hideWhenLocked)
            SetAlphaInstant(0f);
    }
}