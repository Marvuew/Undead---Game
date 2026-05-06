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
[Serializable]
public abstract class RuntimeNode
{
    public string NodeID;
    public string NextNodeID;
    public string MarkAsReadNodeID;
    public virtual string Execute(DialogueGraphManager manager)
    {
        return null;
    }
}

[Serializable]
public class RuntimeDialogueNode : RuntimeNode
{
    public List<string> Dialogue = new List<string>();
    public DialogueSpeaker Speaker;
    public Emotion Emotion;
    public TypingSpeed TypingSpeed;
    public bool MarkAsRead; 
    public override string Execute(DialogueGraphManager manager)
    {
        manager.HandleDialogueNode(this);
        return null;
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
    public string description;
    public List<UndeadType> typePointers = new List<UndeadType>();
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
    public override string Execute(DialogueGraphManager manager)
    {
        manager.callbacksCollected.Add(callback);
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

[Serializable]
public class RuntimeChoiceNode : RuntimeNode
{
    public List<ChoiceData> choices = new List<ChoiceData>();

    public ConditionOptions condition;
    public int humanity;
    public int undead;
    public Callback callback;
    public Clue clue;
    public DialogueSpeaker TalkWillingnessTarget;
    public bool TalkWillingness;

    public override string Execute(DialogueGraphManager manager)
    {
        manager.HandleChoiceNode(this);
        return null; // FOR STOPING THE SHOWNODE METHOD IN THE DIALOUGE MANAGER - TO WAIT FOR PLAYER INPUT
    }
}

[Serializable]
public class RuntimeConditionNode : RuntimeNode
{
    public ConditionOptions condition;
    public int humanity;
    public int undead;
    public Callback callback;
    public Clue clue;
    public DialogueSpeaker TalkWillingnessTarget;
    public bool TalkWillingness;

    public string FailNodeID;
    public string SuccessNodeID;
    public override string Execute(DialogueGraphManager manager)
    {
        return manager.HandleConditionNode(this) ? SuccessNodeID : FailNodeID;
    }
}

[Serializable]
public class RuntimeSoundNode : RuntimeNode
{
    public AudioClip clip;
    public override string Execute(DialogueGraphManager manager)
    {
        manager.HandleSoundNode(this);
        return NextNodeID;
    }
}

[Serializable]
public class RuntimeFadeNode : RuntimeNode
{
    public float duration;
    public float stayBlackDuration;
    public Color color;
    public bool blockSpaceDuringFade = true;
    public override string Execute(DialogueGraphManager manager)
    {
        manager.HandleFadeNode(this);
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

