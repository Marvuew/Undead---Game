using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.Timeline.Actions;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

    
public class DialogueGraphManager : MonoBehaviour
{
    #region Singleton Pattern
    public static DialogueGraphManager instance;

    void Awake()
    {
        instance = this;
    }
    #endregion

    #region Inspector
    public RuntimeDialogueGraph RuntimeGraph;

    [Header("UI Components")]
    public GameObject DialoguePanel;
    public TextMeshProUGUI SpeakerNameText;
    public TextMeshProUGUI DialogueText;

    [Header("Speaker UI")]
    public Image SpeakerSprite;
    public Transform SpeakerSpriteContainer;

    [Header("Choice Button UI")]
    public Button ChoiceButtonPrefab;
    public Transform ChoiceButtonContainer;

    [Header("Typing Speeds")]
    [Tooltip("0.05 is good")]
    public float Slow;
    public float Mid;
    public float Fast;

    [Header("Animator")]
    public Animator DialogueAnimator;
    #endregion

    #region Variables
    // For controlling dialogue flow
    private bool skipTyping = false;
    private bool isWaitingForClick = false;
    private bool isTyping = false;

    private Dictionary<string, RuntimeNode> _nodeLookup = new Dictionary<string, RuntimeNode>();
    private RuntimeNode _currentNode;
    
    // For tracking choices
    private HashSet<string> exploredChoices = new HashSet<string>();
    #endregion

    #region Event Subscribing
    private void OnEnable()
    {
        GameEvents.Dialogue.AddListener(StartDialogue);
    }
    private void OnDisable()
    {
        GameEvents.Dialogue.RemoveListener(StartDialogue);
    }
    #endregion

    #region Input Handling (update)
    private void Update()
    {   // Runs if the current node is and end node.
        if (_currentNode == null)
        {
            if (DialoguePanel.activeSelf && !isTyping)
            {
                StartCoroutine(EndDialogue());
            }
            return;
        }

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (isTyping && DialoguePanel.activeSelf)
            {
                if (!skipTyping) skipTyping = true;
            }
            else
            {
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
            StartCoroutine(EndDialogue());
        }
        // The DialougeBox Appears
        DialogueAnimator.SetBool("IsOpen", true);
    }

    public void ShowNode(string nodeID)
    {
        while (nodeID != null)
        {
            if (!_nodeLookup.ContainsKey(nodeID))
            {
                StartCoroutine(EndDialogue());
                return;
            }

            _currentNode = _nodeLookup[nodeID];

            if (_currentNode.RuntimeMarkAsRead)
            {
                ShowNode(_currentNode.MarkAsReadNodeID);
                return;
            }

            if (_currentNode is RuntimeDialogueNode)
            {
                if (!ViableNode(_currentNode as RuntimeDialogueNode))
                {
                    ShowNode(_currentNode.ConditionFailNodeID);
                    return;
                }
            }

            string nextNode = _currentNode.Execute(this);

            if (_currentNode is RuntimeDialogueNode)
                return;

            nodeID = nextNode;
        }
    }


    private IEnumerator EndDialogue()
    {
        DialogueAnimator.SetBool("IsOpen", false);
        yield return new WaitForSeconds(0.5f);
        DialoguePanel.SetActive(false);
        _currentNode = null;

        foreach(Transform child in ChoiceButtonContainer)
        {
            Destroy(child.gameObject);
        }
        StopAllCoroutines();
        yield return null;
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
        GameEvents.ChangeAlignment(node.HumanityChange, node.UndeadChange);
    }

    public void HandleActionNode(RuntimeActionNode node)
    {
        node.Action.DoAction();
    }

    public void HandleRandomizer(RuntimeRandomizer node)
    {
        int randomIndex = Random.Range(0, node.randomNextNodeID.Count);
        node.NextNodeID = node.randomNextNodeID[randomIndex];
    }

    #endregion
    #region Helping Functions

