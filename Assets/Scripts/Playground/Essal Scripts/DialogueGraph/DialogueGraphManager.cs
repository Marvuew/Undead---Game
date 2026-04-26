using Assets.Scripts.GameScripts;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;


public class DialogueGraphManager : MonoBehaviour
{
    #region Singleton Pattern
    public static DialogueGraphManager instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        SpeakerTextY = DialogueText.transform.position;
    }
    #endregion

    #region Inspector
    //public RuntimeDialogueGraph RuntimeGraph;

    [Header("UI Components")]
    public GameObject DialoguePanel;
    public TextMeshProUGUI SpeakerNameText;
    public TextMeshProUGUI DialogueText;
    private Vector3 SpeakerTextY;

    [Header("Speaker UI")]
    public Image SpeakerSprite;
    public Transform SpeakerSpriteContainer;

    [HideInInspector]
    public Clue currentInteractable;

    [Header("Choice Button UI")]
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
    // For controlling dialogue flow
    private bool skipTyping = false;
    private bool isTyping = false;

    // NodeIDs pointing to a node
    private Dictionary<string, RuntimeNode> _nodeLookup = new Dictionary<string, RuntimeNode>();
    private RuntimeNode _currentNode;
    
    // For tracking choices - they will be marked as read
    private HashSet<string> exploredChoicesLookup = new HashSet<string>();

    // For tracking Callbacks
    [HideInInspector]
    public HashSet<Callback> CallbackLookup = new HashSet<Callback>();

    // For handling MarkAsRead
    private HashSet<RuntimeDialogueNode> MarkAsReadNodeLookup = new HashSet<RuntimeDialogueNode>();

    // For Handling TalkWillingness
    public HashSet<DialogueSpeaker> TalkWillingnessLookup = new HashSet<DialogueSpeaker>();
    #endregion

    #region Input Handling (update)

    public void Start()
    {
        
    }
    private void Update()
    {   // Runs if the current node is an end node.
        if (_currentNode == null)
        {
            // Only closes if it is finished typing
            if (DialoguePanel.activeSelf && !isTyping)
            {
                EndDialogue();
            }
            return;
        }


        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            // First click finishes the text.
            if (isTyping && DialoguePanel.activeSelf)
            {
                if (!skipTyping) skipTyping = true;
            }
            else
            {
                // If the text is finsihed typing show next node
                if (_currentNode is RuntimeDialogueNode dialogueNode && dialogueNode.Choices.Count == 0)
                {
                    ShowNode(_currentNode.NextNodeID);
                }
            }
        }
    }

    #endregion

    #region Node Flow Handling
    public void StartDialogue(RuntimeDialogueGraph dialogue)
    {
        // Clear Dialogue and reset the nodelookup
        ClearDialogue();
        _nodeLookup.Clear();
        
        // Add the nodes to the nodelookup
        foreach (var node in dialogue.AllNodes)
        {
            _nodeLookup[node.NodeID] = node;
        }

        // Show the first node if it has a entry ID
        if (!string.IsNullOrEmpty(dialogue.EntryNodeID))
        {
            ShowNode(dialogue.EntryNodeID);
        }
        else
        {
            EndDialogue();
        }
        // Make Dialogue Visible
        DialoguePanel.SetActive(true);
    }

    public void ShowNode(string nodeID)
    {
        while (nodeID != null)
        {
            if (!_nodeLookup.ContainsKey(nodeID))
            {
                EndDialogue();
                return;
            }

            _currentNode = _nodeLookup[nodeID];

            // 1. CONDITION CHECK (Logic Branching)
            if (_currentNode is RuntimeDialogueNode diagNode && diagNode.conditionToggle)
            {
                if (!ViableNode(diagNode))
                {
                    // Move to Fail branch and continue loop
                    nodeID = diagNode.ConditionFailNodeID;
                    continue;
                }
                else if (!string.IsNullOrEmpty(diagNode.ConditionSuccessNodeID))
                {
                    // If we have a success branch, move there
                    nodeID = diagNode.ConditionSuccessNodeID;
                    continue;
                }
            }

            // 2. MARK AS READ CHECK
            if (_currentNode is RuntimeDialogueNode readNode && MarkAsReadNodeLookup.Contains(readNode))
            {
                nodeID = readNode.MarkAsReadNodeID;
                continue;
            }

            // 3. EXECUTION
            // This actually calls HandleDialogueNode, HandleAlignment, etc.
            string nextNode = _currentNode.Execute(this);

            // 4. WAIT FOR USER (Dialogue Nodes only)
            if (_currentNode is RuntimeDialogueNode)
            {
                // We stop the loop here because HandleDialogueNode started a 
                // Coroutine and is waiting for player input/typing.
                return;
            }

            // 5. CONTINUE (For non-UI nodes like Alignment or Action)
            nodeID = nextNode;
        }
    }


    // Ends Dialogue by setting it inactive and deleting all choices from container
    private void EndDialogue()
    {
        DialoguePanel.SetActive(false);
        _currentNode = null;

        foreach(Transform child in ChoiceButtonContainer)
        {
            Destroy(child.gameObject);
        }
        
        Player.Instance.interacting = false;
    }
    #endregion

    #region NodeHandling
    public void HandleDialogueNode(RuntimeDialogueNode node)
    {
        if (DialoguePanel.gameObject.activeSelf == false)
        {
            DialoguePanel.SetActive(true);
        }

        HandleSpeakerData(node);

        StopAllCoroutines();
        StartCoroutine(TypeDialogue(node.Dialogue, node));
    }
    public void HandleAlignmentNode(RuntimeAlignmentNode node)
    {
        //GameEvents.ChangeAlignment(node.HumanityChange, node.UndeadChange);
        Player.Instance.ChangeHumanity(node.HumanityChange);
        Player.Instance.ChangeUndead(node.UndeadChange);
    }

    public void HandleActionNode(RuntimeActionNode node)
    {
        node.Action.DoAction();
    }

    public void HandleRandomizer(RuntimeRandomizer node)
    {
        int randomIndex = UnityEngine.Random.Range(0, node.randomNextNodeID.Count);
        node.NextNodeID = node.randomNextNodeID[randomIndex];
    }

    public void HandleClueNode(RuntimeClueNode node)
    {
        CaseManager.Instance.OnClueFound(node.clue);
        Debug.Log("Handling the clue");
    }

    public void HandleTalkWillingnessNode(RuntimeTalkWillingnessNode node)
    {
        if (node.IsWillingToTalk == TalkWillingNessEnum.WILLING)
        {
            if (TalkWillingnessLookup.Contains(node.Speaker))
            {
                TalkWillingnessLookup.Remove(node.Speaker);
            }
        }
        else if (node.IsWillingToTalk == TalkWillingNessEnum.NOT_WILLING)
        {
            if (!TalkWillingnessLookup.Contains(node.Speaker))
            {
                TalkWillingnessLookup.Add(node.Speaker);
            }
        }
    }

    #endregion

    #region Helping Functions


    IEnumerator TypeDialogue(List<string> dialogue, RuntimeDialogueNode node)
    {
        List<string> activeDialogue = new List<string>(dialogue);

        // Find the typingspeed
        float _typingSpeed = HandleTypingSpeed(node.TypingSpeed);
        isTyping = true;

        //Handle CallBacks first
        if (node.Callbacks != null)
        {
            foreach (var callback in node.Callbacks)
            {
                if (CallbackLookup.Contains(callback.CallbackAsset))
                {
                    if (!callback.Replace)
                    {
                        activeDialogue.Insert(callback.Index, callback.Sentence);
                    }
                    else if (callback.Replace)
                    {
                        activeDialogue[callback.Index] = callback.Sentence;
                    }
                }
            }
        }

        // Set the text at the top where the name should be.
        if (node.Speaker == null)
        {
            DialogueText.transform.position = SpeakerTextY;
        }
        else
        {
            if (node.Speaker.SpeakerName == "Narrator")
            {
                DialogueText.transform.position = new Vector3(DialogueText.transform.position.x, SpeakerTextY.y + 60f, DialogueText.transform.position.z);
            }
            else
            {
                DialogueText.transform.position = SpeakerTextY;
            }
        }

        //yield return null; // Wait a frame to ensure UI updates before typing starts
        foreach (string sentence in activeDialogue)
        {

            DialogueText.text = "";
            skipTyping = false;
            // Type each letter step by step according to typingspeed
            foreach (char letter in sentence.ToCharArray())
            {
                AudioManager.instance.PlaySFX("Dialogue");
                DialogueText.text += letter;
                float timer = 0f;
                while (timer < _typingSpeed)
                {
                    if (skipTyping) break;
                    timer += Time.deltaTime;
                    yield return null;
                }
                // Set the text if player has clicked
                if (skipTyping)
                {
                    AudioManager.instance.PlaySFX("skipTyping");
                    DialogueText.text = sentence;
                    break;
                }
            }
            AudioManager.instance.StopSFX("Dialogue");
            skipTyping = false;

            // Wait until the mouse is up and then you can continue to the next node.
            yield return new WaitUntil(() => !Mouse.current.leftButton.isPressed);
            yield return new WaitUntil(() => Keyboard.current.spaceKey.wasPressedThisFrame);

        }
        isTyping = false;
        //Handles the mark as read attribute
        if (node.MarkAsRead)
        {
            MarkAsReadNodeLookup.Add(node);
        }

        //Now list choice
        if (node.Choices.Count > 0) ListChoices(node);
    }

    public void ListChoices(RuntimeDialogueNode node)
    {
        ClearChoices();
        foreach (var choice in node.Choices)
        {

            // Inside your ListChoices loop
            bool isViable = ViableChoice(choice);
            if (!isViable) continue;
            Button button = Instantiate(ChoiceButtonPrefab, ChoiceButtonContainer);
            buttons.Add(button.gameObject);
            button.GetComponentInChildren<TextMeshProUGUI>().text = choice.ChoiceText;

            // --- COLOR SELECTION ---
            Color targetColor = Color.white; // Default


            if (exploredChoicesLookup.Contains(choice.ChoiceID))
            {
                Debug.Log(choice);
                button.GetComponent<Image>().color = PathExplored; // Gray/Red
                button.interactable = true;
            }
            else
            {
                button.GetComponent<Image>().color = Color.white; // Normal
                button.interactable = true;
            }

            Debug.Log($"Choice: {choice.ChoiceText} | Viable: {isViable} ");

                button.onClick.AddListener(() =>
                {
                    AudioManager.instance.PlaySFX("pickChoice");
                    exploredChoicesLookup.Add(choice.ChoiceID);
                    ClearChoices();
                    ShowNode(choice.DestinationNodeID);
                });
            }
        if (buttons.Count > 0) StartCoroutine(SelectFirst(buttons));
    }

    public IEnumerator SelectFirst(List<GameObject> buttons)
    {
        EventSystem.current.SetSelectedGameObject(buttons[0]);
        GameObject lastSelected = buttons[0];

        for (int i = 0; i < buttons.Count; i++)
        {
            Navigation nav = new Navigation();
            nav.mode = Navigation.Mode.Explicit;

            Selectable up = i > 0 ? buttons[i - 1].GetComponent<Selectable>() : buttons[buttons.Count - 1].GetComponent<Selectable>();
            Selectable down = i < buttons.Count - 1 ? buttons[i + 1].GetComponent<Selectable>() : buttons[0].GetComponent<Selectable>();

            nav.selectOnDown = down;
            nav.selectOnUp = up;

            var selectable = buttons[i].GetComponent<Selectable>();
            if (selectable != null)
            {
                selectable.navigation = nav;
            }
        }

        int lastIndex = -1;
        
        while (true)
        {
            var current = EventSystem.current.currentSelectedGameObject;

            if (buttons.Contains(current))
            {
                int currentindex = buttons.IndexOf(current);

                if (Mathf.Abs(currentindex - lastIndex) > 1)
                {
                    if (currentindex == 0)
                    {
                        for (int i = 0; i < Mathf.Abs(currentindex - lastIndex); i++)
                        {
                            SetViewPortToSelectedButton(1);
                        }
                    }
                    else if (currentindex == buttons.Count - 1)
                    {
                        for (int i = 0; i < Mathf.Abs(currentindex - lastIndex); i++)
                        {
                            SetViewPortToSelectedButton(-1);
                        }
                    }
                }
                else if (currentindex > lastIndex && currentindex > 2)
                {
                    Debug.Log("Moved Down");
                    SetViewPortToSelectedButton(-1);
                    
                }
                else if (currentindex < lastIndex && currentindex < buttons.Count - 3)
                {
                    Debug.Log("Moved Up");
                    SetViewPortToSelectedButton(1);
                }
                lastSelected = current;
                lastIndex = currentindex;
            }
            else
            {
                EventSystem.current.SetSelectedGameObject(lastSelected);
            }

            yield return null;
        }
    }

    public void SetViewPortToSelectedButton(int direction)
    {
        RectTransform content = scroll.content;

        // Get ANY child (they're same size)
        RectTransform item = content.GetChild(0) as RectTransform;

        float itemHeight = item.rect.height;
        float spacing = content.GetComponent<VerticalLayoutGroup>().spacing;
        float stepSize = itemHeight + spacing;

        Vector2 pos = content.anchoredPosition;

        if (direction == 1) // DOWN
        {
            pos.y -= stepSize;
        }
        else if (direction == -1) // UP
        {
            pos.y += stepSize;
        }

        float maxY = content.rect.height - scroll.viewport.rect.height;
        pos.y = Mathf.Clamp(pos.y, 0, maxY);

        content.anchoredPosition = pos;
    }

    // Check if its a viable choice. Otherwise dont show the Choice
    // Update this in DialogueGraphManager.cs
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
            print(TalkWillingnessLookup.Contains(choice.choiceConditionSpeaker));
            if (TalkWillingnessLookup.Contains(choice.choiceConditionSpeaker)) return true; else return false;
        }
        else if (choice.condition == ConditionOptions.CALLBACK)
        {
            if (choice.choiceConditionCallback == null) return true;
            if (CallbackLookup.Contains(choice.choiceConditionCallback)) return true; else return false;
        }
        return true;
    }

    bool ViableNode(RuntimeDialogueNode node)
    {
        if (node == null) return true;

            if (node.condition == ConditionOptions.NONE) return true;
            else if (node.condition == ConditionOptions.ALIGNMENT)
            {
                if (Player.Instance == null) return true;
                if (node.conditionHumanity > Player.Instance.humanity) return false;
                if (node.conditionUndead > Player.Instance.undead) return false;
            }
            else if (node.condition == ConditionOptions.CLUE)
            {
                if (node.conditionClue == null) return true;
                if (CaseManager.Instance.cluesfound.Contains(node.conditionClue)) return true; else return false;
            }
            else if (node.condition == ConditionOptions.WILLING_TO_TALK)
            {
                if (node.conditionSpeaker == null) return true;
            print(TalkWillingnessLookup.Contains(node.conditionSpeaker));
            if (TalkWillingnessLookup.Contains(node.conditionSpeaker)) return true; else return false; 
            }
            else if (node.condition == ConditionOptions.CALLBACK)
            {
                if (node.callbackCondition == null) return true;
            if (CallbackLookup.Contains(node.callbackCondition)) return true; else return false;
            }
        return true;
    }

    void HandleSpeakerData(RuntimeDialogueNode node)
    {
        // if there is no speaker attaches then it disables the speaker and sets the text to an empty string.
        if (node.Speaker == null)
        {
            SpeakerSprite.enabled = true;
            SpeakerSprite.preserveAspect = true;
            SpeakerNameText.text = currentInteractable.name;
            SpeakerSprite.sprite = currentInteractable.sprite;
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
        switch (emotion)
        {
            case Emotion.ANGRY:
                SpeakerSprite.sprite = node.Speaker.Angry;
                SpeakerSprite.preserveAspect = true;
                break;
            case Emotion.HAPPY:
                SpeakerSprite.sprite = node.Speaker.Happy;
                SpeakerSprite.preserveAspect = true;
                break;
            case Emotion.CONTENT:
                SpeakerSprite.sprite = node.Speaker.Content;
                SpeakerSprite.preserveAspect = true;
                break;
            case Emotion.SAD:
                SpeakerSprite.sprite = node.Speaker.Sad;
                SpeakerSprite.preserveAspect = true;
                break;
            default:
                Debug.LogWarning("Setting speaker to content!");
                SpeakerSprite.sprite = node.Speaker.Content;
                break;
        }
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

    // Delete all choicebuttons
    private void ClearChoices()
    {
        foreach (Transform child in ChoiceButtonContainer)
        {
            Destroy(child.gameObject);
        }
        buttons.Clear();
        StopCoroutine(SelectFirst(buttons));
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

    #endregion

    #region Legacy Code
    /*public void HandleInteractionNode(RuntimeinteractionNode node)
{
    SpeakerNameText.text = node.Name;
    HandleSpeakerSprite(node.Image);

    StopAllCoroutines();
    StartCoroutine(TypeDialogue(node.FluffText, node));
}*/

    /*public void ShowChoices(RuntimeChoiceNode node)
    {
        DialoguePanel.SetActive(true);

        SpeakerNameText.text = node.speaker.speakerName.ToString();

        HandleSpeakerSprite(node.speaker.SpeakerSprite);

        StopAllCoroutines();
        StartCoroutine(TypeDialogue(node.Dialogue));

        foreach (Transform child in ChoiceButtonContainer)
        {
            Destroy(child.gameObject);
        }

        if (node.Choices.Count > 0)
        {
            foreach (var choice in node.Choices)
            {
                Button button = Instantiate(ChoiceButtonPrefab, ChoiceButtonContainer);

                TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = choice.ChoiceText;
                }

                if (button != null)
                {
                    button.onClick.AddListener(() =>
                    {
                        if (!string.IsNullOrEmpty(choice.DestinationNodeID))
                        {
                            if (choice.HumanityChange != 0)
                            {
                                Debug.Log("Changing Humanity");
                                TriggerHumanityChange(choice.HumanityChange);
                            }
                            ShowNode(choice.DestinationNodeID);
                        }
                        else
                        {
                            StartCoroutine(EndDialogue());
                        }
                    });
                }
            }
        }
    }*/

    /*#region Event Subscribing
private void OnEnable()
{
    GameEvents.Dialogue.AddListener(StartDialogue);
}
private void OnDisable()
{
    GameEvents.Dialogue.RemoveListener(StartDialogue);
}
#endregion*/
    #endregion
}
