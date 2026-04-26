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
    public static readonly string IN_OPTION_CHOICE_COUNT = "Choices";
    public static readonly string IN_OPTION_CHOICE_TEXT = "Choice text";
    public static readonly string IN_PORT_TYPING_SPEED = "TypingSpeed";
    public static readonly string IN_PORT_EMOTION = "Emotion";

    public static readonly string IN_OPTION_MARK_AS_READ = "Mark as Read (MAR)";
    public static readonly string OUT_PORT_MARK_AS_READ = "If MAR - out";

    public static readonly string IN_OPTION_CALLBACK_COUNT = "Callback Count ";
    public static readonly string IN_PORT_CALLBACKS = "Callback ";
    public static readonly string IN_PORT_CALLBACK_SENTENCE = "Callback Sentence ";
    public static readonly string IN_PORT_CALLBACK_INDEX = "Callback Occurence Index ";
    public static readonly string IN_PORT_CALLBACK_REPLACE_TOGGLE = "Replace at index? ";

    //Conditions
    public static readonly string OUT_PORT_CONDITION_FAIL = "Node Condition Fail - out";
    public static readonly string OUT_PORT_CONDITION_SUCCES = "Node Condition Succes - out";
    //public static readonly string IN_OPTION_CHOICE_CONDITION_TOGGLE = "Show Conditions for choices";
    //public static readonly string IN_OPTION_NODE_CONDITION_TOGGLE = "Show Condition for node";
    public static readonly string IN_OPTION_CHOICE_CONDITION = "Choice condition ";
    public static readonly string IN_PORT_CONDITION_NODE_OPTION = "Choose node condition";
    public static readonly string IN_PORT_CONDITON_CHOICE_OPTION = "Choose choice condition";

    public static readonly string IN_PORT_ALIGNMENT_CONDITION_HUMANITY = "Requried Humanity";
    public static readonly string IN_PORT_ALIGNMENT_CONDITION_UNDEAD = "Required Undead";
    public static readonly string IN_PORT_CLUE_CONDITION_CLUE = "Required Clue";
    public static readonly string IN_PORT_ISWIILINGTOTALK_CONDITION = "Required Speaker";
    public static readonly string IN_PORT_CALLBACK_CONDITION = "Required Callback";

    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        // IN PORT
        context.AddInputPort(IN_PORT).Build();

        // SPEAKER
        context.AddInputPort<DialogueSpeaker>(IN_PORT_SPEAKER).Build();
        // SPEAKER EMOTION
        context.AddInputPort<Emotion>(IN_PORT_EMOTION).WithDefaultValue(Emotion.CONTENT).Build();
        // TYPING SPEED
        context.AddInputPort<TypingSpeed>(IN_PORT_TYPING_SPEED).WithDefaultValue(TypingSpeed.MID).Build();

        // NODE CONDITION
        GetNodeOptionByName(IN_PORT_CONDITION_NODE_OPTION).TryGetValue(out ConditionOptions conditionOption);
            if (conditionOption != ConditionOptions.NONE)
            {
                if (conditionOption == ConditionOptions.ALIGNMENT)
                {
                    context.AddInputPort<int>(IN_PORT_ALIGNMENT_CONDITION_HUMANITY).Build();
                    context.AddInputPort<int>(IN_PORT_ALIGNMENT_CONDITION_UNDEAD).Build();
                    context.AddOutputPort(OUT_PORT_CONDITION_FAIL).Build();
                    context.AddOutputPort(OUT_PORT_CONDITION_SUCCES).Build();
                }
                else if (conditionOption == ConditionOptions.CLUE)
                {
                    context.AddInputPort<Clue>(IN_PORT_CLUE_CONDITION_CLUE).Build();
                    context.AddOutputPort(OUT_PORT_CONDITION_FAIL).Build();
                    context.AddOutputPort(OUT_PORT_CONDITION_SUCCES).Build();
                }
                else if (conditionOption == ConditionOptions.WILLING_TO_TALK)
                {
                    context.AddInputPort<DialogueSpeaker>(IN_PORT_ISWIILINGTOTALK_CONDITION).Build();
                    context.AddOutputPort(OUT_PORT_CONDITION_FAIL).Build();
                    context.AddOutputPort(OUT_PORT_CONDITION_SUCCES).Build();
                }
                else if (conditionOption == ConditionOptions.CALLBACK)
                {
                    context.AddInputPort<Callback>(IN_PORT_CALLBACK_CONDITION).Build();
                    context.AddOutputPort(OUT_PORT_CONDITION_FAIL).Build();
                    context.AddOutputPort(OUT_PORT_CONDITION_SUCCES).Build();
                }
            }

            // SPAWN SENTENCES
            GetNodeOptionByName(IN_OPTION_SENTENCE_COUNT).TryGetValue(out int sentenceCount);
            for (int i = 0; i < sentenceCount; i++)
            {
                context.AddInputPort<string>(IN_OPTION_SENTENCE + i).WithConnectorUI(PortConnectorUI.Arrowhead).AsTextArea().Build();
            }

            // CALLBACKS
            GetNodeOptionByName(IN_OPTION_CALLBACK_COUNT).TryGetValue(out int callbackCount);
            for (int i = 0; i < callbackCount; i++)
            {
                context.AddInputPort<Callback>(IN_PORT_CALLBACKS + i).Build();
                context.AddInputPort<string>(IN_PORT_CALLBACK_SENTENCE + i).AsTextArea().Build();
                context.AddInputPort<int>(IN_PORT_CALLBACK_INDEX + i).Build();
                context.AddInputPort<bool>(IN_PORT_CALLBACK_REPLACE_TOGGLE + i).Build();
            }

            // SPAWN CHOICES
            GetNodeOptionByName(IN_OPTION_CHOICE_COUNT).TryGetValue(out int choiceCount);
            for (int i = 0; i < choiceCount; i++)
            {
                context.AddInputPort<string>(IN_OPTION_CHOICE_TEXT + i).AsTextArea().Build();
            }


            // OUT PORTS

            // MARK AS READ
            GetNodeOptionByName(IN_OPTION_MARK_AS_READ).TryGetValue(out bool markAsRead);
            if (markAsRead)
            {
                context.AddOutputPort(OUT_PORT_MARK_AS_READ).Build();
            }

            if (choiceCount == 0 && conditionOption == ConditionOptions.NONE)
            {         
                context.AddOutputPort(OUT_PORT).Build();
            }
            else
            {
                for (int i = 0; i < choiceCount; i++)
                {
                    context.AddOutputPort(OUT_PORT_CHOICE + i).Build();
                    // READ FROM OPTION, NOT PORT
                    GetNodeOptionByName(IN_PORT_CONDITON_CHOICE_OPTION + i).TryGetValue(out ConditionOptions option);
                    if (option != ConditionOptions.NONE)
                    {
                        if (option == ConditionOptions.ALIGNMENT)
                        {
                            context.AddInputPort<int>(IN_PORT_ALIGNMENT_CONDITION_HUMANITY + i).Build();
                            context.AddInputPort<int>(IN_PORT_ALIGNMENT_CONDITION_UNDEAD + i).Build();
                        }
                        else if (option == ConditionOptions.CLUE)
                        {
                            context.AddInputPort<Clue>(IN_PORT_CLUE_CONDITION_CLUE + i).Build();
                        }
                        else if (option == ConditionOptions.WILLING_TO_TALK)
                        {
                            context.AddInputPort<DialogueSpeaker>(IN_PORT_ISWIILINGTOTALK_CONDITION + i).Build();
                        }
                        else if (option == ConditionOptions.CALLBACK)
                        {
                            context.AddInputPort<Callback>(IN_PORT_CALLBACK_CONDITION + i).Build();
                        }
                    }
                }
            }
        }
    


    protected override void OnDefineOptions(IOptionDefinitionContext context)
    {
        // Number of sentences
        context.AddOption<int>(IN_OPTION_SENTENCE_COUNT).WithDefaultValue(1).Build();
        // Number of choices
        context.AddOption<int>(IN_OPTION_CHOICE_COUNT).WithDefaultValue(0).Build();
        // Node Condition
        context.AddOption<ConditionOptions>(IN_PORT_CONDITION_NODE_OPTION).WithDefaultValue(ConditionOptions.NONE);
        //context.AddOption<bool>(IN_OPTION_NODE_CONDITION_TOGGLE).WithDefaultValue(false).Build();

        // Choice Condition
        //context.AddOption<bool>(IN_OPTION_CHOICE_CONDITION_TOGGLE).WithDefaultValue(false).Build();

        // Mark As Read
        context.AddOption<bool>(IN_OPTION_MARK_AS_READ).WithDefaultValue(false).Build();
        //CallBacks
        context.AddOption<int>(IN_OPTION_CALLBACK_COUNT).WithDefaultValue(0).Build();

        GetNodeOptionByName(IN_OPTION_CHOICE_COUNT).TryGetValue(out int choiceCount);

        for (int i = 0; i < choiceCount; i++)
        {
            context.AddOption<ConditionOptions>(IN_PORT_CONDITON_CHOICE_OPTION + i)
                   .WithDefaultValue(ConditionOptions.NONE)
                   .Build();
        }
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
    public static readonly string OUT_PORT_FAIL = "fail out";
    public static readonly string OUT_PORT_SUCCESS = "succes out";
    public static readonly string IN_OPTION_CONDITION_TYPE = "Choose Condition Type";

    // DESIGN YOUR CONDITION
    public static readonly string IN_OPTION_CHOICE_CONDITION_TYPE = "Condition Type ";
    public static readonly string IN_PORT_HUMANITY_CONDITION = "Humanity ";
    public static readonly string IN_PORT_UNDEAD_CONDITION = "Undead ";
    public static readonly string IN_PORT_CLUE_CONDITION = "Clue ";
    public static readonly string IN_PORT_CALLBACK_CONDITION = "Callback ";
    public static readonly string IN_PORT_IS_WILLING_TO_TALK_CONDITION = "Talk Willingness Target ";

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

