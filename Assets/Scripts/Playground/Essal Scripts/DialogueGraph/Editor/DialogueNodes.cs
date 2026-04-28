using JetBrains.Annotations;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using Unity.GraphToolkit.Editor;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using System.Collections.Generic;
using Codice.Client.BaseCommands.WkStatus.Printers;
using Unity.VisualScripting;

#region In and out
[Serializable]
public class StartNode : Node
{
    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        context.AddOutputPort("out").Build();
    }
}

[Serializable]
public class EndNode : Node
{
    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        context.AddInputPort("in").Build();
    }
}
#endregion

#region Nodes
[Serializable]
public class DialogueNode : Node
{
    public static readonly string IN_PORT = "in";
    public static readonly string OUT_PORT = "out";
    public static readonly string IN_PORT_SPEAKER = "Speaker";
    public static readonly string OUT_PORT_CHOICE = "Choice";
    public static readonly string IN_OPTION_SENTENCE_COUNT = "Number of sentences";
    public static readonly string IN_OPTION_SENTENCE = "Sentence ";
    public static readonly string IN_PORT_TYPING_SPEED = "Typing Speed";
    public static readonly string IN_PORT_EMOTION = "Emotion";

    public static readonly string IN_OPTION_MARK_AS_READ = "Mark as Read";
    public static readonly string OUT_PORT_MARK_AS_READ = "Marked as read out";

    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        context.AddInputPort(IN_PORT).Build(); // IN PORT
        context.AddOutputPort(OUT_PORT).Build(); // OUT PORT

        context.AddInputPort<DialogueSpeaker>(IN_PORT_SPEAKER).Build(); // SPEAKER PORT
        context.AddInputPort<Emotion>(IN_PORT_EMOTION).WithDefaultValue(Emotion.CONTENT).Build(); // SPEAKER EMOTION PORT
        context.AddInputPort<TypingSpeed>(IN_PORT_TYPING_SPEED).WithDefaultValue(TypingSpeed.MID).Build(); // TYPING SPEED PORT

        GetNodeOptionByName(IN_OPTION_SENTENCE_COUNT).TryGetValue(out int sentenceCount);
        for (int i = 0; i < sentenceCount; i++)
        {
            context.AddInputPort<string>(IN_OPTION_SENTENCE + i).AsTextArea().Build();         // SPAWN SENTENCES
        }

        GetNodeOptionByName(IN_OPTION_MARK_AS_READ).TryGetValue(out bool markAsRead);  
        if (markAsRead)
        {
            context.AddOutputPort(OUT_PORT_MARK_AS_READ).Build();      // MARK AS READ
        }
    }

    protected override void OnDefineOptions(IOptionDefinitionContext context)
    {
        context.AddOption<int>(IN_OPTION_SENTENCE_COUNT).WithDefaultValue(1).Build();         // NUMBER OF SENTENCES OPTION
        context.AddOption<bool>(IN_OPTION_MARK_AS_READ).WithDefaultValue(false).Build();         // MARK AS READ OPTION
    }

    public override void OnEnable()
    {
        Subtitle = "Dialogue Node for designing dialogue";
        DefaultColor = Color.green;
    }
}
[Serializable]
public class AlignmentNode : Node
{
    public static readonly string IN_PORT = "in";
    public static readonly string OUT_PORT = "out";
    public static readonly string IN_OPTION_HUMANITY = "HumanityChange";
    public static readonly string IN_OPTION_UNDEAD = "UndeadChange";
    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        context.AddInputPort(IN_PORT).Build();
        context.AddOutputPort(OUT_PORT).Build();

        context.AddInputPort<int>(IN_OPTION_UNDEAD).WithConnectorUI(PortConnectorUI.Arrowhead).WithDefaultValue(0).Build();
        context.AddInputPort<int>(IN_OPTION_HUMANITY).WithConnectorUI(PortConnectorUI.Circle).WithDefaultValue(0).Build();
    }
}

[Serializable]
public class ChoiceNode : Node
{
    public static readonly string IN_PORT = "in";
    public static readonly string OUT_PORT = "out ";

    public static readonly string IN_PORT_CHOICES = "Choice ";
    public static readonly string IN_OPTION_CONDITIONS = "Show Conditions";
    public static readonly string IN_OPTION_CHOICE_COUNT = "Number of choices";
    public static readonly string IN_OPTION_CHOICE_CONDITION_TYPE = "Condition Type ";
    public static readonly string IN_PORT_HUMANITY_CONDITION = "Humanity ";
    public static readonly string IN_PORT_UNDEAD_CONDITION = "Undead ";
    public static readonly string IN_PORT_CLUE_CONDITION = "Clue ";
    public static readonly string IN_PORT_CALLBACK_CONDITION = "Callback ";
    public static readonly string IN_PORT_IS_WILLING_TO_TALK_CONDITION = "Talk Willingness Target ";



    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        context.AddInputPort(IN_PORT).Build();

