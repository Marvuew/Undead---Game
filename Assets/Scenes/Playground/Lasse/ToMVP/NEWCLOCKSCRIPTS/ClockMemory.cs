using System.Collections;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ClockMemory : MonoBehaviour
{
    public static ClockMemory Instance { get; private set; }

    [Header("Distance")]
    public float totalWalkedDistance = 0f;

    [Header("Day Settings")]
    [SerializeField] private float distanceForFullDay = 100f;
    [SerializeField] private bool isDayActive = true;

    [Header("End Of Day")]
    [SerializeField] private bool triggerSceneChangeAtEndOfDay = true;
    [SerializeField] private string endOfDaySceneName = "CaseScene";

    [TextArea]
    [SerializeField] private string endOfDayMessage = "Time has run out, we need to figure out who did this.";

    [SerializeField] private float fadeToBlackSeconds = 1.5f;

    [Header("Message UI")]
    [SerializeField] private CanvasGroup messagePanel;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private float messageDisplaySeconds = 2.5f;
    [SerializeField] private float messageFadeSeconds = 0.5f;
    [SerializeField] private float wordDelaySeconds = 0.12f;

    [Header("Scene Entry Delay")]
    [SerializeField] private float ignoreTimeAfterSceneLoad = 0.5f;

    private bool canCountDistance = true;
    private bool endOfDayTriggered = false;

    public float DistanceForFullDay => distanceForFullDay;
    public bool IsDayActive => isDayActive;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;

        HideMessageInstant();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SetPlayerInteracting(false);
        HideMessageInstant();
        StartCoroutine(DelayDistanceCounting());
    }

    private IEnumerator DelayDistanceCounting()
    {
        canCountDistance = false;
        yield return new WaitForSeconds(ignoreTimeAfterSceneLoad);
        canCountDistance = true;
    }

    public void AddDistance(float amount)
    {
        if (!isDayActive)
            return;

        if (!canCountDistance)
            return;

        if (amount <= 0f)
            return;

        totalWalkedDistance += amount;

        if (totalWalkedDistance >= distanceForFullDay)
        {
            totalWalkedDistance = distanceForFullDay;
            EndDay();
        }
    }

    private void EndDay()
    {
        CaseManager.Instance.TransitionToSelectScene();
        if (endOfDayTriggered)
            return;

        endOfDayTriggered = true;
        isDayActive = false;

        SetPlayerInteracting(true);
        StartCoroutine(EndDayRoutine());
    }

    private IEnumerator EndDayRoutine()
    {
        yield return ShowMessageRoutine();

        if (!triggerSceneChangeAtEndOfDay)
            yield break;

        if (PersistentScreenFader.Instance != null)
        {
            PersistentScreenFader.Instance.FadeToBlackAndLoadScene(
                endOfDaySceneName,
                fadeToBlackSeconds
            );
        }
        else
        {
            Debug.LogWarning("ClockMemory: No PersistentScreenFader found. Loading scene without fade.");
            SceneManager.LoadScene(endOfDaySceneName);
        }
    }

    private IEnumerator ShowMessageRoutine()
    {
        if (messagePanel == null || messageText == null)
            yield break;

        messagePanel.gameObject.SetActive(true);
        messagePanel.blocksRaycasts = true;
        messagePanel.interactable = false;
        messagePanel.alpha = 0f;

        messageText.text = "";
        messageText.alpha = 1f;

        yield return FadeMessagePanel(0f, 1f, messageFadeSeconds);

        string[] words = endOfDayMessage.Split(' ');
        string currentText = "";

        for (int i = 0; i < words.Length; i++)
        {
            currentText += (i == 0 ? "" : " ") + words[i];
            messageText.text = currentText;
            yield return new WaitForSeconds(wordDelaySeconds);
        }

        yield return new WaitForSeconds(messageDisplaySeconds);

        yield return FadeMessagePanel(1f, 0f, messageFadeSeconds);

        HideMessageInstant();
    }

    private IEnumerator FadeMessagePanel(float from, float to, float duration)
    {
        if (messagePanel == null)
            yield break;

        float safeDuration = Mathf.Max(0.01f, duration);
        float t = 0f;

        while (t < safeDuration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / safeDuration);
            messagePanel.alpha = Mathf.Lerp(from, to, p);
            yield return null;
        }

        messagePanel.alpha = to;
    }

    private void HideMessageInstant()
    {
        if (messagePanel != null)
        {
            messagePanel.alpha = 0f;
            messagePanel.blocksRaycasts = false;
            messagePanel.interactable = false;
        }

        if (messageText != null)
            messageText.text = "";
    }

    public float GetNormalizedProgress()
    {
        return Mathf.Clamp01(totalWalkedDistance / Mathf.Max(0.01f, distanceForFullDay));
    }

    public void StartNewDay()
    {
        totalWalkedDistance = 0f;
        isDayActive = true;
        endOfDayTriggered = false;
        SetPlayerInteracting(false);
        HideMessageInstant();
    }

    public void ResetDistance()
    {
        totalWalkedDistance = 0f;
    }

    public void StopDayProgress()
    {
        isDayActive = false;
    }

    public void ResumeDayProgress()
    {
        isDayActive = true;
    }

    private void SetPlayerInteracting(bool value)
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject == null)
            return;

        Component[] components = playerObject.GetComponents<Component>();

        foreach (Component component in components)
        {
            if (component == null)
                continue;

            FieldInfo field = component.GetType().GetField(
                "interacting",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
            );

            if (field != null && field.FieldType == typeof(bool))
            {
                field.SetValue(component, value);
                return;
            }
        }
    }
}