using Assets.Scripts.GameScripts;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WorldFade : MonoBehaviour
{
    private static WorldFade instance;
    public static WorldFade Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<WorldFade>();
                if (instance == null)
                {
                    GameObject fadeObject = new GameObject("WorldFade");
                    instance = fadeObject.AddComponent<WorldFade>();
                }
            }
            return instance;
        }
    }

    [Header("Default Fade Settings")]
    public float defaultFadeDuration = 1.0f;
    public Color defaultFadeColor = Color.black;

    private static Texture2D fadeTexture;
    private float fadeAlpha = 0f;
    private bool isFading = false;
    private bool isSceneTransitioning = false;
    private Color currentFadeColor = Color.black;
    public bool isSceneTransitioning2 = false; // Added to prevent the fade from triggering twice when loading a scene that also uses WorldFade for its transition

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }

        if (fadeTexture == null)
        {
            fadeTexture = new Texture2D(1, 1);
            fadeTexture.SetPixel(0, 0, Color.white);
            fadeTexture.Apply();
        }
    }

    public void StartSceneTransition(string sceneName, float duration, Color color)
    {
        StartCoroutine(FadeSceneTransition(sceneName, duration, color));
    }

    public void StartSceneTransitionAndStayBlack(string sceneName, float duration, Color color)
    {
        StartCoroutine(FadeSceneTransitionAndStayBlack(sceneName, duration, color));
    }

    private IEnumerator FadeSceneTransition(string sceneName, float duration, Color color)
    {
        yield return StartCoroutine(Fade(0f, 1f, duration, color));

        isSceneTransitioning = true;
        SceneManager.LoadScene(sceneName);

        while (isSceneTransitioning)
            yield return null;

        yield return StartCoroutine(Fade(1f, 0f, duration, color));
    }

    private IEnumerator FadeSceneTransitionAndStayBlack(string sceneName, float duration, Color color)
    {
        isSceneTransitioning2 = true; // Intro uses this to time when to disable the intro UI
        yield return StartCoroutine(Fade(0f, 1f, duration, color));

        isSceneTransitioning = true;
        SceneManager.LoadScene(sceneName);

        while (isSceneTransitioning)
            yield return null;

        currentFadeColor = color;
        fadeAlpha = 1f;
        isFading = false;
        isSceneTransitioning2 = false; // Know the tutorial can begin
    }

    private IEnumerator Fade(float from, float to, float duration, Color color)
    {
        isFading = true;
        currentFadeColor = color;
        fadeAlpha = from;

        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            fadeAlpha = Mathf.Lerp(from, to, Mathf.Clamp01(timer / duration));
            yield return null;
        }

        fadeAlpha = to;
        isFading = false;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {   if (scene.name == "Home")
        {
            Player.Instance.MovePlayerToSpawnPoint();
        }
        if (!isSceneTransitioning) return;
        isSceneTransitioning = false;

        CaseManager.Instance.SetUpNewDayEnviroment();
    }

    private void OnGUI()
    {
        if (!isFading && fadeAlpha <= 0f) return;

        Color oldColor = GUI.color;
        GUI.color = new Color(currentFadeColor.r, currentFadeColor.g, currentFadeColor.b, fadeAlpha);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), fadeTexture);
        GUI.color = oldColor;
    }

    public void SetBlackScreen(Color fadeColor)
    {
        currentFadeColor = fadeColor;
        fadeAlpha = 1f;
        isFading = false;
    }

    public void StartFadeFromBlack(float fadeDuration, Color fadeColor)
    {
        StartCoroutine(Fade(1f, 0f, fadeDuration, fadeColor));
    }

    public void StartScreenFade(float fadeDuration, float stayBlackDuration, Color fadeColor)
    {
        StartCoroutine(ScreenFadeRoutine(fadeDuration, stayBlackDuration, fadeColor));
    }

    private IEnumerator ScreenFadeRoutine(float fadeDuration, float stayBlackDuration, Color fadeColor)
    {
        yield return StartCoroutine(Fade(0f, 1f, fadeDuration, fadeColor));

        yield return new WaitForSeconds(stayBlackDuration);

        yield return StartCoroutine(Fade(1f, 0f, fadeDuration, fadeColor));
    }

    public IEnumerator FadeToBlackAndBack(float fadeDuration, float stayBlackDuration, Color fadeColor)
    {
        yield return StartCoroutine(Fade(0f, 1f, fadeDuration, fadeColor));

        yield return new WaitForSeconds(stayBlackDuration);

        yield return StartCoroutine(Fade(1f, 0f, fadeDuration, fadeColor));
    }
}