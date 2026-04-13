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

    [SerializeField] private bool lockPlayerMovementWhileDialogueIsOpen = true;

    private PlayerMovement2D lockedPlayerMovement;

    [Serializable]
    public class QuestionNode
    {
        [TextArea] public string question;
        [TextArea] public string yesReply;
        [TextArea] public string noReply;

        public int nextAfterYes = -1;
        public int nextAfterNo = -1;

        [Header("Choice labels")]
        public string yesLabel = "";
        public string noLabel = "";

        [Header("Time cost (minutes)")]
        [Min(0)] public int minutesOnEnter = 0;
        [Min(0)] public int minutesOnYes = 0;
        [Min(0)] public int minutesOnNo = 0;

        [Header("Character actions")]
        public CharacterAction mcOnYes = CharacterAction.None;
        public CharacterAction mcOnNo = CharacterAction.None;

        public EnemyAction enemyOnYes = EnemyAction.None;
        public EnemyAction enemyOnNo = EnemyAction.None;

        [Header("Alignment impact")]
        public bool affectsAlignment = false;
        public float humansOnYes = 0f;
        public float undeadOnYes = 0f;
        public float humansOnNo = 0f;
        public float undeadOnNo = 0f;
    }

    private bool hasLockedMovementThisConversation = false;

    [Header("Start Behavior")]
    [SerializeField] private bool autoStartOnPlay = false;

    [Header("Persistence")]
    [SerializeField] private string conversationId = "";
    [SerializeField] private bool rememberCompletion = true;

    [TextArea]
    [SerializeField] private string revisitText = "We've already been here, maybe we should look at some other place?";

    [Header("UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private TMP_Text transcriptText;
    [SerializeField] private GameObject choicesPanel;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    [Header("Choice label refs")]
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

    [Header("End Of Dialogue")]
    [TextArea] [SerializeField] private string endPromptText = "Where should we go next?";
    [SerializeField] private ClueUpdatePopup cluePopup;
    [TextArea] [SerializeField] private string cluePopupMessage = "Necrolexicon - clues have been updated";

    [Header("Clue Unlock")]
    [Tooltip("Unlocked only when this interaction is fully completed.")]
    [SerializeField] private string clueIdToUnlock = "";


    [Header("Time")]
    [Tooltip("If true, the main interaction time is spent as soon as the conversation starts. If false, it is spent when the interaction finishes.")]
    [SerializeField] private bool spendInteractionMinutesOnEnter = false;

    [Tooltip("Main time cost for this interaction in minutes.")]
    [Min(0)]
    [SerializeField] private int interactionMinutesCost = 0;

    [Tooltip("If true, player must have enough remaining time before the interaction can begin.")]
    [SerializeField] private bool requireEnoughTimeToStart = true;

    [TextArea]
    [SerializeField] private string notEnoughTimeText = "It's getting too late to investigate that right now.";

    [Header("Alignment UI")]
    [SerializeField] private AlignmentSlider alignmentSlider;
    [SerializeField] private AlignmentPointPopupSpawner alignmentPopupSpawner;

    [Header("Alignment Behavior")]
    [SerializeField] private bool applyAlignmentOnlyOncePerNode = true;

    [Header("Soft Reset")]
    [SerializeField] private bool enableSoftReset = true;

    [Header("World Character Reactions")]
    [SerializeField] private DialogueCharacterWorldController characterWorld;

    private bool waitingForChoice;
    private bool isTyping;
    private bool unlockedNewClueThisRun;
    private bool interactionProtectionActive;
    private int currentNodeIndex = -1;

    private Coroutine flowRoutine;
    private Coroutine finalWaitRoutine;

    public bool IsConversationRunning { get; private set; }

    private string CompletedKey => $"DialogueCompleted_{conversationId}";
    private string ChoiceAppliedKey(int nodeIndex) => $"ChoiceApplied_{conversationId}_Node_{nodeIndex}";
    private string TickAppliedKey => $"DialogueTickApplied_{conversationId}";

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

        ResolveSceneRefs();
        SetChoiceLabels(defaultYesLabel, defaultNoLabel);
    }

    private void OnEnable()
    {
        ResolveSceneRefs();

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
        HandleSoftResetInput();
    }

    private void OnDisable()
    {
        IsConversationRunning = false;
        UnlockPlayerMovement();
        EndInteractionProtection();
    }

    public void StartConversationFromInteraction()
    {
        StartConversation();
    }

    public void StartConversation()
    {
        if (!ValidateCoreRefs())
            return;

        if (!CanBeginInteraction())
        {
            ShowNotEnoughTimeMessage();
            return;
        }

        ResetConversationRuntimeState();
        BeginInteractionProtection();

        if (spendInteractionMinutesOnEnter && !TrySpendInteractionMinutes())
        {
            EndInteractionProtection();
            return;
        }

        OpenDialogue();
        UnlockPlayerMovement();
        LockPlayerMovement();
        IsConversationRunning = true;

        if (rememberCompletion && IsCompleted())
        {
            ShowRevisitFlow();
            return;
        }

        StartIntroFlow();
    }

    public void OnYesClicked()
    {
        HandleChoice(choseYes: true);
    }

    public void OnNoClicked()
    {
        HandleChoice(choseYes: false);
    }

    private void HandleChoice(bool choseYes)
    {
        if (!waitingForChoice || isTyping || !HasValidCurrentNode())
            return;

        QuestionNode node = nodes[currentNodeIndex];
        int minutesCost = choseYes ? node.minutesOnYes : node.minutesOnNo;

        if (!TrySpendMinutes(minutesCost))
            return;

        waitingForChoice = false;
        ShowChoices(false);

        ApplyCharacterActions(node, choseYes);
        ApplyAlignmentForChoice(currentNodeIndex, choseYes);

        string reply = choseYes ? node.yesReply : node.noReply;
        int nextIndex = choseYes ? node.nextAfterYes : node.nextAfterNo;
        bool endsDialogue = nextIndex < 0;

        StartLine(reply, () =>
        {
            if (endsDialogue)
                CompleteDialogue();
            else
                GoToNode(nextIndex);
        }, waitDelay: !endsDialogue);
    }

    private void StartIntroFlow()
    {
        if (clearTranscriptOnStart && transcriptText != null)
            transcriptText.text = "";

        StartLine(introText, () =>
        {
            if (nodes != null && nodes.Length > 0)
                GoToNode(0);
        }, waitDelay: true);
    }

    private void ShowRevisitFlow()
    {
        if (clearTranscriptOnStart && transcriptText != null)
            transcriptText.text = "";

        StartLine(revisitText, () =>
        {
            StartLine(endPromptText, CloseDialogue, waitDelay: false);
        }, waitDelay: false);
    }

    private void GoToNode(int index)
    {
        if (nodes == null || index < 0 || index >= nodes.Length)
            return;

        QuestionNode node = nodes[index];

        if (!TrySpendMinutes(node.minutesOnEnter))
            return;

        currentNodeIndex = index;
        waitingForChoice = false;
        ShowChoices(false);
        StopFinalWaitRoutine();

        StartLine(node.question, () =>
        {
            if (IsFinalNode(node))
            {
                finalWaitRoutine = StartCoroutine(WaitForSpaceThenComplete());
                return;
            }

            ApplyNodeChoiceLabels(node);
            waitingForChoice = true;
            ShowChoices(true);
        }, waitDelay: false);
    }

    private IEnumerator WaitForSpaceThenComplete()
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

        CompleteDialogue();
        finalWaitRoutine = null;
    }

    private void CompleteDialogue()
    {
        if (!spendInteractionMinutesOnEnter && !TrySpendInteractionMinutes())
            return;

        MarkCompleted();
        TryApplyTickProgress();

        unlockedNewClueThisRun = UnlockClueAtEnd();
        ShowCluePopupIfNeeded();

        waitingForChoice = false;
        ShowChoices(false);
        SetChoiceLabels(defaultYesLabel, defaultNoLabel);

        StartLine(endPromptText, CloseDialogue, waitDelay: false);
    }

