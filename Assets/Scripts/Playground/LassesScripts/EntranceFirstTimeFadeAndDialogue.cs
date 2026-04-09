using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class EntranceFirstTimeFadeAndDialogue : MonoBehaviour
{
    [Header("Unique ID for this entrance (must be unique across game)")]
    [SerializeField] private string entranceId = "cellar_entrance";

    [Header("Fade UI (full-screen Image on a Canvas)")]
    [SerializeField] private Image fadeImage;          // black image that covers screen
    [SerializeField] private float fadeSeconds = 1.0f; // fade from 1 -> 0 alpha
    [SerializeField] private float afterFadeDelay = 0.15f;

    [Header("Dialogue")]
    [SerializeField] private MessageChainDialogue dialogue;
    [SerializeField] private bool startDialogueAfterFade = true;

    [Header("Behavior")]
    [SerializeField] private bool onlyFirstTimeEver = true; // PlayerPrefs-based

    private string SeenKey => $"EntranceSeen_{entranceId}";

    private void Awake()
    {
        if (fadeImage == null)
        {
            Debug.LogError($"{nameof(EntranceFirstTimeFadeAndDialogue)}: Assign fadeImage (full-screen UI Image).");
            enabled = false;
            return;
        }

        // Start fully black (alpha 1) so you don't see a frame of the scene
        SetAlpha(1f);
        fadeImage.raycastTarget = true; // blocks clicks during fade
    }

    private void Start()
    {
        bool seen = PlayerPrefs.GetInt(SeenKey, 0) == 1;

        if (onlyFirstTimeEver && seen)
        {
            // Not first time: remove fade immediately
            SetAlpha(0f);
            fadeImage.raycastTarget = false;
            return;
        }

        // Mark as seen (first time triggers fade + dialogue)
        PlayerPrefs.SetInt(SeenKey, 1);
        PlayerPrefs.Save();

        StartCoroutine(FadeThenStartDialogue());
    }

    private IEnumerator FadeThenStartDialogue()
    {
        // Fade from black -> clear using unscaled time
        float t = 0f;
        while (t < fadeSeconds)
        {
            t += Time.unscaledDeltaTime;
            float a = Mathf.Lerp(1f, 0f, Mathf.Clamp01(t / fadeSeconds));
            SetAlpha(a);
            yield return null;
        }

        SetAlpha(0f);
        fadeImage.raycastTarget = false;

        if (afterFadeDelay > 0f)
        {
            float d = 0f;
            while (d < afterFadeDelay)
            {
                d += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        if (startDialogueAfterFade && dialogue != null)
        {
            // Use this if you want the “interaction-start” path
            dialogue.StartConversationFromInteraction();

            // If you prefer direct start, use:
            // dialogue.StartConversation();
        }
    }

    private void SetAlpha(float a)
    {
        Color c = fadeImage.color;
        c.a = a;
        fadeImage.color = c;
    }

    // Optional: call this from a debug button to re-test the first-time behavior
    public void DebugClearSeenFlag()
    {
        PlayerPrefs.DeleteKey(SeenKey);
        PlayerPrefs.Save();
    }
}