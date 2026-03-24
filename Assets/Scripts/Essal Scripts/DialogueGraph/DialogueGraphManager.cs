using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
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

    [Header("Text Settings")]
    [Tooltip("0.05 is good")]
    public float typingSpeed;

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
            if (isTyping)
            {
                if (!skipTyping && !isWaitingForClick) skipTyping = true;
                else if (isWaitingForClick) isWaitingForClick = false;
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

            string nextNode = _currentNode.Execute(this);

            // 🛑 Dialogue pauses the system
            if (_currentNode is RuntimeDialogueNode)
                return;

            nodeID = nextNode;
        }
    }


    private IEnumerator EndDialogue()
    {
        StopAllCoroutines();
        DialogueAnimator.SetBool("IsOpen", false);
        yield return new WaitForSeconds(1f);
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

        Sprite SpeakerSprite = GetSpeakerSprite(node.SpeakerName);
        HandleSpeakerData(SpeakerSprite, node.SpeakerName);

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

    #endregion
    #region Helping Functions

    IEnumerator TypeDialogue(List<string> dialogue, RuntimeDialogueNode node)
    {
        isTyping = true;
        foreach (string sentence in dialogue)
        {
            DialogueText.text = "";
            skipTyping = false;
            isWaitingForClick = false;
            foreach (char letter in sentence.ToCharArray())
            {
                DialogueText.text += letter;

                float timer = 0f;
                while (timer < typingSpeed)
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
            isWaitingForClick = true;

            yield return new WaitUntil(() => !isWaitingForClick);
        }
            isTyping = false;

            if (node.Choices.Count > 0) ListChoices(node);

    }

    public void ListChoices(RuntimeDialogueNode node)
    {
        ClearChoices();
        foreach (var choice in node.Choices)
        {

            if (!ViableChoice(choice)) return;
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

    void HandleSpeakerData(Sprite sprite, Speakers name)
    {
        if (sprite != null)
        {
            if (SpeakerSpriteContainer.gameObject.activeSelf == false)
            {
                SpeakerSpriteContainer.gameObject.SetActive(true);
            }
            SpeakerNameText.text = name.ToString();
            SpeakerSprite.sprite = sprite;
        }
        if (sprite == null)
        {
            SpeakerSpriteContainer.gameObject.SetActive(false);
            SpeakerNameText.text = "";
        }
    }

    public Sprite GetSpeakerSprite(Speakers speaker)
    {
        if (SpeakerToSpriteHandler.instance.speakerSprites.TryGetValue(speaker, out Sprite sprite))
        {
            return sprite;
        }
        else
        {
            if (speaker == Speakers.None)
            {
                return null;
            }
            Debug.LogWarning($"No sprite found for speaker {speaker}");
            return null;
        }
    }

    private void ClearChoices()
    {
        foreach (Transform child in ChoiceButtonContainer)
        {
            Destroy(child.gameObject);
        }
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
