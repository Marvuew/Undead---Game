using System;
using UnityEngine;
using System.Collections.Generic;

public class RuntimeDialogueGraph : ScriptableObject
{
    public string EntryNodeID;
    [SerializeReference]
    public List<RuntimeNode> AllNodes = new List<RuntimeNode>();
}

#region Nodes
// Abstract class for all nodes to derive from. So we use polymorphism to virtually disptach each nodes methods.
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
public class RuntimeDialogueNode : RuntimeNode
{
    // Dialogue
    public List<string> Dialogue = new List<string>();
    public Speaker speaker;

    //Choices
    public List<ChoiceData> Choices = new List<ChoiceData>();

    public override string Execute(DialogueGraphManager manager)
    {
        manager.ShowDialogue(this);
        return NextNodeID;
    }
}

#endregion
// Container for choicedata
[Serializable]
public class ChoiceData
{
    public string ChoiceText;
    public string DestinationNodeID;
    public int HumanityChange;
    public int UndeadChange;
}

#region Speaker 
//Scriptable Object for a speaker
[CreateAssetMenu(menuName = "Dialogue/Create new Speaker")]
[Serializable]
public class Speaker : ScriptableObject
{
    public Speakers speakerName;

    public Sprite SpeakerSprite;
}

// Enum for all speaker
public enum Speakers
{
    Dhampir, Rookie_Officer, Narrator, Drunk_Priest, Gravedigger
}
#endregion

#region Legacy Nodes

/*[Serializable]
public class RuntimeHumanityNode : RuntimeNode
{
    public int HumanityChange;

    public override string Execute(DialogueGraphManager manager)
    {
        GameEvents.ChangeHumanity(HumanityChange);
        return NextNodeID;
    }
}*/


/*[Serializable]
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
}*/

/*[Serializable]
public class RuntimeChoiceNode : RuntimeNode
{
    public Speaker speaker;
    public List<string> Dialogue;
    public List<ChoiceData> Choices = new List<ChoiceData>();

    public override string Execute(DialogueGraphManager manager)
    {
        manager.ShowChoices(this);
        return null;
    }
}*/
#endregion

