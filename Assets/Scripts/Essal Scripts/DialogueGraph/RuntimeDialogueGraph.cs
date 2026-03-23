using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using JetBrains.Annotations;
using Unity.GraphToolkit.Editor;

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
    public Sprite SpeakerSprite;
    public Speakers SpeakerName;

    //Choices
    public List<ChoiceData> Choices = new List<ChoiceData>();

    public override string Execute(DialogueGraphManager manager)
    {
        manager.HandleDialogueNode(this);
        return NextNodeID;
    }
}

[Serializable]
public class RuntimeAlignmentNode : RuntimeNode
{
    public int HumanityChange;
    public int UndeadChange;

    public override string Execute(DialogueGraphManager manager)
    {
        manager.HandleAlignmentNode(this);
        return NextNodeID;
    }
}

[Serializable]
public class RuntimeActionNode : RuntimeNode
{
    public string eventName;
    public override string Execute(DialogueGraphManager manager)
    {
        manager.HandleActionNode(this);
        return NextNodeID;
    }
}

#endregion

#region Data Containers
// Container for choicedata
[Serializable]
public class ChoiceData
{
    public string ChoiceText;
    public string DestinationNodeID;
    public string ChoiceID;
    public DialogueCondition Condition;
}
#endregion

#region Speaker 

// Enum for all speaker
public enum Speakers
{
    Dhampir, Rookie_Officer, Narrator, Drunk_Priest, Gravedigger, Strigoi, None
}

public enum Actions
{
    Kill,
    Resolve
}

public enum Requirement
{
    Axe, Wine, Book
}

#endregion

#region Legacy Nodes

/*[Serializable] 
public class RuntimeActionNode: RuntimeNode
{
    public GameEvent Action;
    public override string Execute(DialogueGraphManager manager)
    {
        manager.HandleActionNode(this);
        return NextNodeID;
    }
}*/
/*[CreateAssetMenu(menuName = "Events/Game Event")]
public class GameEvent : ScriptableObject
{
    // This is the "Basic Event" (C# Action)
    private Action<object> _onTrigger;

    public void Register(Action<object> listener) => _onTrigger += listener;
    public void Unregister(Action<object> listener) => _onTrigger -= listener;

    public void Raise(object data = null)
    {
        _onTrigger?.Invoke(data);
    }
}*/

/*[Serializable]
public class RuntimeinteractionNode : RuntimeNode
{
    public string Name;
    public List<string> FluffText;
    public Sprite Image;

    public override string Execute(DialogueGraphManager manager)
    {
        manager.HandleInteractionNode(this);
        return NextNodeID;
    }
}*/


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