        GetNodeOptionByName(IN_OPTION_CHOICE_COUNT).TryGetValue(out int choiceCount); // GET CHOICECOUNT
        GetNodeOptionByName(IN_OPTION_CONDITIONS).TryGetValue(out bool showConditions); // GET SHOW CONDITION BOOL
        for (int i = 1; i < choiceCount + 1; i++)
        {
            context.AddOutputPort(OUT_PORT + i).Build(); // OUT PORT
            context.AddInputPort<string>(IN_PORT_CHOICES + i).AsTextArea().Build(); // CHOICE TEXT PORT
            if (showConditions)
            {
                GetNodeOptionByName(IN_OPTION_CHOICE_CONDITION_TYPE + i).TryGetValue(out ConditionOptions option);
                if (option != ConditionOptions.NONE)
                {
                    if (option == ConditionOptions.ALIGNMENT) // ALIGNMENT PORT
                    {
                        context.AddInputPort<int>(IN_PORT_HUMANITY_CONDITION + i).Build();
                        context.AddInputPort<int>(IN_PORT_UNDEAD_CONDITION + i).Build();
                    }
                    else if (option == ConditionOptions.CLUE) // CLUE PORT
                    {
                        context.AddInputPort<Clue>(IN_PORT_CLUE_CONDITION + i).Build();
                    }
                    else if (option == ConditionOptions.WILLING_TO_TALK) // IS WILLING TO TALK PORT
                    {
                        context.AddInputPort<DialogueSpeaker>(IN_PORT_IS_WILLING_TO_TALK_CONDITION + i).Build();
                    }
                    else if (option == ConditionOptions.CALLBACK) // CALLBACK PORT
                    {
                        context.AddInputPort<Callback>(IN_PORT_CALLBACK_CONDITION + i).Build();
                    }

                }
            }
        }
    }
    protected override void OnDefineOptions(IOptionDefinitionContext context)
    {
        context.AddOption<bool>(IN_OPTION_CONDITIONS).Build(); // SHOW CONDITION OPTION
        context.AddOption<int>(IN_OPTION_CHOICE_COUNT).Build(); // PICK NUMBER OF CHOICES

        GetNodeOptionByName(IN_OPTION_CONDITIONS).TryGetValue(out bool showConditions);
        GetNodeOptionByName(IN_OPTION_CHOICE_COUNT).TryGetValue(out int choiceCount);
        if (showConditions)
        {
            for (int i = 1; i < choiceCount + 1; i++)
            {
                context.AddOption<ConditionOptions>(IN_OPTION_CHOICE_CONDITION_TYPE + i).WithDefaultValue(ConditionOptions.NONE).Build(); // CREATE CONDITION OPTION PORT
            }
        }
    }

    public override void OnEnable()
    {
        Subtitle = "Use this to select choices for the dialogue";
        DefaultColor = Color.blue;
    }
}

[Serializable]
public class ActionNode : Node
{
    public static readonly string IN_PORT = "in";
    public static readonly string OUT_PORT = "out";
    public static readonly string IN_PORT_ACTION = "Action";

    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        context.AddInputPort(IN_PORT).Build();
        context.AddOutputPort(OUT_PORT).Build();
        context.AddInputPort<DialogueAction>(IN_PORT_ACTION).Build();
    }
}

[Serializable]
public class Randomizer : Node
{
    public static readonly string IN_PORT = "in";
    public static readonly string OUT_PORT = "out";

    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        context.AddInputPort(IN_PORT).Build();
        context.AddOutputPort(OUT_PORT).Build();
    }
}

[Serializable]
public class ClueNode : Node
{
    public static readonly string IN_PORT = "in";
    public static readonly string OUT_PORT = "out";
    public static readonly string IN_PORT_CLUE = "Clue";

    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        context.AddInputPort(IN_PORT).Build();
        context.AddOutputPort(OUT_PORT).Build();

        context.AddInputPort<Clue>(IN_PORT_CLUE).Build();
    }
}

[Serializable]
public class CallBackNode : Node
{
    public static readonly string IN_PORT = "in";
    public static readonly string OUT_PORT = "out";
    public static readonly string IN_PORT_CALLBACK = "Callback";

    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        context.AddInputPort(IN_PORT).Build();
        context.AddOutputPort(OUT_PORT).Build();
        context.AddInputPort<Callback>(IN_PORT_CALLBACK).Build();
    }
}

