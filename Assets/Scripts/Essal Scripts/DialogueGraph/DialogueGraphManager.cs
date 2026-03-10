using System.Collections.Generic;
using System.Text;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR;
using System.Threading.Tasks;
using System.Collections;
using System.Runtime.CompilerServices;

    
public class DialogueGraphManager : MonoBehaviour
{
    public static DialogueGraphManager instance;

    void Awake()
    {
        instance = this;
    }

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

    private bool skipTyping = false;


    private Dictionary<string, RuntimeNode> _nodeLookup = new Dictionary<string, RuntimeNode>();
    private RuntimeNode _currentNode;

    private void OnEnable()
    {
        GameEvents.Dialogue.AddListener(StartDialogue);
    }

    private void OnDisable()
    {
        GameEvents.Dialogue.RemoveListener(StartDialogue);
    }

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

        DialogueAnimator.SetBool("IsOpen", true);
    }

    public void TriggerHumanityChange(int humanity)
    {
        GameEvents.ChangeHumanity(humanity);
    }

    private void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame && _currentNode != null)
        {
            if (!skipTyping)
            {
                skipTyping = true;
                return;
            }
            else if (_currentNode is RuntimeChoiceNode choiceNode)
            {
                if (choiceNode.Choices.Count > 0)
                {
                    Debug.Log("Clicked Mouse");
                    ShowNode(_currentNode.NextNodeID);
                }
            }
        }
        else if (_currentNode == null)
        {
            StartCoroutine(EndDialogue());
        }
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
        _currentNode = _nodeLookup[nodeID];

        string nextNode = _currentNode.Execute(this);    

        if (!string.IsNullOrEmpty(nextNode))
        {
            ShowNode(nextNode);
        }
    }

    public void ShowChoices(RuntimeChoiceNode node)
    {
        DialoguePanel.SetActive(true);

        SpeakerNameText.text = node.speaker.speakerName.ToString();

        HandleSpeakerSprite(node.speaker.SpeakerSprite);

        StopAllCoroutines();
        StartCoroutine(TypeDialogue(node.DialogueText));

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
    }

    public void ShowDialogue(RuntimeDialogueNode node)
    {
        DialoguePanel.SetActive(true);

        SpeakerNameText.text = node.speaker.speakerName.ToString();

        HandleSpeakerSprite(node.speaker.SpeakerSprite);

        StopAllCoroutines();
        StartCoroutine(TypeDialogue(node.DialogueText));
    }

    void HandleSpeakerSprite(Sprite sprite)
    {
        if (sprite != null)
            SpeakerSprite.sprite = sprite;
    }

    IEnumerator TypeDialogue(string dialogue)
    {
        DialogueText.text = "";
        skipTyping = false;
        foreach (char letter in dialogue.ToCharArray())
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
                DialogueText.text = dialogue;
                break;
            }
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
}