    IEnumerator TypeDialogue(List<string> dialogue, RuntimeDialogueNode node)
    {
        float _typingSpeed = HandleTypingSpeed(node.TypingSpeed);
        isTyping = true;
        yield return null; // Wait a frame to ensure UI updates before typing starts
        foreach (string sentence in dialogue)
        {
            DialogueText.text = "";
            skipTyping = false;
            foreach (char letter in sentence.ToCharArray())
            {
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
                    break;
                }
            }
            skipTyping = false;

            yield return new WaitUntil(() => !Mouse.current.leftButton.isPressed);
            yield return new WaitUntil(() => Mouse.current.leftButton.wasPressedThisFrame);

        }
        isTyping = false;
        if (node.MarkAsRead != false)
        {
            MarkAsRead(node);
        }

        if (node.Choices.Count > 0) ListChoices(node);
    }

    public void ListChoices(RuntimeDialogueNode node)
    {
        ClearChoices();
        foreach (var choice in node.Choices)
        {

            if (!ViableChoice(choice)) continue;
            Button button = Instantiate(ChoiceButtonPrefab, ChoiceButtonContainer);
            button.GetComponentInChildren<TextMeshProUGUI>().text = choice.ChoiceText;

            //------------------CHOICE COLOR_________________________________//
            var choiceColor = button.GetComponent<Image>().color;
            choiceColor = choice.Condition == null ? Color.white : Color.yellow;
            if (exploredChoices.Contains(choice.ChoiceID))
            {
                choiceColor = Color.red;
            }
            button.GetComponent<Image>().color = choiceColor;
            //_____________________________________________________________//

            button.onClick.AddListener(() =>
            {
                if (!string.IsNullOrEmpty(choice.DestinationNodeID))
                {
                    exploredChoices.Add(choice.ChoiceID);

                    ClearChoices();

                    ShowNode(choice.DestinationNodeID);
                }
            });
        }
    }

    bool ViableChoice(ChoiceData choice)
    {
        if (choice.Condition == null) return true;
        if (choice.Condition.IsMet()) return true;
        else return false;
    }

    bool ViableNode(RuntimeDialogueNode node)
    {
        if (node.NodeCondition == null) return true;
        if (node.NodeCondition.IsMet()) return true;
        else return false;
    }

    public void MarkAsRead(RuntimeDialogueNode node)
    {
        node.RuntimeMarkAsRead = node.MarkAsRead;
    }

    void HandleSpeakerData(RuntimeDialogueNode node)
    {
        // if there is no speaker attaches then it disables the speaker and sets the text to an empty string.
        if (node.Speaker == null)
        {
            SpeakerNameText.text = "";
            SpeakerSprite.enabled = false;
        }
        else
        {
            SpeakerSprite.enabled = true;
            SpeakerNameText.text = node.Speaker.SpeakerName;
            HandleEmotion(node.Emotion, node);
        }
    }

    void HandleEmotion(Emotion emotion, RuntimeDialogueNode node)
    {
        switch (emotion)
        {
            case Emotion.Angry:
                SpeakerSprite.sprite = node.Speaker.Angry;
                break;
            case Emotion.Happy:
                SpeakerSprite.sprite = node.Speaker.Happy;
                    break;
            case Emotion.Content:
                SpeakerSprite.sprite = node.Speaker.Content;
                break;
            case Emotion.Sad:
                SpeakerSprite.sprite = node.Speaker.Sad;
                break;
            default:
                Debug.LogWarning("Setting speaker to content!");
                SpeakerSprite.sprite = node.Speaker.Content;
                break;
        }
    }

    float HandleTypingSpeed(TypingSpeed typingSpeed)
    {
        // It sets typing speed relative to the chosen enum. By default it sets it to 0.05f.
        float _typingSpeed;
        switch (typingSpeed)
        {
            case TypingSpeed.Slow:
                _typingSpeed = Slow;
                break;
            case TypingSpeed.Mid:
                _typingSpeed = Mid;
                break;
            case TypingSpeed.Fast:
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
        foreach (Transform child in ChoiceButtonContainer)
        {
            Destroy(child.gameObject);
        }
    }

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
    #endregion
}
