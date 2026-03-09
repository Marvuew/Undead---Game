using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MessageChainDialogue : MonoBehaviour
{
    public enum CharacterAction
    {
        None,
        Enter,
        Exit
    }

    public enum EnemyAction
    {
        None,
        Enter,
        Exit,
        React
    }

    [Serializable]
    public class QuestionNode
    {
        [TextArea] public string question;
        [TextArea] public string yesReply;
        [TextArea] public string noReply;

        public int nextAfterYes = -1;
        public int nextAfterNo = -1;

        [Header("Choice button labels (optional)")]
        [Tooltip("If empty, falls back to defaultYesLabel.")]
        public string yesLabel = "";

        [Tooltip("If empty, falls back to defaultNoLabel.")]
        public string noLabel = "";

        [Header("Character actions (dropdown per choice)")]
        public CharacterAction mcOnYes = CharacterAction.None;
        public CharacterAction mcOnNo = CharacterAction.None;

        public EnemyAction enemyOnYes = EnemyAction.None;
        public EnemyAction enemyOnNo = EnemyAction.None;

        [Header("Alignment impact (optional)")]
        public bool affectsAlignment = false;

        [Tooltip("Positive number. Adds Humans (moves right) on YES.")]
        public float humansOnYes = 0f;

        [Tooltip("Positive number. Adds Undead (moves left) on YES.")]
        public float undeadOnYes = 0f;

        [Tooltip("Positive number. Adds Humans (moves right) on NO.")]
        public float humansOnNo = 0f;

        [Tooltip("Positive number. Adds Undead (moves left) on NO.")]
        public float undeadOnNo = 0f;
    }

    [Header("Start Behavior")]
    [SerializeField] private bool autoStartOnPlay = false;

    [Header("Persistence")]
    [SerializeField] private string conversationId = "";
    [SerializeField] private bool rememberCompletion = true;

    [TextArea]
    [SerializeField] private string revisitText =
        "We've already been here, maybe we should look at some other place?";

    [Header("UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private TMP_Text transcriptText;

    [SerializeField] private GameObject choicesPanel;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    [Header("Choice button label refs")]
    [SerializeField] private TMP_Text yesButtonLabel;
    [SerializeField] private TMP_Text noButtonLabel;

    [SerializeField] private string defaultYesLabel = "Yes";
    [SerializeField] private string defaultNoLabel = "No";

    [Header("Typing")]
    [SerializeField] private float charsPerSecond = 40f;
    [SerializeField] private float holdToFastTypeMultiplier = 6f;

    [Header("Flow")]
    [TextArea] [SerializeField] private string introText = "You see something on the floor.";
    [SerializeField] private QuestionNode[] nodes;

    [SerializeField] private float readDelaySeconds = 1.2f;
    [SerializeField] private Key skipDelayKey = Key.Space;

    [Header("Behavior")]
    [SerializeField] private bool clearTranscriptOnStart = true;
    [SerializeField] private bool autoScrollToBottom = true;

    [Header("End-of-dialogue actions")]
    [TextArea] [SerializeField] private string endPromptText = "Where should we go next?";
    [SerializeField] private ClueUpdatePopup cluePopup;
    [TextArea] [SerializeField] private string cluePopupMessage = "Necrolexicon - clues have been updated";

    [Header("Clue unlock")]
    [Tooltip("Unlocked only when this interaction is fully completed.")]
    [SerializeField] private string clueIdToUnlock = "";

    [Header("Alignment UI + popups")]
    [SerializeField] private AlignmentSlider alignmentSlider;
    [SerializeField] private AlignmentPointPopupSpawner alignmentPopupSpawner;

    [Header("Alignment behavior")]
    [Tooltip("ON = node only affects alignment once ever. OFF = useful for testing.")]
    [SerializeField] private bool applyAlignmentOnlyOncePerNode = true;

    [Header("Soft reset")]
    [SerializeField] private bool enableSoftReset = true;

    [Header("World character reactions")]
    [SerializeField] private DialogueCharacterWorldController characterWorld;

    private bool waitingForChoice;
    private bool isTyping;
    private bool unlockedNewClueThisRun;
    private int currentNodeIndex = -1;

    private Coroutine flowRoutine;
    private Coroutine finalWaitRoutine;

    private string CompletedKey => $"DialogueCompleted_{conversationId}";
    private string ChoiceAppliedKey(int nodeIndex) => $"ChoiceApplied_{conversationId}_Node_{nodeIndex}";

    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(conversationId))
            conversationId = $"{gameObject.scene.name}_{gameObject.name}";
    }

    private void Awake()
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (choicesPanel != null) choicesPanel.SetActive(false);

        if (yesButton != null) yesButton.onClick.AddListener(OnYesClicked);
        if (noButton != null) noButton.onClick.AddListener(OnNoClicked);

        if (yesButtonLabel == null && yesButton != null)
            yesButtonLabel = yesButton.GetComponentInChildren<TMP_Text>(true);

        if (noButtonLabel == null && noButton != null)
            noButtonLabel = noButton.GetComponentInChildren<TMP_Text>(true);

        ResolveAlignmentRefs();
        SetChoiceLabels(defaultYesLabel, defaultNoLabel);
    }

    private void OnEnable()
    {
        ResolveAlignmentRefs();

        if (alignmentSlider != null)
            alignmentSlider.RefreshFromSave();
    }

    private void Start()
    {
        if (autoStartOnPlay)
            StartConversation();
    }

    private void Update()
    {
        if (!enableSoftReset) return;
        if (Keyboard.current == null) return;

        bool one = Keyboard.current.digit1Key.isPressed;
        bool two = Keyboard.current.digit2Key.isPressed;
        bool three = Keyboard.current.digit3Key.isPressed;

        if (one && two && three && Keyboard.current.digit3Key.wasPressedThisFrame)
        {
            SoftResetDialogueAndClue();
            return;
        }
    }

    /// <summary>
    /// Call this from your interactable object to start dialogue.
    /// </summary>
    public void StartConversationFromInteraction()
    {
        StartConversation();
    }

    public void StartConversation()
    {
        if (transcriptText == null || scrollRect == null)
        {
            Debug.LogError("MessageChainDialogue: Assign transcriptText and scrollRect.");
            return;
        }

        unlockedNewClueThisRun = false;

        if (finalWaitRoutine != null)
        {
            StopCoroutine(finalWaitRoutine);
            finalWaitRoutine = null;
        }

        ResolveAlignmentRefs();

        Open();
        ShowChoices(false);
        SetChoiceLabels(defaultYesLabel, defaultNoLabel);

        if (clearTranscriptOnStart)
            transcriptText.text = "";

        if (rememberCompletion && IsCompleted())
        {
            StartLine(revisitText, afterTyping: () =>
            {
                StartLine(endPromptText, afterTyping: CloseDialoguePanel, waitDelay: false);
            }, waitDelay: false);

            return;
        }

        StartLine(introText, afterTyping: () =>
        {
            if (nodes != null && nodes.Length > 0)
                GoToNode(0);
        }, waitDelay: true);
    }

    private void Open()
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(true);
    }

    private void CloseDialoguePanel()
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
    }

    private void ShowChoices(bool show)
    {
        if (choicesPanel != null) choicesPanel.SetActive(show);
        if (yesButton != null) yesButton.interactable = show;
        if (noButton != null) noButton.interactable = show;
    }

    private void SetChoiceLabels(string yes, string no)
    {
        if (yesButtonLabel != null) yesButtonLabel.text = yes;
        if (noButtonLabel != null) noButtonLabel.text = no;
    }

    private void ApplyNodeChoiceLabels(QuestionNode node)
    {
        string y = string.IsNullOrWhiteSpace(node.yesLabel) ? defaultYesLabel : node.yesLabel;
        string n = string.IsNullOrWhiteSpace(node.noLabel) ? defaultNoLabel : node.noLabel;
        SetChoiceLabels(y, n);
    }

    private void GoToNode(int index)
    {
        if (nodes == null || index < 0 || index >= nodes.Length) return;

        currentNodeIndex = index;
        waitingForChoice = false;
        ShowChoices(false);

        if (finalWaitRoutine != null)
        {
            StopCoroutine(finalWaitRoutine);
            finalWaitRoutine = null;
        }

        StartLine(nodes[currentNodeIndex].question, afterTyping: () =>
        {
            var node = nodes[currentNodeIndex];

            bool isFinalNode = node.nextAfterYes < 0 && node.nextAfterNo < 0;
            if (isFinalNode)
            {
                finalWaitRoutine = StartCoroutine(WaitForSpaceThenEndPrompt());
                return;
            }

            ApplyNodeChoiceLabels(node);
            waitingForChoice = true;
            ShowChoices(true);

        }, waitDelay: false);
    }

    public void OnYesClicked()
    {
        if (!waitingForChoice || isTyping) return;

        waitingForChoice = false;
        ShowChoices(false);

        var node = nodes[currentNodeIndex];

        ApplyCharacterActions(node, choseYes: true);
        ApplyAlignmentForChoice(currentNodeIndex, choseYes: true);

        bool isEnd = node.nextAfterYes < 0;

        StartLine(node.yesReply, afterTyping: () =>
        {
            if (!isEnd) GoToNode(node.nextAfterYes);
            else EndDialogueChain();
        }, waitDelay: !isEnd);
    }

    public void OnNoClicked()
    {
        if (!waitingForChoice || isTyping) return;

        waitingForChoice = false;
        ShowChoices(false);

        var node = nodes[currentNodeIndex];

        ApplyCharacterActions(node, choseYes: false);
        ApplyAlignmentForChoice(currentNodeIndex, choseYes: false);

        bool isEnd = node.nextAfterNo < 0;

        StartLine(node.noReply, afterTyping: () =>
        {
            if (!isEnd) GoToNode(node.nextAfterNo);
            else EndDialogueChain();
        }, waitDelay: !isEnd);
    }

    private void ApplyCharacterActions(QuestionNode node, bool choseYes)
    {
        if (characterWorld == null || node == null) return;

        CharacterAction mcAction = choseYes ? node.mcOnYes : node.mcOnNo;
        EnemyAction enemyAction = choseYes ? node.enemyOnYes : node.enemyOnNo;

        switch (mcAction)
        {
            case CharacterAction.Enter:
                characterWorld.EnterMC();
                break;
            case CharacterAction.Exit:
                characterWorld.ExitMC();
                break;
        }

        switch (enemyAction)
        {
            case EnemyAction.Enter:
                characterWorld.EnterEnemy();
                break;
            case EnemyAction.Exit:
                characterWorld.ExitEnemy();
                break;
            case EnemyAction.React:
                characterWorld.ReactAgainstHumans();
                break;
        }
    }

    private void ApplyAlignmentForChoice(int nodeIndex, bool choseYes)
    {
        if (nodes == null || nodeIndex < 0 || nodeIndex >= nodes.Length) return;

        ResolveAlignmentRefs();

        var node = nodes[nodeIndex];
        if (!node.affectsAlignment) return;

        if (applyAlignmentOnlyOncePerNode && PlayerPrefs.GetInt(ChoiceAppliedKey(nodeIndex), 0) == 1)
            return;

        float humansDelta = choseYes ? node.humansOnYes : node.humansOnNo;
        float undeadDelta = choseYes ? node.undeadOnYes : node.undeadOnNo;

        if (!Mathf.Approximately(humansDelta, 0f))
            AlignmentSave.AddHumans(humansDelta);

        if (!Mathf.Approximately(undeadDelta, 0f))
            AlignmentSave.AddUndead(undeadDelta);

        if (applyAlignmentOnlyOncePerNode)
        {
            PlayerPrefs.SetInt(ChoiceAppliedKey(nodeIndex), 1);
            PlayerPrefs.Save();
        }

        if (alignmentSlider != null)
            alignmentSlider.RefreshFromSave();

        if (alignmentPopupSpawner != null)
            alignmentPopupSpawner.ShowBoth(humansDelta, undeadDelta);
    }

    private IEnumerator WaitForSpaceThenEndPrompt()
    {
        ShowChoices(false);
        SetChoiceLabels(defaultYesLabel, defaultNoLabel);

        if (Keyboard.current != null)
        {
            while (Keyboard.current[skipDelayKey].isPressed)
                yield return null;
        }

        while (Keyboard.current == null || !Keyboard.current[skipDelayKey].wasPressedThisFrame)
            yield return null;

        MarkCompleted();
        unlockedNewClueThisRun = UnlockClueAtEnd();
        ShowCluePopupIfNeeded();

        StartLine(endPromptText, afterTyping: CloseDialoguePanel, waitDelay: false);

        finalWaitRoutine = null;
    }

    private void EndDialogueChain()
    {
        MarkCompleted();
        unlockedNewClueThisRun = UnlockClueAtEnd();
        ShowCluePopupIfNeeded();

        waitingForChoice = false;
        ShowChoices(false);
        SetChoiceLabels(defaultYesLabel, defaultNoLabel);

        StartLine(endPromptText, afterTyping: CloseDialoguePanel, waitDelay: false);
    }

    private bool UnlockClueAtEnd()
    {
        if (string.IsNullOrWhiteSpace(clueIdToUnlock))
            return false;

        return ClueSaveSystem.Unlock(clueIdToUnlock);
    }

    private void ShowCluePopupIfNeeded()
    {
        if (cluePopup != null && unlockedNewClueThisRun)
            cluePopup.Show(cluePopupMessage);
    }

    private void StartLine(string text, Action afterTyping, bool waitDelay)
    {
        if (flowRoutine != null) StopCoroutine(flowRoutine);
        flowRoutine = StartCoroutine(TypeLineRoutine(text, afterTyping, waitDelay));
    }

    private IEnumerator TypeLineRoutine(string line, Action afterTyping, bool waitDelay)
    {
        isTyping = true;

        if (!string.IsNullOrEmpty(transcriptText.text))
            transcriptText.text += "\n\n";

        int startLen = transcriptText.text.Length;
        transcriptText.text += line;
        transcriptText.maxVisibleCharacters = startLen;

        transcriptText.ForceMeshUpdate();
        int totalChars = transcriptText.textInfo.characterCount;

        for (int i = startLen; i <= totalChars; i++)
        {
            transcriptText.maxVisibleCharacters = i;

            if (autoScrollToBottom)
                ScrollToBottom();

            float speed = charsPerSecond;

            if (Keyboard.current != null && Keyboard.current[skipDelayKey].isPressed)
                speed *= holdToFastTypeMultiplier;

            float delay = 1f / Mathf.Max(1f, speed);
            yield return new WaitForSecondsRealtime(delay);
        }

        isTyping = false;

        if (waitDelay && readDelaySeconds > 0f)
        {
            float t = 0f;
            while (t < readDelaySeconds)
            {
                if (Keyboard.current != null && Keyboard.current[skipDelayKey].isPressed)
                    break;

                t += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        if (autoScrollToBottom)
            ScrollToBottom();

        afterTyping?.Invoke();
        flowRoutine = null;
    }

    private void ScrollToBottom()
    {
        if (scrollRect == null || scrollRect.content == null) return;
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }

    private void SoftResetDialogueAndClue()
    {
        if (flowRoutine != null)
        {
            StopCoroutine(flowRoutine);
            flowRoutine = null;
        }

        if (finalWaitRoutine != null)
        {
            StopCoroutine(finalWaitRoutine);
            finalWaitRoutine = null;
        }

        PlayerPrefs.DeleteKey(CompletedKey);

        if (nodes != null)
        {
            for (int i = 0; i < nodes.Length; i++)
                PlayerPrefs.DeleteKey(ChoiceAppliedKey(i));
        }

        if (!string.IsNullOrWhiteSpace(clueIdToUnlock))
            ClueSaveSystem.Lock(clueIdToUnlock);

        PlayerPrefs.Save();

        if (transcriptText != null)
            transcriptText.text = "";

        waitingForChoice = false;
        isTyping = false;
        unlockedNewClueThisRun = false;

        ShowChoices(false);
        SetChoiceLabels(defaultYesLabel, defaultNoLabel);

        Open();
        StartConversation();

        Debug.Log("Soft reset done (dialogue completion + clue unlock + choice flags).");
    }

    private void MarkCompleted()
    {
        if (!rememberCompletion) return;
        PlayerPrefs.SetInt(CompletedKey, 1);
        PlayerPrefs.Save();
    }

    private bool IsCompleted()
    {
        if (!rememberCompletion) return false;
        return PlayerPrefs.GetInt(CompletedKey, 0) == 1;
    }

    private void ResolveAlignmentRefs()
    {
        if (alignmentSlider == null)
            alignmentSlider = FindFirstObjectByType<AlignmentSlider>(FindObjectsInactive.Include);

        if (alignmentPopupSpawner == null)
            alignmentPopupSpawner = FindFirstObjectByType<AlignmentPointPopupSpawner>(FindObjectsInactive.Include);
    }
}