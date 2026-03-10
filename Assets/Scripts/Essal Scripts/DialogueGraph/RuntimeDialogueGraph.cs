using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using System.Runtime.InteropServices.WindowsRuntime;

public class RuntimeDialogueGraph : ScriptableObject
{
    public string EntryNodeID;
    [SerializeReference]
    public List<RuntimeNode> AllNodes = new List<RuntimeNode>();
}


[Serializable]
public abstract class RuntimeNode
{
    public string NodeID;
    public string NextNodeID;

    public virtual string Execute(DialogueGraphManager manager)
    {
        return null;
    }
}

[Serializable]
public class RuntimeItemCheckNode : RuntimeNode
{
    public Item RequiredItem;
    public string SuccessNodeID;
    public string FailureNodeID;

    public override string Execute(DialogueGraphManager manager)
    {
        if (InventoryManager.Instance.Items.Contains(RequiredItem))
        {
            InventoryManager.Instance.Items.Remove(RequiredItem);
            return SuccessNodeID;
        }
        return FailureNodeID;
    }  
}

[Serializable]
public class RuntimeDialogueNode : RuntimeNode
{
    public string DialogueText;
    public Speaker speaker;
    public int HumanityChange;

    public override string Execute(DialogueGraphManager manager)
    {
        manager.ShowDialogue(this);
        return NextNodeID;
    }
}

[Serializable]
public class RuntimeHumanityNode : RuntimeNode
{
    public int HumanityChange;

    public override string Execute(DialogueGraphManager manager)
    {
        GameEvents.ChangeHumanity(HumanityChange);
        return NextNodeID;
    }
}

[Serializable]
public class RuntimeChoiceNode : RuntimeNode
{
    public Speaker speaker;
    public string DialogueText;
    public List<ChoiceData> Choices = new List<ChoiceData>();

    public override string Execute(DialogueGraphManager manager)
    {
        manager.ShowChoices(this);
        return null;
    }
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

