using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SceneManager : MonoBehaviour
{

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI speakerText;
    [SerializeField] private TextMeshProUGUI dialogueText;

    [Header("Choice Buttons (assign 3)")]
    [SerializeField] private Button[] choiceButtons;
    [SerializeField] private TextMeshProUGUI[] choiceLabels;

    [Header("Extra Buttons")]
    [SerializeField] private Button restartDialogueBtn;

    private DialogueTree tree;

    private void Start()
    {
        // Initialize the dialogue tree
        tree = DialogueTree.BuildScene2Tree();
        
        // Set up button listeners
        for (int i = 0; i < choiceButtons.Length; i++)
        {
            int index = i; // Capture the current index for the lambda
            choiceButtons[i].onClick.AddListener(() => OnChoiceSelected(index));
        }
        restartDialogueBtn.onClick.AddListener(RestartDialogue);
        DisplayCurrentNode();
    }
    void OnChoiceSelected(int index)
    {
        var node = tree.SelectChoice(index);

        

        DisplayCurrentNode();
    }
    void DisplayCurrentNode()
    {
        var node = tree.CurrentNode;

        speakerText.text = node.speaker;
        dialogueText.text = node.text;

        for (int i = 0; i < choiceButtons.Length; i++)
        {
            if (i < node.Children.Count)
            {
                choiceButtons[i].gameObject.SetActive(true);
                choiceLabels[i].text = node.Children[i].choiceLabel;
            }
            else
            {
                choiceButtons[i].gameObject.SetActive(false);
            }
        }

        if (node.isLeaf)
        {
            restartDialogueBtn.gameObject.SetActive(true);
        }

    }

    void RestartDialogue()
    {
        tree.Reset();
        restartDialogueBtn.gameObject.SetActive(false);
        DisplayCurrentNode();
    }
}




