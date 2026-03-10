using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

public class RuntimeDialogueGraph : ScriptableObject
{
    public string EntryNodeID;
    public List<RunTimeDialogueNode> AllNodes = new List<RunTimeDialogueNode>();
}

[Serializable]
public class RunTimeDialogueNode
{
    public string NodeID;
    public Speaker speaker;
    public string DialogueText;
    public string NextNodeID;

    //Choice Nodes
    public List<ChoiceData> Choices = new List<ChoiceData>();

    //Optional
    public int HumanityChange;

    //Action Nodes
    public Item CorrectItem;
    public string SuccesText;
    public string FailureText;
}

[Serializable]
public class ChoiceData
{
    public string ChoiceText;
    public string DestinationNodeID;
    public int HumanityChange;
}

[CreateAssetMenu(menuName = "Dialogue/Create new Speaker")]
[Serializable]
public class Speaker : ScriptableObject
{
    public Speakers speakerName;

    public Sprite SpeakerSprite;
}

public enum Speakers
{
    Dhampir, Rookie_Officer, Narrator, Drunk_Priest
}

