using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections;

    
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
    private bool skipTyping = false;
    private bool isWaitingForClick = false;
    private bool isTyping = false;

    private Dictionary<string, RuntimeNode> _nodeLookup = new Dictionary<string, RuntimeNode>();
    private RuntimeNode _currentNode;
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

    /*private void Update()
    {
        if (_currentNode != null)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                if (isTyping)
                {
                    if (!skipTyping && !isWaitingForClick) skipTyping = true;
                    else if (isWaitingForClick) isWaitingForClick = false;
                }
                else if (DialoguePanel.activeSelf && !isTyping)
                {
                    ShowNode(_currentNode.NextNodeID);
                }
            }
        }
        else if (DialoguePanel.activeSelf && !isTyping)
        {
            StartCoroutine(EndDialogue());
        }
    }*/

    private void Update()
    {
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
            // A: Hvis vi stadig skriver tekst
            if (isTyping)
            {
                if (!skipTyping && !isWaitingForClick) skipTyping = true;
                else if (isWaitingForClick) isWaitingForClick = false;
            }
            // B: Hvis teksten er færdig - Tjek om vi må klikke videre
            else
            {
                // Vi må KUN klikke videre, hvis der ikke er nogen svarknapper (Choices)
                if (_currentNode is RuntimeDialogueNode dialogueNode && dialogueNode.Choices.Count == 0)
                {
                    ShowNode(_currentNode.NextNodeID);
                }
                // Hvis der ER choices, gør vi intet her. 
                // Vi venter på at OnClick() på knappen kalder ShowNode.
            }
        }
    }
    #endregion

    #region Dialogue Methods
    public void StartDialogue(RuntimeDialogueGraph dialogue)
    {
        _nodeLookup.Clear();
        
        foreach (var node in dialogue.AllNodes)
        {
            Debug.Log("All Nodes Added");
            _nodeLookup[node.NodeID] = node;
        }

        if (!string.IsNullOrEmpty(dialogue.EntryNodeID))
        {
            Debug.Log("Showing Entry Node");
            ShowNode(dialogue.EntryNodeID);
        }
        else
        {
            StartCoroutine(EndDialogue());
        }

        DialogueAnimator.SetBool("IsOpen", true);
    }

    public void ShowNode(string nodeID)
    {
        Debug.Log($"Showing node {nodeID}");
        if (!_nodeLookup.ContainsKey(nodeID))
        {
            Debug.Log("Ending Dialogue");
            StartCoroutine(EndDialogue());
            return;
        }
        Debug.Log("Updating CurrentNode");
        _currentNode = _nodeLookup[nodeID];

        string nextNode = _currentNode.Execute(this);    
    }

    public void ShowDialogue(RuntimeDialogueNode node)
    {
        Debug.Log("Showing Dialogue");
        DialoguePanel.SetActive(true);

        SpeakerNameText.text = node.speaker.speakerName.ToString();

        HandleSpeakerSprite(node.speaker.SpeakerSprite);

        StopAllCoroutines();
        StartCoroutine(TypeDialogue(node.Dialogue, node));
    }

    public void ListChoices(RuntimeDialogueNode node)
    {
        ClearChoices();
            foreach (var choice in node.Choices)
            {
                Button button = Instantiate(ChoiceButtonPrefab, ChoiceButtonContainer);

                TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
                button.GetComponentInChildren<TextMeshProUGUI>().text = choice.ChoiceText;

                    button.onClick.AddListener(() =>
                    {
                        if (!string.IsNullOrEmpty(choice.DestinationNodeID))
                        {
                            TriggerHumanityChange(choice.HumanityChange);

                            ClearChoices();

                            ShowNode(choice.DestinationNodeID);
                        }
                        /*else
                        {
                            StartCoroutine(EndDialogue());
                        }*/
                    });  
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

    void HandleSpeakerSprite(Sprite sprite)
    {
        if (sprite != null)
            SpeakerSprite.sprite = sprite;
    }

    void TriggerHumanityChange(int humanityChange)
    {
        Debug.Log("Changing Humanity");
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