private void TryApplyTickProgress()
{
    // No longer needed.
    // Clock system now reads completion automatically.
}

    private bool HasAppliedTickAlready()
    {
        return PlayerPrefs.GetInt(TickAppliedKey, 0) == 1;
    }

    private bool CanBeginInteraction()
    {
        if (GlobalTimeSystem.Instance == null)
        {
            Debug.LogError("MessageChainDialogue: No GlobalTimeSystem instance found in the scene.");
            return false;
        }

        if (GlobalTimeSystem.Instance.IsEndingSequenceRunning || GlobalTimeSystem.Instance.IsTimeUp)
            return false;

        if (!requireEnoughTimeToStart)
            return true;

        return CanSpendMinutes(interactionMinutesCost);
    }

    private bool TrySpendInteractionMinutes()
    {
        return TrySpendMinutes(interactionMinutesCost);
    }

    private bool TrySpendMinutes(int minutes)
    {
        if (minutes <= 0)
            return true;

        if (GlobalTimeSystem.Instance == null)
        {
            Debug.LogError("MessageChainDialogue: No GlobalTimeSystem instance found in the scene.");
            return false;
        }

        if (!GlobalTimeSystem.Instance.CanSpend(minutes))
        {
            ShowNotEnoughTimeMessage();
            return false;
        }

        GlobalTimeSystem.Instance.Spend(minutes);
        return true;
    }

    private bool CanSpendMinutes(int minutes)
    {
        if (minutes <= 0)
            return true;

        if (GlobalTimeSystem.Instance == null)
        {
            Debug.LogError("MessageChainDialogue: No GlobalTimeSystem instance found in the scene.");
            return false;
        }

        return GlobalTimeSystem.Instance.CanSpend(minutes);
    }

    private void ShowNotEnoughTimeMessage()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        ShowChoices(false);

        if (transcriptText != null && !string.IsNullOrWhiteSpace(notEnoughTimeText))
            transcriptText.text = notEnoughTimeText;
    }

    private void ApplyCharacterActions(QuestionNode node, bool choseYes)
    {
        if (characterWorld == null || node == null)
            return;

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
        if (nodes == null || nodeIndex < 0 || nodeIndex >= nodes.Length)
            return;

        ResolveSceneRefs();

        QuestionNode node = nodes[nodeIndex];
        if (!node.affectsAlignment)
            return;

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
        if (flowRoutine != null)
            StopCoroutine(flowRoutine);

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
            float timer = 0f;

            while (timer < readDelaySeconds)
            {
                if (Keyboard.current != null && Keyboard.current[skipDelayKey].isPressed)
                    break;

                timer += Time.unscaledDeltaTime;
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
        if (scrollRect == null || scrollRect.content == null)
            return;

        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }

    private void OpenDialogue()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        ShowChoices(false);
        SetChoiceLabels(defaultYesLabel, defaultNoLabel);

        if (characterWorld != null)
            characterWorld.NotifyDialogueOpened(conversationId);
    }

    private void CloseDialogue()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (characterWorld != null)
            characterWorld.NotifyDialogueClosed();

        IsConversationRunning = false;
        UnlockPlayerMovement();
        EndInteractionProtection();
    }

    private void ShowChoices(bool show)
    {
        if (choicesPanel != null)
            choicesPanel.SetActive(show);

        if (yesButton != null)
            yesButton.interactable = show;

        if (noButton != null)
            noButton.interactable = show;
    }

    private void SetChoiceLabels(string yes, string no)
    {
        if (yesButtonLabel != null)
            yesButtonLabel.text = yes;

        if (noButtonLabel != null)
            noButtonLabel.text = no;
    }

    private void ApplyNodeChoiceLabels(QuestionNode node)
    {
        string yes = string.IsNullOrWhiteSpace(node.yesLabel) ? defaultYesLabel : node.yesLabel;
        string no = string.IsNullOrWhiteSpace(node.noLabel) ? defaultNoLabel : node.noLabel;
        SetChoiceLabels(yes, no);
    }

    private void ResetConversationRuntimeState()
    {
        unlockedNewClueThisRun = false;
        waitingForChoice = false;
        isTyping = false;
        currentNodeIndex = -1;

        StopFinalWaitRoutine();
    }

    private void StopFinalWaitRoutine()
    {
        if (finalWaitRoutine != null)
        {
            StopCoroutine(finalWaitRoutine);
            finalWaitRoutine = null;
        }
    }

    private void BeginInteractionProtection()
    {
        if (interactionProtectionActive)
            return;

        if (GlobalTimeSystem.Instance != null)
        {
            GlobalTimeSystem.Instance.BeginProtectedInteraction();
            interactionProtectionActive = true;
        }
    }

    private void EndInteractionProtection()
    {
        if (!interactionProtectionActive)
            return;

        if (GlobalTimeSystem.Instance != null)
            GlobalTimeSystem.Instance.EndProtectedInteraction();

        interactionProtectionActive = false;
    }

    private bool IsFinalNode(QuestionNode node)
    {
        return node.nextAfterYes < 0 && node.nextAfterNo < 0;
    }

    private bool HasValidCurrentNode()
    {
        return nodes != null && currentNodeIndex >= 0 && currentNodeIndex < nodes.Length;
    }

    private bool ValidateCoreRefs()
    {
        if (transcriptText == null || scrollRect == null)
        {
            Debug.LogError("MessageChainDialogue: Assign transcriptText and scrollRect.");
            return false;
        }

        return true;
    }

    private void HandleSoftResetInput()
    {
        if (!enableSoftReset || Keyboard.current == null)
            return;

        bool one = Keyboard.current.digit1Key.isPressed;
        bool two = Keyboard.current.digit2Key.isPressed;
        bool three = Keyboard.current.digit3Key.isPressed;

        if (one && two && three && Keyboard.current.digit3Key.wasPressedThisFrame)
            SoftResetDialogueAndClue();
    }

    private void SoftResetDialogueAndClue()
    {
        if (flowRoutine != null)
        {
            StopCoroutine(flowRoutine);
            flowRoutine = null;
        }

        StopFinalWaitRoutine();

        PlayerPrefs.DeleteKey(CompletedKey);
        PlayerPrefs.DeleteKey(TickAppliedKey);

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
        currentNodeIndex = -1;

        ShowChoices(false);
        SetChoiceLabels(defaultYesLabel, defaultNoLabel);
        CloseDialogue();

        Debug.Log("Soft reset done (dialogue + clue + choice flags + tick flag).");
    }

    private void MarkCompleted()
    {
        if (!rememberCompletion)
            return;

        PlayerPrefs.SetInt(CompletedKey, 1);
        PlayerPrefs.Save();
    }

    private bool IsCompleted()
    {
        if (!rememberCompletion)
            return false;

        return PlayerPrefs.GetInt(CompletedKey, 0) == 1;
    }

    private void ResolveSceneRefs()
    {
        if (alignmentSlider == null)
            alignmentSlider = FindFirstObjectByType<AlignmentSlider>(FindObjectsInactive.Include);

        if (alignmentPopupSpawner == null)
            alignmentPopupSpawner = FindFirstObjectByType<AlignmentPointPopupSpawner>(FindObjectsInactive.Include);
    }

    private void LockPlayerMovement()
    {
        if (!lockPlayerMovementWhileDialogueIsOpen)
            return;

        if (hasLockedMovementThisConversation)
            return;

        if (lockedPlayerMovement == null)
            lockedPlayerMovement = GameObject.FindAnyObjectByType<PlayerMovement2D>();

        if (lockedPlayerMovement != null)
        {
            lockedPlayerMovement.LockMovement();
            hasLockedMovementThisConversation = true;
        }
    }

    private void UnlockPlayerMovement()
    {
        if (!lockPlayerMovementWhileDialogueIsOpen)
            return;

        if (!hasLockedMovementThisConversation)
            return;

        if (lockedPlayerMovement != null)
            lockedPlayerMovement.UnlockMovement();

        hasLockedMovementThisConversation = false;
    }

    public string ConversationId => conversationId;

    public static bool HasCompletedConversation(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return false;

        return PlayerPrefs.GetInt($"DialogueCompleted_{id}", 0) == 1;
    }
}