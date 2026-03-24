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
    public static readonly string IN_OPTION_SENTENCE_COUNT = "Sentences";
    public static readonly string IN_OPTION_SENTENCE = "Sentence";
    public static readonly string IN_OPTION_CHOICE_COUNT = "Choices";
    public static readonly string IN_OPTION_CHOICE_TEXT = "Choice text";
    public static readonly string IN_OPTION_CONDITION_TOGGLE = "Requirement";
    public static readonly string IN_PORT_CONDITION= "Condition";
    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        // In Port
        context.AddInputPort(IN_PORT).Build();

        //Speaker port
        context.AddInputPort<DialogueSpeaker>(IN_PORT_SPEAKER).Build();

        // Spawn Sentence Ports
        GetNodeOptionByName(IN_OPTION_SENTENCE_COUNT).TryGetValue(out int sentenceCount);
        for (int i = 0; i < sentenceCount; i++)
        {
            context.AddInputPort<string>(IN_OPTION_SENTENCE + i).WithConnectorUI(PortConnectorUI.Arrowhead).Build();
        }

        // Spawn Choice Ports
        GetNodeOptionByName(IN_OPTION_CHOICE_COUNT).TryGetValue(out int choiceCount);
        for (int i = 0; i < choiceCount; i++)
        {
            context.AddInputPort<string>(IN_OPTION_CHOICE_TEXT + i).Build();
        }

        //Out Ports
        if (choiceCount == 0)
        {
            context.AddOutputPort(OUT_PORT).Build();
        }
        else
        {
            for (int i = 0; i < choiceCount; i++)
            {
                GetNodeOptionByName(IN_OPTION_CONDITION_TOGGLE).TryGetValue(out bool requirement);
                if (requirement)
                {
                    context.AddOutputPort(OUT_PORT_CHOICE + i).Build();
                    context.AddInputPort<DialogueCondition>(IN_PORT_CONDITION + i).Build();
                }
                else
                {
                    context.AddOutputPort(OUT_PORT_CHOICE + i).Build();
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
        // Requirements
        context.AddOption<bool>(IN_OPTION_CONDITION_TOGGLE).WithDefaultValue(false).Build();
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

