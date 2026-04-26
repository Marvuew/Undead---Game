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
    public string ConditionFailNodeID;
    public string ConditionSuccessNodeID;
    public string MarkAsReadNodeID;
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
    public DialogueSpeaker Speaker;
    public Emotion Emotion;
    public TypingSpeed TypingSpeed;

    // Node Condition
    public ConditionOptions condition;
    public int conditionHumanity;
    public int conditionUndead;
    public DialogueSpeaker conditionSpeaker;
    public Clue conditionClue;
    public Callback callbackCondition;
    public bool conditionToggle;

    // Mark as Read
    public bool MarkAsRead;

    //Choices
    public List<ChoiceData> Choices = new List<ChoiceData>();

    //Callbacks
    public List<CallbackData> Callbacks = new List<CallbackData>();
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
    public DialogueAction Action;
    public override string Execute(DialogueGraphManager manager)
    {
        manager.HandleActionNode(this);
        return NextNodeID;
    }
}
[Serializable]
public class RuntimeRandomizer : RuntimeNode
{
    public List<string> randomNextNodeID = new();
    public override string Execute(DialogueGraphManager manager)
    {
        manager.HandleRandomizer(this);
        return NextNodeID;
    }
}

[Serializable]
public class RuntimeClueNode : RuntimeNode
{
    public Clue clue;

    public override string Execute(DialogueGraphManager manager)
    {
        manager.HandleClueNode(this);
        return NextNodeID;
    }
}

[Serializable]
public class RuntimeCallBackNode : RuntimeNode
{
    public Callback callback;
    
    // Registers Callback to a hashset
    public override string Execute(DialogueGraphManager manager)
    {
        manager.CallbackLookup.Add(callback);
        return NextNodeID;
    }
}
[Serializable]
public class RuntimeTalkWillingnessNode : RuntimeNode
{
    public DialogueSpeaker Speaker;
    public TalkWillingNessEnum IsWillingToTalk;

    public override string Execute(DialogueGraphManager manager)
    {
        manager.HandleTalkWillingnessNode(this);
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


    // Condtions
    public ConditionOptions condition;
    public int choiceHumanityCondtion;
    public int choiceUndeadCondtion;
    public DialogueSpeaker choiceConditionSpeaker;
    public Clue choiceConditionClue;
    public Callback choiceConditionCallback;
    public bool conditionToggled;
}

[Serializable]
public class CallbackData
{
    public Callback CallbackAsset;
    public string Sentence;
    public int Index;
    public bool Replace;
}

#endregion

#region Enums


public enum Emotion
{
    HAPPY, SAD, CONTENT, ANGRY
}

public enum TypingSpeed
{
    SLOW, MID, FAST
}

public enum TalkWillingNessEnum
{
    WILLING, NOT_WILLING
}

public enum ConditionOptions
{
    ALIGNMENT, CLUE, WILLING_TO_TALK, CALLBACK, NONE
}

#endregion

#region Legacy Nodes

/*[Serializable]
public class Callback
{
    public DialogueSpeaker speaker;
    public string CallBackSentence;
}*/
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

