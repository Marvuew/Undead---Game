using Assets.Scripts;
using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class DialogueManager
{
    public static DialogueManager instance;

    DialogueNode currentNode;

    void Awake() { instance = this; }
    private DialogueManager() { }
    public static void SetUpDialogueManager() 
    {
        if (instance != null)
            return;
        instance = new DialogueManager();
    }
    public void StartDialogue(DialogueNode startNode)
    {
        currentNode = startNode;
        Console.WriteLine($"currentNode is {(currentNode!=null)}");
        ShowNode();
    }

    void ShowNode()
    {
        GameManager.instance.dialogueBox.SetActive(true);
        GameManager.instance.dialogueTxt.text = currentNode.text;
        GameManager.instance.speakerTxt.text = currentNode.speaker;
        DisplayChoices();
    }

    void DisplayChoices()
    {
        if (currentNode.choices.Count == 0)
            return;

        GameManager.instance.optionsBox.SetActive(true);

        GameManager.instance.optionATxt.text = currentNode.choices[0].text;
        GameManager.instance.optionBTxt.text = currentNode.choices[1].text;
    }

    public void ChooseOption(int index)
    {
        Choice choice = currentNode.choices[index];

        Player.Instance.alignment += choice.alignmentChange;

        currentNode = choice.nextNode;

        ShowNode();
    }
}
