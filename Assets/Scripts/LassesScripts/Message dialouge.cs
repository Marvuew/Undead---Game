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

        [Header("Choice button labels")]
        public string yesLabel = "";
        public string noLabel = "";

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

        [Header("Optional extra stamina cost per choice")]
        [Min(0)] public int staminaCostOnYes = 0;
        [Min(0)] public int staminaCostOnNo = 0;
    }

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

    [Header("Stamina")]
    [Tooltip("If true, stamina is spent immediately when this conversation starts.")]
    [SerializeField] private bool spendStaminaOnEnter = false;

    [Tooltip("Main stamina cost for this interaction.")]
    [SerializeField] private int staminaCostPerInteraction = 1;

    [Tooltip("If true, player must have enough stamina before the interaction can begin.")]
    [SerializeField] private bool requireEnoughStaminaToStart = true;

    [TextArea]
    [SerializeField] private string notEnoughStaminaText = "I'm too exhausted for that right now.";

    [SerializeField] private StaminaBarUI staminaBarUI;

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
        GlobalStaminaSystem.InitializeIfNeeded();

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
        RefreshStaminaUI();
    }

    private void OnEnable()
    {
        ResolveSceneRefs();

        if (alignmentSlider != null)
            alignmentSlider.RefreshFromSave();

        RefreshStaminaUI();
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
            ShowNotEnoughStaminaMessage();
            return;
        }

        if (spendStaminaOnEnter && !TrySpendMainInteractionStamina())
            return;

        ResetConversationRuntimeState();
        OpenDialogue();

        if (rememberCompletion && IsCompleted())
        {
            ShowRevisitFlow();
            return;
        }

        StartIntroFlow();
    }

    public void OnYesClicked()
    {
        HandleChoice(true);
    }

    public void OnNoClicked()
    {
        HandleChoice(false);
    }

    private void HandleChoice(bool choseYes)
    {
        if (!waitingForChoice || isTyping || !HasValidCurrentNode())
            return;

        QuestionNode node = nodes[currentNodeIndex];
        int staminaCost = choseYes ? node.staminaCostOnYes : node.staminaCostOnNo;

        if (!TrySpendChoiceStamina(staminaCost))
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

        currentNodeIndex = index;
        waitingForChoice = false;
        ShowChoices(false);
        StopFinalWaitRoutine();

        QuestionNode node = nodes[currentNodeIndex];

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
        if (!spendStaminaOnEnter && !TrySpendMainInteractionStamina())
            return;

        MarkCompleted();
        unlockedNewClueThisRun = UnlockClueAtEnd();
        ShowCluePopupIfNeeded();

        waitingForChoice = false;
        ShowChoices(false);
        SetChoiceLabels(defaultYesLabel, defaultNoLabel);

        StartLine(endPromptText, CloseDialogue, waitDelay: false);
    }

    private bool CanBeginInteraction()
    {
        if (!requireEnoughStaminaToStart)
            return true;

        int mainCost = Mathf.Max(0, staminaCostPerInteraction);
        return GlobalStaminaSystem.HasEnough(mainCost);
    }

    private bool TrySpendMainInteractionStamina()
    {
        int cost = Mathf.Max(0, staminaCostPerInteraction);
        return TrySpendStamina(cost);
    }

    private bool TrySpendChoiceStamina(int cost)
    {
        return TrySpendStamina(Mathf.Max(0, cost));
    }

    private bool TrySpendStamina(int cost)
    {
        if (cost <= 0)
            return true;

        if (!GlobalStaminaSystem.TrySpend(cost))
        {
            ShowNotEnoughStaminaMessage();
            return false;
        }

        RefreshStaminaUI();
        return true;
    }

    private void ShowNotEnoughStaminaMessage()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        ShowChoices(false);

        if (transcriptText != null && !string.IsNullOrWhiteSpace(notEnoughStaminaText))
            transcriptText.text = notEnoughStaminaText;
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

    private void RefreshStaminaUI()
    {
        ResolveSceneRefs();

        if (staminaBarUI != null)
            staminaBarUI.Refresh();
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

        if (nodes != null)
        {
            for (int i = 0; i < nodes.Length; i++)
                PlayerPrefs.DeleteKey(ChoiceAppliedKey(i));
        }

        if (!string.IsNullOrWhiteSpace(clueIdToUnlock))
            ClueSaveSystem.Lock(clueIdToUnlock);

        GlobalStaminaSystem.RefillToMax();
        PlayerPrefs.Save();

        if (transcriptText != null)
            transcriptText.text = "";

        waitingForChoice = false;
        isTyping = false;
        unlockedNewClueThisRun = false;
        currentNodeIndex = -1;

        ShowChoices(false);
        SetChoiceLabels(defaultYesLabel, defaultNoLabel);
        RefreshStaminaUI();
        CloseDialogue();

        Debug.Log("Soft reset done (dialogue + clue + choice flags + stamina refill).");
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

        if (staminaBarUI == null)
            staminaBarUI = FindFirstObjectByType<StaminaBarUI>(FindObjectsInactive.Include);
    }
}