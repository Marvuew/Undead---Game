using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.Timeline.Actions;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;

    
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

    [Header("Choice Button UI")]
    public Button ChoiceButtonPrefab;
    public Transform ChoiceButtonContainer;

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
    private HashSet<string> exploredChoices = new HashSet<string>();
    #endregion


    #region Input Handling (update)
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


        if (Mouse.current.leftButton.wasPressedThisFrame)
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
        // Runs while handling the node
        while (nodeID != null)
        {
            // If it encounters a wrong key to node mapping => end dialogue
            if (!_nodeLookup.ContainsKey(nodeID))
            {
                EndDialogue();
                return;
            }

            // Set the current node
            _currentNode = _nodeLookup[nodeID];

            // If it has the MarkAsRead attribute lead to another node saying like: You already asked me that or.. smth...
            if (_currentNode.RuntimeMarkAsRead)
            {
                ShowNode(_currentNode.MarkAsReadNodeID);
                return;
            }

            // Show the node if its a viable one: If it has the right requirements met.
            if (_currentNode is RuntimeDialogueNode)
            {
                if (!ViableNode(_currentNode as RuntimeDialogueNode))
                {
                    ShowNode(_currentNode.ConditionFailNodeID);
                    return;
                }
            }

            // set the nexNodeID and execute the nodes implementation of execute. For dialogue it calls the HandleDialogueNode, for AlignmentNodes it calls the Handle Alignment etc...
            string nextNode = _currentNode.Execute(this);

            // Lastly catch dialogue nodes so it waits until that node it finished. That will be handled in the typedialogue function.
            if (_currentNode is RuntimeDialogueNode)
            {
                return;
            }

            // set the next node so this loop will continue with the next node.
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
        int randomIndex = UnityEngine.Random.Range(0, node.randomNextNodeID.Count);
        node.NextNodeID = node.randomNextNodeID[randomIndex];
    }

    public void HandleClueNode(RuntimeClueNode node)
    {
        CaseManager.Instance.OnClueFound(node.clue);
        Debug.Log("Handling the clue");
    }

    #endregion

    #region Helping Functions

    IEnumerator TypeDialogue(List<string> dialogue, RuntimeDialogueNode node)
    {
        // Find the typingspeed
        float _typingSpeed = HandleTypingSpeed(node.TypingSpeed);
        isTyping = true;

        // Set the text at the top where the name should be.
        if (node.Speaker == null)
        {
            DialogueText.transform.position = SpeakerTextY;
            DialogueText.transform.position = new Vector3(DialogueText.transform.position.x, DialogueText.transform.position.y + 60f, DialogueText.transform.position.z);
        }
        else
        {
            DialogueText.transform.position = SpeakerTextY;
        }

        //yield return null; // Wait a frame to ensure UI updates before typing starts
        foreach (string sentence in dialogue)
        {

            DialogueText.text = "";
            skipTyping = false;
            // Type each letter step by step according to typingspeed
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
                // Set the text if player has clicked
                if (skipTyping)
                {
                    DialogueText.text = sentence;
                    break;
                }
            }
            skipTyping = false;

            // Wait until the mouse is up and then you can continue to the next node.
            yield return new WaitUntil(() => !Mouse.current.leftButton.isPressed);
            yield return new WaitUntil(() => Mouse.current.leftButton.wasPressedThisFrame);

        }
        isTyping = false;
        //Mark Node as MarkAsRead if the node should be deletede for second dialogue playthroughs
        if (node.MarkAsRead == true)
        {
            MarkAsRead(node);
        }

        //Now list choice
        if (node.Choices.Count > 0) ListChoices(node);
    }

    public void ListChoices(RuntimeDialogueNode node)
    {
        ClearChoices();
        foreach (var choice in node.Choices)
        {
            // if it isnt a viable choice return
            if (!ViableChoice(choice)) continue;
            // Instantiate the button and set the text
            Button button = Instantiate(ChoiceButtonPrefab, ChoiceButtonContainer);
            button.GetComponentInChildren<TextMeshProUGUI>().text = choice.ChoiceText;

            // Handle the Color
            // If its an unlockable choice set the color to yellow.
            var choiceColor = button.GetComponent<Image>().color;
            choiceColor = choice.Condition == null ? choiceColor : Color.yellow;
            if (exploredChoices.Contains(choice.ChoiceID))
            {
                //Set the color to red if it is alreade explored.
                choiceColor = Color.red;
            }
            button.GetComponent<Image>().color = choiceColor;
            
            // Add an OnClick Event
            button.onClick.AddListener(() =>
            {
                if (!string.IsNullOrEmpty(choice.DestinationNodeID))
                {
                    // Add it to the hashset of explored choices
                    exploredChoices.Add(choice.ChoiceID);

                    ClearChoices();

                    // Call the the next node with the designated nextnodeID
                    ShowNode(choice.DestinationNodeID);
                }
            });
        }
    }

    // Check if its a viable choice. Otherwise dont show the Choice
    bool ViableChoice(ChoiceData choice)
    {
        if (choice.Condition == null) return true;
        if (choice.Condition.IsMet()) return true;
        else return false;
    }

    // Unlockable Dialogue Nodes. If its node Viable there should be designed another fail node to run instead.
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

    // Set the Sprite in relation to the given emotion.
    void HandleEmotion(Emotion emotion, RuntimeDialogueNode node)
    {
        switch (emotion)
        {
            case Emotion.Angry:
                SpeakerSprite.sprite = node.Speaker.Angry;
                SpeakerSprite.preserveAspect = true;
                break;
            case Emotion.Happy:
                SpeakerSprite.sprite = node.Speaker.Happy;
                SpeakerSprite.preserveAspect = true;
                break;
            case Emotion.Content:
                SpeakerSprite.sprite = node.Speaker.Content;
                SpeakerSprite.preserveAspect = true;
                break;
            case Emotion.Sad:
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

    // Delete all choicebuttons
    private void ClearChoices()
    {
        foreach (Transform child in ChoiceButtonContainer)
        {
            Destroy(child.gameObject);
        }
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
