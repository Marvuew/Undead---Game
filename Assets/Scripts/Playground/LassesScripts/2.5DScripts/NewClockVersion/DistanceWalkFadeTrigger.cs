using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class PersistentScreenFader : MonoBehaviour
{
    public static PersistentScreenFader Instance;

    [Header("Fade")]
    [SerializeField] private CanvasGroup fadeGroup;
    [SerializeField] private float defaultFadeOutSeconds = 1f;
    [SerializeField] private float defaultFadeInSeconds = 1f;
    [SerializeField] private bool autoFadeInAfterSceneLoad = true;

    [Header("Message")]
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private float messageDisplaySeconds = 2.5f;
    [SerializeField] private float messageFadeSeconds = 0.5f;
    [SerializeField] private float wordDelaySeconds = 0.12f;

    private Coroutine fadeRoutine;
    private bool shouldFadeInAfterLoad;

    public bool IsFading => fadeRoutine != null;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (fadeGroup != null)
        {
            fadeGroup.gameObject.SetActive(true);
            fadeGroup.alpha = 0f;
            fadeGroup.blocksRaycasts = false;
            fadeGroup.interactable = false;
        }

        if (messageText != null)
        {
            messageText.text = "";
            messageText.alpha = 0f;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!autoFadeInAfterSceneLoad || !shouldFadeInAfterLoad)
            return;

        shouldFadeInAfterLoad = false;
        FadeFromBlack(defaultFadeInSeconds);
    }

    public void SetAlphaInstant(float alpha)
    {
        if (fadeGroup == null)
            return;

        fadeGroup.gameObject.SetActive(true);
        fadeGroup.alpha = Mathf.Clamp01(alpha);
        fadeGroup.blocksRaycasts = fadeGroup.alpha > 0f;
        fadeGroup.interactable = false;
    }

    public void FadeFromBlack()
    {
        FadeFromBlack(defaultFadeInSeconds);
    }

    public void FadeFromBlack(float duration)
    {
        if (fadeGroup == null)
            return;

        StartFade(1f, 0f, duration, true);
    }

    public void FadeToBlack()
    {
        FadeToBlack(defaultFadeOutSeconds);
    }

    public void FadeToBlack(float duration)
    {
        if (fadeGroup == null)
            return;

        StartFade(fadeGroup.alpha, 1f, duration, false);
    }

    public void FadeToBlackAndLoadScene(string sceneName)
    {
        FadeToBlackAndLoadScene(sceneName, defaultFadeOutSeconds, "");
    }

    public void FadeToBlackAndLoadScene(string sceneName, float duration)
    {
        FadeToBlackAndLoadScene(sceneName, duration, "");
    }

    public void FadeToBlackAndLoadScene(string sceneName, float duration, string message)
    {
        if (fadeGroup == null)
        {
            SceneManager.LoadScene(sceneName);
            return;
        }

        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadeToBlackAndLoadRoutine(sceneName, duration, message));
    }

    private void StartFade(float from, float to, float duration, bool disableRaycastsAtEnd)
    {
        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadeRoutine(from, to, duration, disableRaycastsAtEnd));
    }

    private IEnumerator FadeToBlackAndLoadRoutine(string sceneName, float duration, string message)
    {
        yield return FadeRoutine(fadeGroup.alpha, 1f, duration, false);

        if (!string.IsNullOrWhiteSpace(message) && messageText != null)
            yield return StartCoroutine(ShowMessageRoutine(message));

        shouldFadeInAfterLoad = true;
        SceneManager.LoadScene(sceneName);
        fadeRoutine = null;
    }

    private IEnumerator ShowMessageRoutine(string message)
    {
        messageText.text = "";
        messageText.alpha = 1f;

        string[] words = message.Split(' ');
        string currentText = "";

        for (int i = 0; i < words.Length; i++)
        {
            currentText += (i == 0 ? "" : " ") + words[i];
            messageText.text = currentText;
            yield return new WaitForSeconds(wordDelaySeconds);
        }

        yield return new WaitForSeconds(messageDisplaySeconds);

        float t = 0f;
        float safeDuration = Mathf.Max(0.01f, messageFadeSeconds);

        while (t < safeDuration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / safeDuration);
            messageText.alpha = Mathf.Lerp(1f, 0f, p);
            yield return null;
        }

        messageText.alpha = 0f;
        messageText.text = "";
    }

    private IEnumerator FadeRoutine(float from, float to, float duration, bool disableRaycastsAtEnd)
    {
        if (fadeGroup == null)
            yield break;

        fadeGroup.gameObject.SetActive(true);
        fadeGroup.alpha = from;
        fadeGroup.blocksRaycasts = true;
        fadeGroup.interactable = false;

        float safeDuration = Mathf.Max(0.01f, duration);
        float t = 0f;

        while (t < safeDuration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / safeDuration);
            fadeGroup.alpha = Mathf.Lerp(from, to, p);
            yield return null;
        }

        fadeGroup.alpha = to;

        if (disableRaycastsAtEnd)
        {
            fadeGroup.blocksRaycasts = false;
            fadeGroup.interactable = false;
        }

        fadeRoutine = null;
    }
}