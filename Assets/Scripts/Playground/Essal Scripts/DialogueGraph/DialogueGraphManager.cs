using Assets.Scripts.GameScripts;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;


public class DialogueGraphManager : MonoBehaviour
{
    #region Singleton Pattern
    public static DialogueGraphManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
        dialogueRectTransform = DialogueText.GetComponent<RectTransform>();
        speakerTextInitialAnchoredPos = dialogueRectTransform.anchoredPosition;
    }
    #endregion

    #region Inspector
    //public RuntimeDialogueGraph RuntimeGraph;

    [Header("UI Components")]
    public GameObject DialoguePanel;
    public TextMeshProUGUI SpeakerNameText;
    public TextMeshProUGUI DialogueText;
    private RectTransform dialogueRectTransform;
    private Vector2 speakerTextInitialAnchoredPos;

    [Header("Speaker UI")]
    public Image SpeakerSprite;
    public Transform SpeakerSpriteContainer;

    [HideInInspector]
    public InteractableScriptableObject currentInteractable;

    [Header("Choice Button UI")]
    public GameObject ChoiceUI;
    public Button ChoiceButtonPrefab;
    public Transform ChoiceButtonContainer;
    public Color PathExplored;
    public Color Unlockable;
    public ScrollRect scroll;
    private List<GameObject> buttons = new List<GameObject>();

    // Typing Speeds
    private const float Slow = 0.1f;
    private const float Mid = 0.03f;
    private const float Fast = 0.001f;


    #endregion

    #region Variables

    // Helper bool
    [HideInInspector]
    public bool isDialogueRunning = false;

    // For controlling dialogue flow
    private bool skipTyping = false;
    private bool isTyping = false;

    // For pausing dialogue while fading
    private bool isFading = false;

    // NodeIDs pointing to a node
    private Dictionary<string, RuntimeNode> _nodeLookup = new Dictionary<string, RuntimeNode>();
    private RuntimeNode _currentNode;

    // For tracking choices - they will be marked as read
    private HashSet<string> exploredChoicesLookup = new HashSet<string>();

    // For tracking Callbacks
    [NonSerialized]
    public HashSet<Callback> callbacksCollected = new HashSet<Callback>();

    // For handling MarkAsRead
    private HashSet<RuntimeDialogueNode> nodesMarkedAsRead = new HashSet<RuntimeDialogueNode>();

    // For Handling TalkWillingness
    [NonSerialized]
    public HashSet<DialogueSpeaker> speakersNotWillingToTalk = new HashSet<DialogueSpeaker>();

    public void ClearLists()
    {
        exploredChoicesLookup.Clear();
        callbacksCollected.Clear();
        nodesMarkedAsRead.Clear();
        speakersNotWillingToTalk.Clear();
    }
    #endregion



    #region Input Handling (update)
    private void Update()
    {
        if (!DialoguePanel.activeSelf) return;

        if (DialogueInputBlocker.BlockSpaceAdvance)
        return;

        if (ChoiceButtonContainer.childCount > 0) return;

        if (_currentNode == null)
        {
            if (!isTyping && Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                EndDialogue();
            }
            return;
        }

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (isTyping)
            {
                skipTyping = true;
            }
            else if (_currentNode is RuntimeDialogueNode dialogueNode)
            {
                ShowNode(dialogueNode.NextNodeID);
            }
        }
    }

    #endregion

    #region Node Flow Handling
    public void StartDialogue(RuntimeDialogueGraph dialogue)
    {
        if (Player.Instance != null)
        {
            Player.Instance.interacting = true;
        }
        else
        {
            Debug.LogWarning("Dialogue started in a scene without the Player singleton. No need to worry this is ideal in the Main Menu");
        }

        isDialogueRunning = true;
        ClearDialogue();
        _nodeLookup.Clear();

        foreach (var node in dialogue.AllNodes)
        {
            _nodeLookup[node.NodeID] = node;
        }

        if (!string.IsNullOrEmpty(dialogue.EntryNodeID))
        {
            ShowNode(dialogue.EntryNodeID);
        }
        else
        {
            EndDialogue();
        }

        DialoguePanel.SetActive(true);
    }

    public void ShowNode(string nodeID)
    {
        StartCoroutine(ShowNodeRoutine(nodeID));
    }

    // Change this from 'public void' to 'public IEnumerator'
    public IEnumerator ShowNodeRoutine(string nodeID)
    {
        while (!string.IsNullOrEmpty(nodeID))
        {
            if (!_nodeLookup.TryGetValue(nodeID, out _currentNode))
            {
                EndDialogue();
                yield break;
            }

            ChoiceUI.SetActive(_currentNode is RuntimeChoiceNode ? true : false); // Toggles the Choice UI based on if the current node is a choice node or not.

            // 1. Mark as Read Check
            if (_currentNode is RuntimeDialogueNode readNode && nodesMarkedAsRead.Contains(readNode))
            {
                nodeID = readNode.MarkAsReadNodeID;
                continue;
            }

            // 2. Execution & Waiting
            if (_currentNode is RuntimeFadeNode fadeNode)
            {
                // Yield return the Fade Coroutine directly to wait for it!
                yield return StartCoroutine(FadeRoutine(fadeNode.blockSpaceDuringFade, fadeNode.duration, fadeNode.stayBlackDuration, fadeNode.color));
                nodeID = fadeNode.NextNodeID;
            }
            else
            {
                string nextNodeID = _currentNode.Execute(this);

                // 3. Stop loop if we hit Dialogue (UI handles the rest via Update)
                if (_currentNode is RuntimeDialogueNode) yield break;

                nodeID = nextNodeID;
            }
        }
    }

    public void EndDialogue()
    {
        StopAllCoroutines(); // Kill any typing or navigation
        _navigationCoroutine = null;
        isTyping = false;

        AudioManager.instance.StopSFX("Dialogue");
        DialoguePanel.SetActive(false);
        _currentNode = null;

        ClearChoices();

        if (Player.Instance != null)
            Player.Instance.interacting = false;
        isDialogueRunning = false;
    }
    #endregion

    #region NodeHandling
    public void HandleDialogueNode(RuntimeDialogueNode node)
    {
        if (DialoguePanel.gameObject.activeSelf == false) // OPEN THE DIALOGUE UI
        {
            DialoguePanel.SetActive(true);
        }

        HandleSpeakerData(node); // HANDLE THE SPEAKER DATA
        StopAllCoroutines(); // STOP ALL COROUTINES AND START THE TYPING COROUTINE
        StartCoroutine(TypeDialogue(node.Dialogue, node));
    }
    public void HandleAlignmentNode(RuntimeAlignmentNode node) // SETS THE ALIGNMENT ON THE PLAYER SINGLETON
    {
        Player.Instance.ChangeHumanity(node.HumanityChange);
        Player.Instance.ChangeUndead(node.UndeadChange);
    }

    public void HandleActionNode(RuntimeActionNode node) // Calls the DoAction method implemented on the SO
    {
        node.Action.DoAction();
    }

    public void HandleRandomizer(RuntimeRandomizer node) // FINDS A RANDOM PORT IF ITS CONNECTED TO SEVERAL NODES
    {
        int randomIndex = UnityEngine.Random.Range(0, node.randomNextNodeID.Count);
        node.NextNodeID = node.randomNextNodeID[randomIndex];
    }

    public void HandleClueNode(RuntimeClueNode node) // ADDS CLUE TO CASEMANAGERS CLUEFOUND LIST
    {
        CaseManager.Instance.ClueInfoUpdated(node.clue, node.description, node.typePointers);
        Debug.Log("Handling Clue");
    }

    public void HandleTalkWillingnessNode(RuntimeTalkWillingnessNode node)
    {
        if (node.IsWillingToTalk == TalkWillingNessEnum.WILLING) // IF WILLING
        {
            if (speakersNotWillingToTalk.Contains(node.Speaker)) // AND THE TALK WILLINGNESS LOOKUP CONTAINS THAT SPEAKER
            {
                speakersNotWillingToTalk.Remove(node.Speaker); // REMOVE IT, SO YOUR ABLE TO PASS THE TALK WILLINGNESS CHECK
            }
        }
        else if (node.IsWillingToTalk == TalkWillingNessEnum.NOT_WILLING) // IF NOT WIILLING
        {
            if (!speakersNotWillingToTalk.Contains(node.Speaker)) // AND THE WILLINGNESS LOOKUP DOESNT CONTAIN THAT SPEAKER
            {
                speakersNotWillingToTalk.Add(node.Speaker); // ADD IT, SO THAT NEXT TIME YOU WONT BE ABLE TO PASS THE TALK WILLINGNESS CHECK
            }
        }
    }

    public void HandleMajorDecisionNode(RuntimeMajorDecisionNode node)
    {
        CaseManager.Instance.majorDecisions.Add(node.decisionString);
        Debug.Log("Major Decison Logged from dialogue");
    }

    public bool HandleConditionNode(RuntimeConditionNode node)
    {
        Debug.Log(callbacksCollected.Contains(node.callback));
        if (node == null || node.condition == ConditionOptions.NONE) return true;

        return node.condition switch
        {
            ConditionOptions.ALIGNMENT =>
                Player.Instance != null &&
                Player.Instance.humanity >= node.humanity &&
                Player.Instance.undead >= node.undead,

            ConditionOptions.CLUE =>
                node.clue == null || CaseManager.Instance.cluesfound.Contains(node.clue),

            ConditionOptions.WILLING_TO_TALK =>
                node.TalkWillingnessTarget == null ||
                !speakersNotWillingToTalk.Contains(node.TalkWillingnessTarget),

            ConditionOptions.CALLBACK =>
                node.callback == null || callbacksCollected.Contains(node.callback),

            ConditionOptions.CUSTOM =>
                node.customCondition == null || node.customCondition.IsMet(),

            _ => true
        };
    }

    private Coroutine _navigationCoroutine;

    public void HandleChoiceNode(RuntimeChoiceNode node)
    {
        ClearChoices();
        List<GameObject> choiceButtons = new List<GameObject>();

        foreach (var choice in node.choices)
        {
            if (!ViableChoice(choice)) continue;

            Button button = Instantiate(ChoiceButtonPrefab, ChoiceButtonContainer);
            button.GetComponentInChildren<TextMeshProUGUI>().text = choice.ChoiceText;

            // Setup visual state
            button.GetComponent<Image>().color = exploredChoicesLookup.Contains(choice.ChoiceID)
                ? PathExplored
                : Color.white;

            // Button Logic
            button.onClick.AddListener(() =>
            {
                if (_navigationCoroutine != null) StopCoroutine(_navigationCoroutine); // Stop navigating
                AudioManager.instance.PlaySFX("pickChoice", 0.5f);
                exploredChoicesLookup.Add(choice.ChoiceID);
                ClearChoices();
                ShowNode(choice.DestinationNodeID);
            });

            choiceButtons.Add(button.gameObject);
        }

        // Start the navigation logic if we have buttons
        if (choiceButtons.Count > 0)
        {
            if (_navigationCoroutine != null) StopCoroutine(_navigationCoroutine);
            _navigationCoroutine = StartCoroutine(SelectFirst(choiceButtons));
        }
    }

    public void HandleSoundNode(RuntimeSoundNode node)
    {
        if (node.clip != null)
        {
            if (!AudioManager.instance.CheckSound(node.clip.name))
            {
                AudioManager.instance.AddSound(node.clip);
            }
            if (node.isMusic)
            {
                //AudioManager.instance.StopMusic(AudioManager.instance.currentSong.name);
                AudioManager.instance.PlayMusic(node.clip.name);
                AudioManager.instance.StopLoopingTracks();
                Debug.Log("Playing Music: " + node.clip.name);
            }
            else
            {
                AudioManager.instance.PlaySFX(node.clip.name);
                Debug.Log("Playing Sound: " + node.clip.name);
            }
        }
        else
        {
            Debug.LogWarning("Sound node has no clip assigned!");
        }               
    }

    public void HandleFadeNode(RuntimeFadeNode node)
    {
        StartCoroutine(FadeRoutine(node.blockSpaceDuringFade, node.duration, node.stayBlackDuration, node.color));
        Debug.Log("Fading in a dialogue");
    }

    #endregion

    #region Helping Functions
    IEnumerator TypeDialogue(List<string> dialogue, RuntimeDialogueNode node)
    {
        float _typingSpeed = HandleTypingSpeed(node.TypingSpeed);
        isTyping = true;
        UpdateDialoguePosition(node.Speaker);

        for (int i = 0; i < dialogue.Count; i++)
        {
            string sentence = dialogue[i];
            DialogueText.text = "";
            skipTyping = false;

            // --- Typing Loop ---
            foreach (char letter in sentence.ToCharArray())
            {
                AudioManager.instance.PlaySFX("Dialogue", 0.8f);
                DialogueText.text += letter;

                float timer = 0f;
                while (timer < _typingSpeed)
                {
                    if (skipTyping) break;
                    timer += Time.deltaTime;
                    yield return null;
                }

                if (skipTyping)
                {
                    DialogueText.text = sentence;
                    AudioManager.instance.PlaySFX("skipTyping", 0.5f);
                    break;
                }
            }

            AudioManager.instance.StopSFX("Dialogue");
            skipTyping = false;

            // NEW LOGIC: Only wait for input if this is NOT the last sentence of the node.
            // If it IS the last sentence, we exit the loop so Update() handles the transition.
            bool isLastSentence = (i == dialogue.Count - 1);

            if (!isLastSentence)
            {
                yield return null; // Buffer frame
                yield return new WaitUntil(() => !Keyboard.current.spaceKey.isPressed);
                yield return new WaitUntil(() => Keyboard.current.spaceKey.wasPressedThisFrame);
            }
        }

        isTyping = false;

        if (node.MarkAsRead)
        {
            nodesMarkedAsRead.Add(node);
        }
    }
    private void UpdateDialoguePosition(DialogueSpeaker speaker)
    {
        if (dialogueRectTransform == null) return;

        if (speaker == null)
        {
            // Snap back to original UI design position
            dialogueRectTransform.anchoredPosition = speakerTextInitialAnchoredPos;
            return;
        }

        // Adjust this pixel value if 60 looks too small or large on higher resolutions
        float offset = (speaker.SpeakerName == "Narrator") ? 60f : 0f;

        dialogueRectTransform.anchoredPosition = new Vector2(
            speakerTextInitialAnchoredPos.x,
            speakerTextInitialAnchoredPos.y + offset
        );
    }

    public IEnumerator SelectFirst(List<GameObject> buttons)
    {
        if (buttons == null || buttons.Count == 0) yield break;

        // 1. Initial Setup
        EventSystem.current.SetSelectedGameObject(buttons[0]);
        GameObject lastSelected = buttons[0];

        // 2. Setup Explicit Navigation (Wrapping)
        for (int i = 0; i < buttons.Count; i++)
        {
            Navigation nav = new Navigation { mode = Navigation.Mode.Explicit };

            // Wrap around logic
            Selectable up = buttons[i > 0 ? i - 1 : buttons.Count - 1].GetComponent<Selectable>();
            Selectable down = buttons[i < buttons.Count - 1 ? i + 1 : 0].GetComponent<Selectable>();

            nav.selectOnUp = up;
            nav.selectOnDown = down;

            if (buttons[i].TryGetComponent<Selectable>(out var selectable))
            {
                selectable.navigation = nav;
            }
        }

        int lastIndex = 0;

        // 3. Navigation Loop
        while (true)
        {
            var current = EventSystem.current.currentSelectedGameObject;

            // If the player clicks away or uses a mouse to click empty space, force selection back
            if (current == null || !buttons.Contains(current))
            {
                EventSystem.current.SetSelectedGameObject(lastSelected);
            }
            else
            {
                int currentindex = buttons.IndexOf(current);

                if (currentindex != lastIndex)
                {
                    // Logic for Jumping (Top to Bottom or Bottom to Top)
                    if (Mathf.Abs(currentindex - lastIndex) > 1)
                    {
                        if (currentindex == 0) SetViewPortToSelectedButton(10); // Snap to top
                        else if (currentindex == buttons.Count - 1) SetViewPortToSelectedButton(-10); // Snap to bottom
                    }
                    // Standard Sequential Scroll
                    else if (currentindex > lastIndex && currentindex > 2)
                    {
                        SetViewPortToSelectedButton(-1);
                    }
                    else if (currentindex < lastIndex && currentindex < buttons.Count - 3)
                    {
                        SetViewPortToSelectedButton(1);
                    }

                    lastSelected = current;
                    lastIndex = currentindex;
                }
            }

            yield return null;
        }
    }

    public void SetViewPortToSelectedButton(int direction)
    {
        if (scroll.content.childCount == 0) return;

        RectTransform content = scroll.content;
        VerticalLayoutGroup layout = content.GetComponent<VerticalLayoutGroup>();

        // Measure the "Step" (Height of one button + the gap between them)
        RectTransform item = content.GetChild(0) as RectTransform;
        float stepSize = item.rect.height + layout.spacing;

        Vector2 pos = content.anchoredPosition;

        // direction -1 = Player moved DOWN index -> Content moves UP (y increases)
        // direction  1 = Player moved UP index   -> Content moves DOWN (y decreases)
        if (direction == -1)
        {
            pos.y += stepSize;
        }
        else if (direction == 1)
        {
            pos.y -= stepSize;
        }

        // Clamp the scroll so we don't fly off into the void
        float maxY = Mathf.Max(0, content.rect.height - scroll.viewport.rect.height);
        pos.y = Mathf.Clamp(pos.y, 0, maxY);

        content.anchoredPosition = pos;
    }

    bool ViableChoice(ChoiceData choice)
    {
        if (choice == null) return true;

        if (choice.condition == ConditionOptions.NONE) return true;
        else if (choice.condition == ConditionOptions.ALIGNMENT)
        {
            if (Player.Instance == null) return true;
            if (choice.choiceHumanityCondtion > Player.Instance.humanity) return false;
            if (choice.choiceUndeadCondtion > Player.Instance.undead) return false;
        }
        else if (choice.condition == ConditionOptions.CLUE)
        {
            if (choice.choiceConditionClue == null) return true;
            if (CaseManager.Instance.cluesfound.Contains(choice.choiceConditionClue)) return true; else return false;
        }
        else if (choice.condition == ConditionOptions.WILLING_TO_TALK)
        {
            if (choice.choiceConditionSpeaker == null) return true;
            print(speakersNotWillingToTalk.Contains(choice.choiceConditionSpeaker));
            if (speakersNotWillingToTalk.Contains(choice.choiceConditionSpeaker)) return true; else return false;
        }
        else if (choice.condition == ConditionOptions.CALLBACK)
        {
            if (choice.choiceConditionCallback == null) return true;
            if (callbacksCollected.Contains(choice.choiceConditionCallback)) return true; else return false;
        }
        return true;
    }


    void HandleSpeakerData(RuntimeDialogueNode node)
    {
        SpeakerSprite.sprite = null;
        SpeakerSprite.enabled = false;

        if (node.Speaker == null)
        {
            SpeakerNameText.text = currentInteractable != null
                ? currentInteractable.name
                : "???";

            if (currentInteractable != null)
            {
                SpeakerSprite.enabled = true;
                SpeakerSprite.sprite = currentInteractable.interactableSprite;
                SpeakerSprite.preserveAspect = true;
            }
            else
            {
                SpeakerSprite.sprite = null;
                SpeakerSprite.enabled = false;
            }

            return;
        }
        else
        {
            if (node.Speaker.SpeakerName == "Narrator")
            {
                SpeakerNameText.text = "";
                SpeakerSprite.enabled = false;
            }
            else
            {
                SpeakerSprite.enabled = true;
                SpeakerNameText.text = node.Speaker.SpeakerName;
                SpeakerSprite.preserveAspect = true;
                HandleEmotion(node.Emotion, node);
            }
        }
    }


    // Set the Sprite in relation to the given emotion.
    void HandleEmotion(Emotion emotion, RuntimeDialogueNode node)
    {
        Sprite spriteToUse = null;

        if (node.Speaker != null)
        {
            switch (emotion)
            {
                case Emotion.ANGRY:
                    spriteToUse = node.Speaker.Angry;
                    break;

                case Emotion.HAPPY:
                    spriteToUse = node.Speaker.Happy;
                    break;

                case Emotion.CONTENT:
                    spriteToUse = node.Speaker.Content;
                    break;

                case Emotion.SAD:
                    spriteToUse = node.Speaker.Sad;
                    break;

                default:
                    spriteToUse = node.Speaker.Content;
                    break;
            }
        }

        // fallback ONLY if no speaker sprite exists
        if (spriteToUse == null && currentInteractable != null)
        {
            spriteToUse = currentInteractable.interactableSprite;
        }

        // FINAL APPLY — ONLY PLACE THAT TOUCHES UI
        SpeakerSprite.sprite = spriteToUse;
        SpeakerSprite.enabled = spriteToUse != null;
        SpeakerSprite.preserveAspect = true;
    }

    // Set the typingspeed in relation to the given typingspeed.
    float HandleTypingSpeed(TypingSpeed typingSpeed)
    {
        // It sets typing speed relative to the chosen enum. By default it sets it to 0.05f.
        float _typingSpeed;
        switch (typingSpeed)
        {
            case TypingSpeed.SLOW:
                _typingSpeed = Slow;
                break;
            case TypingSpeed.MID:
                _typingSpeed = Mid;
                break;
            case TypingSpeed.FAST:
                _typingSpeed = Fast;
                break;
            default:
                Debug.LogWarning("You didnt set a typing speed!");
                _typingSpeed = Mid;
                break;
        }
        return _typingSpeed;
    }
    private void ClearChoices()
    {
        if (_navigationCoroutine != null)
        {
            StopCoroutine(_navigationCoroutine);
            _navigationCoroutine = null;
        }

        foreach (Transform child in ChoiceButtonContainer)
        {
            Destroy(child.gameObject);
        }
        buttons.Clear();
    }

    //Reset the dialogue
    public void ClearDialogue()
    {
        DialogueText.text = "";
        SpeakerNameText.text = "";
        SpeakerSprite.enabled = false;
        skipTyping = false;
        isTyping = false;
        ClearChoices();
    }

    private IEnumerator FadeRoutine(bool blockSpaceDuringFade, float duration, float stayBlackDuration, Color color)
    {
        if (blockSpaceDuringFade)
            DialogueInputBlocker.BlockSpaceAdvance = true;

        WorldFade.Instance.StartScreenFade(
            duration,
            stayBlackDuration,
            color
        );

        float totalFadeTime =
            duration +
            stayBlackDuration +
            duration;

        yield return new WaitForSeconds(totalFadeTime);

        if (blockSpaceDuringFade)
            DialogueInputBlocker.BlockSpaceAdvance = false;
    }
}

    #endregion