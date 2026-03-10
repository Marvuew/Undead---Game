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


    private Dictionary<string, RunTimeDialogueNode> _nodeLookup = new Dictionary<string, RunTimeDialogueNode>();
    private RunTimeDialogueNode _currentNode;

    private void OnEnable()
    {
        GameEvents.Dialogue.AddListener(StartDialogue);
    }

    private void OnDisable()
    {
        GameEvents.Dialogue.AddListener(StartDialogue);
    }

    public void StartDialogue(RuntimeDialogueGraph dialogue)
    {
        
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
            }
        }
        if (Mouse.current.leftButton.wasPressedThisFrame && _currentNode != null && _currentNode.Choices.Count == 0)
        {
            Debug.Log("Clicked Mouse");
            ShowNode(_currentNode.NextNodeID);
        }
        else if (_currentNode == null)
        {
            StartCoroutine(EndDialogue());
        }
    }

    private void ShowNode(string nodeID)
    {
        Debug.Log($"Showing node {nodeID}");
        if (!_nodeLookup.ContainsKey(nodeID))
        {
            Debug.Log("Ending Dialogue");
            StartCoroutine(EndDialogue());
            return;
        }

        _currentNode = _nodeLookup[nodeID];


        if (_currentNode.CorrectItem != null)
        {
            GiveItem(_currentNode.CorrectItem);
        }
        else
        {

            DialoguePanel.SetActive(true);
            SpeakerNameText.SetText(_currentNode.speaker.speakerName.ToString());
            StopAllCoroutines();
            StartCoroutine(TypeDialogue(_currentNode.DialogueText));
            //DialogueText.SetText(_currentNode.DialogueText);
            HandleSpeakerSprite(_currentNode.speaker.SpeakerSprite);

            // Triggers humanity change if it is anything than zero.
            if (_currentNode.HumanityChange != 0)
            {
                TriggerHumanityChange(_currentNode.HumanityChange);
            }


            foreach (Transform child in ChoiceButtonContainer)
            {
                Destroy(child.gameObject);
            }

            if (_currentNode.Choices.Count > 0)
            {
                foreach (var choice in _currentNode.Choices)
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
    }

    void HandleSpeakerSprite(Sprite sprite)
    {
        SpeakerSprite.sprite = sprite;
    }

    public void GiveItem(Item item)
    {
        if (!InventoryManager.Instance.Items.Contains(item))
        {
            Debug.LogWarning("You dont have that item");
            StopAllCoroutines();
            StartCoroutine(TypeDialogue(_currentNode.FailureText));
            GameEvents.ChangeHumanity(-10);
        }
        else
        {
            InventoryManager.Instance.Items.Remove(item);
            StopAllCoroutines();
            StartCoroutine(TypeDialogue(_currentNode.SuccesText));
            GameEvents.ChangeHumanity(10);
        }
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
