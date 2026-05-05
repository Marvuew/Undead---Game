using Assets.Scripts.GameScripts;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class DoorTransition2D : MonoBehaviour
{
    [Header("Scene")]
    public SceneNames sceneName;
    public SceneNames sceneID;

    [Header("Interaction")]
    public KeyCode interactKey = KeyCode.E;
    public string playerTag = "Player";

    [Header("Door Requirement")]
    public bool requiresNecrolexicon = false;
    public RuntimeDialogueGraph missingRequirementGraph;

    [Header("Character Event Before Transition")]
    public bool useCharacterEventBeforeTransition = false;
    public bool skipCharacterEventIfIntroWasSkipped = true;
    public GameObject characterObject;
    public RuntimeDialogueGraph characterDialogueGraph;
    public float characterRevealDelay = 0.5f;

    [Header("Reveal After Character Dialogue")]
    public bool hideRevealObjectsAtStart = true;
    public GameObject[] objectsToRevealAfterCharacterDialogue;
    public string revealSoundName = "ClueFound";
    public RuntimeDialogueGraph afterCharacterLeavesGraph;
    public UnityEvent onCharacterDialogueFinished;

    [Header("Character Fade")]
    public bool fadeCharacter = true;
    public float characterAppearDuration = 0.5f;
    public float characterDisappearDuration = 0.5f;

    [Header("Door Sound")]
    public string doorOpenSoundName = "";

    [Header("Door Type")]
    public bool Enter = true;
    public bool Exit = false;

    [Header("Fake Walk After Entering Scene")]
    public Vector2 autoWalkDirection = Vector2.down;
    public float autoWalkDistance = 1.0f;

    [Header("Fade")]
    public float fadeDuration = 1.0f;
    public Color fadeColor = Color.black;

    [Header("Prompt")]
    public Vector2 promptOffset = new Vector2(0, -40);
    public int fontSize = 24;

    private bool playerInRange = false;
    private bool isTransitioning = false;

    private bool characterEventFinished = false;
    private bool waitingForCharacterDialogue = false;
    private bool waitingForAfterCharacterDialogue = false;
    private bool characterRevealInProgress = false;
    private bool characterHideInProgress = false;

    private SpriteRenderer[] characterSprites;
    private CanvasGroup[] characterCanvasGroups;

    void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    void Start()
    {
        CacheCharacterComponents();

        if (characterObject != null && useCharacterEventBeforeTransition)
        {
            SetCharacterAlpha(0f);
            characterObject.SetActive(false);
        }

        if (hideRevealObjectsAtStart)
            HideRevealObjectsAtStart();
    }

    void Update()
    {
        if (isTransitioning)
            return;

        if (characterRevealInProgress || characterHideInProgress)
            return;

        if (waitingForCharacterDialogue)
        {
            if (DialogueGraphManager.instance == null || !DialogueGraphManager.instance.isDialogueRunning)
            {
                waitingForCharacterDialogue = false;
                StartCoroutine(FinishCharacterEventRoutine());
            }

            return;
        }

        if (waitingForAfterCharacterDialogue)
        {
            if (DialogueGraphManager.instance == null || !DialogueGraphManager.instance.isDialogueRunning)
            {
                waitingForAfterCharacterDialogue = false;
            }

            return;
        }

        if (playerInRange && Input.GetKeyDown(interactKey))
        {
            TryUseDoor();
        }
    }

    private void TryUseDoor()
    {
        if (DialogueGraphManager.instance != null && DialogueGraphManager.instance.isDialogueRunning)
            return;

        if (requiresNecrolexicon && !GameProgressState.HasNecrolexicon)
        {
            StartDoorDialogue(missingRequirementGraph);
            return;
        }

        if (ShouldRunCharacterEvent())
        {
            StartCoroutine(CharacterEventRoutine());
            return;
        }

        TransitionThroughDoor();
    }

    private bool ShouldRunCharacterEvent()
    {
        if (!useCharacterEventBeforeTransition)
            return false;

        if (skipCharacterEventIfIntroWasSkipped && GameProgressState.ForceSkippedHouseIntro)
            return false;

        if (characterEventFinished)
            return false;

        if (characterDialogueGraph == null)
            return false;

        return true;
    }

    private IEnumerator CharacterEventRoutine()
    {
        characterRevealInProgress = true;
        characterEventFinished = true;

        FindObjectOfType<HouseIntroController>()?.OnDoorOpened();

        yield return new WaitForSeconds(characterRevealDelay);

        PlayDoorOpenSound();

        if (characterObject != null)
        {
            characterObject.SetActive(true);

            if (fadeCharacter)
                yield return StartCoroutine(FadeCharacter(0f, 1f, characterAppearDuration));
            else
                SetCharacterAlpha(1f);
        }

        StartDoorDialogue(characterDialogueGraph);

        waitingForCharacterDialogue = true;
        characterRevealInProgress = false;
    }

    private IEnumerator FinishCharacterEventRoutine()
    {
        characterHideInProgress = true;

        PlayDoorOpenSound();

        if (characterObject != null)
        {
            if (fadeCharacter)
                yield return StartCoroutine(FadeCharacter(1f, 0f, characterDisappearDuration));
            else
                SetCharacterAlpha(0f);

            characterObject.SetActive(false);
        }

        RevealObjectsAfterCharacterDialogue();
        PlayRevealSound();
        onCharacterDialogueFinished?.Invoke();

        if (afterCharacterLeavesGraph != null)
        {
            StartDoorDialogue(afterCharacterLeavesGraph);
            waitingForAfterCharacterDialogue = true;
        }

        characterHideInProgress = false;
    }

    private void HideRevealObjectsAtStart()
    {
        if (objectsToRevealAfterCharacterDialogue == null)
            return;

        foreach (GameObject obj in objectsToRevealAfterCharacterDialogue)
        {
            if (obj != null)
                obj.SetActive(false);
        }
    }

    private void RevealObjectsAfterCharacterDialogue()
    {
        if (objectsToRevealAfterCharacterDialogue == null)
            return;

        foreach (GameObject obj in objectsToRevealAfterCharacterDialogue)
        {
            if (obj != null)
                obj.SetActive(true);
        }
    }

    private void PlayRevealSound()
    {
        if (!string.IsNullOrWhiteSpace(revealSoundName) && AudioManager.instance != null)
            AudioManager.instance.PlaySFX(revealSoundName);
    }

    private IEnumerator FadeCharacter(float from, float to, float duration)
    {
        duration = Mathf.Max(0.01f, duration);

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / duration);
            float alpha = Mathf.Lerp(from, to, t);

            SetCharacterAlpha(alpha);

            yield return null;
        }

        SetCharacterAlpha(to);
    }

    private void CacheCharacterComponents()
    {
        if (characterObject == null)
            return;

        characterSprites = characterObject.GetComponentsInChildren<SpriteRenderer>(true);
        characterCanvasGroups = characterObject.GetComponentsInChildren<CanvasGroup>(true);
    }

    private void SetCharacterAlpha(float alpha)
    {
        if (characterSprites != null)
        {
            foreach (SpriteRenderer spriteRenderer in characterSprites)
            {
                if (spriteRenderer == null)
                    continue;

                Color color = spriteRenderer.color;
                color.a = alpha;
                spriteRenderer.color = color;
            }
        }

        if (characterCanvasGroups != null)
        {
            foreach (CanvasGroup canvasGroup in characterCanvasGroups)
            {
                if (canvasGroup == null)
                    continue;

                canvasGroup.alpha = alpha;
            }
        }
    }

    private void TransitionThroughDoor()
    {
        FindObjectOfType<HouseIntroController>()?.MarkTutorialCompleted();
        FindObjectOfType<HouseIntroController>()?.OnDoorOpened();

        PlayDoorOpenSound();

        isTransitioning = true;

        TransitionState2D.SetTransition(
            sceneName.ToString(),
            autoWalkDirection,
            autoWalkDistance
        );

        if (WorldFade.Instance != null)
        {
            WorldFade.Instance.StartSceneTransition(sceneName.ToString(), fadeDuration, fadeColor);
        }
        else
        {
            SceneManager.LoadScene(sceneName.ToString());
        }
    }

    private void PlayDoorOpenSound()
    {
        if (!string.IsNullOrWhiteSpace(doorOpenSoundName) && AudioManager.instance != null)
            AudioManager.instance.PlaySFX(doorOpenSoundName);
    }

    private void StartDoorDialogue(RuntimeDialogueGraph graph)
    {
        if (DialogueGraphManager.instance == null || graph == null)
            return;

        if (Player.Instance != null)
            Player.Instance.interacting = true;

        DialogueGraphManager.instance.gameObject.SetActive(true);

        if (DialogueGraphManager.instance.DialoguePanel != null)
            DialogueGraphManager.instance.DialoguePanel.SetActive(true);

        DialogueGraphManager.instance.StartDialogue(graph);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
            playerInRange = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
            playerInRange = false;
    }

    void OnGUI()
    {
        if (Player.Instance != null && Player.Instance.interacting)
            return;

        if (!playerInRange || isTransitioning || characterRevealInProgress || characterHideInProgress)
            return;

        string actionText = Enter ? "Enter" : Exit ? "Exit" : "Use";
        string prompt = $"Press {interactKey} to {actionText}";

        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = fontSize;
        style.alignment = TextAnchor.MiddleCenter;
        style.normal.textColor = Color.white;
        style.fontStyle = FontStyle.Bold;

        Vector2 size = style.CalcSize(new GUIContent(prompt));
        Rect rect = new Rect(
            (Screen.width - size.x) * 0.5f + promptOffset.x,
            Screen.height - 100 + promptOffset.y,
            size.x + 20,
            size.y + 10
        );

        GUI.color = new Color(0, 0, 0, 0.6f);
        GUI.Box(rect, "");
        GUI.color = Color.white;
        GUI.Label(rect, prompt, style);
    }
}