[Serializable]
public class TalkWillingnessNode : Node
{
    public static readonly string IN_PORT = "in";
    public static readonly string OUT_PORT = "out";
    public static readonly string IN_PORT_SPEAKER = "Speaker";
    public static readonly string IN_PORT_TALKWILLINGNESS_TOGGLE = "Set talkwillingness";

    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        context.AddInputPort(IN_PORT).Build();
        context.AddOutputPort(OUT_PORT).Build();
        context.AddInputPort<DialogueSpeaker>(IN_PORT_SPEAKER).Build();
        context.AddInputPort<TalkWillingNessEnum>(IN_PORT_TALKWILLINGNESS_TOGGLE).Build();
    }  
}
[Serializable]
public class ConditionNode : Node
{
    // PORTS AND OPTIONS
    public static readonly string IN_PORT = "in";
    public static readonly string OUT_PORT_FAIL = "condition failed out";
    public static readonly string OUT_PORT_SUCCESS = "condition met out";
    public static readonly string IN_OPTION_CONDITION_TYPE = "Choose Condition Type";

    // DESIGN YOUR CONDITION
    public static readonly string IN_OPTION_CHOICE_CONDITION_TYPE = "Condition Type";
    public static readonly string IN_PORT_HUMANITY_CONDITION = "Humanity";
    public static readonly string IN_PORT_UNDEAD_CONDITION = "Undead";
    public static readonly string IN_PORT_CLUE_CONDITION = "Clue";
    public static readonly string IN_PORT_CALLBACK_CONDITION = "Callback";
    public static readonly string IN_PORT_IS_WILLING_TO_TALK_CONDITION = "Talk Willingness Target";

    protected override void OnDefineOptions(IOptionDefinitionContext context)
    {
        context.AddOption<ConditionOptions>(IN_OPTION_CONDITION_TYPE).WithDefaultValue(ConditionOptions.NONE).Build(); // CONDITION TYPE OPTION
    }

    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        context.AddInputPort(IN_PORT).Build(); // IN PORT
        context.AddOutputPort(OUT_PORT_SUCCESS).Build(); // SUCCESS OUT PORT
        context.AddOutputPort(OUT_PORT_FAIL).Build(); // FAIL OUT PORT

        GetNodeOptionByName(IN_OPTION_CONDITION_TYPE).TryGetValue(out ConditionOptions option);
        if (option != ConditionOptions.NONE)
        {
            if (option == ConditionOptions.ALIGNMENT) // ALIGNMENT PORT
            {
                context.AddInputPort<int>(IN_PORT_HUMANITY_CONDITION).Build();
                context.AddInputPort<int>(IN_PORT_UNDEAD_CONDITION).Build();
            }
            else if (option == ConditionOptions.CLUE) // CLUE PORT
            {
                context.AddInputPort<Clue>(IN_PORT_CLUE_CONDITION).Build();
            }
            else if (option == ConditionOptions.WILLING_TO_TALK) // IS WILLING TO TALK PORT
            {
                context.AddInputPort<DialogueSpeaker>(IN_PORT_IS_WILLING_TO_TALK_CONDITION).Build();
            }
            else if (option == ConditionOptions.CALLBACK) // CALLBACK PORT
            {
                context.AddInputPort<Callback>(IN_PORT_CALLBACK_CONDITION).Build();
            }
        }
    }

    public override void OnEnable()
    {
        Subtitle = "Use this to make dialogue branch depending on a condition";
        DefaultColor = Color.pink;
    }

}


#endregion



#region Legacy Nodes
/*[Serializable]
public class ActionNode : Node
{
    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        context.AddInputPort("in").Build();
        context.AddOutputPort("out").Build();

        context.AddInputPort<UnityEvent>("Action").WithConnectorUI(PortConnectorUI.Arrowhead).WithDisplayName("Speaker").Build();
    }
}*/

/*[Serializable]
public class ItemCheckNode : Node
{
protected override void OnDefinePorts(IPortDefinitionContext context)
{
    context.AddInputPort("in").Build();

    context.AddInputPort<Item>("Item").Build();

    context.AddOutputPort("Success").Build();
    context.AddOutputPort("Failure").Build();
}
}*/



/*[Serializable]
public class ChoiceNode : Node
{
    const string optionID = "portCount";
    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        context.AddInputPort("in").Build();

        context.AddInputPort<Speaker>("Speaker").Build();
        context.AddInputPort<string>("Dialogue").Build();

        var option = GetNodeOptionByName(optionID);
        option.TryGetValue(out int portCount);
        for (int i = 0; i < portCount; i++)
        {
            context.AddInputPort<string>($"Choice Text {i}").Build();
            context.AddOutputPort($"Choice {i}").Build();

            context.AddInputPort<int>($"Humanity - Choice {i}").Build();
            
        }
    }

    protected override void OnDefineOptions(IOptionDefinitionContext context)
    {
        context.AddOption<int>(optionID).WithDefaultValue(2).Delayed();
    }
}*/

/*[Serializable]
public class InteractionNode : Node
{
    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        context.AddInputPort("in").Build();
        context.AddOutputPort("out").Build();
    }

    protected override void OnDefineOptions(IOptionDefinitionContext context)
    {
        context.AddOption<string>("Name").Build();
        context.AddOption<string>("Fluff Text").Build();
        context.AddOption<Sprite>("Image").Build();
    }
}*/
#endregion

