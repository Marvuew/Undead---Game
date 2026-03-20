using UnityEngine;
using Unity.GraphToolkit.Editor;
using System;
using System.ComponentModel;
using UnityEngine.Events;
using JetBrains.Annotations;
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

[Serializable]
public class DialogueNode : Node
{
    INodeOption choices;
    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        // In Port
        context.AddInputPort("in").Build();

        //Speaker port
        context.AddInputPort<Speaker>("Speaker").Build();

        //Out Ports
        choices.TryGetValue(out int choiceCount);
        if (choiceCount == 0)
        {
            context.AddOutputPort("out").Build();
        }
        else
        {
            for (int i = 0; i < choiceCount; i++)
            {
                context.AddOutputPort($"Choice {i}").Build();
            }
        }
    }

    protected override void OnDefineOptions(IOptionDefinitionContext context)
    {
        // Number of sentences
        var sentences = context.AddOption<int>("Sentences").WithDefaultValue(1).Build();

        // Serializes a stirng for number of sentences
        sentences.TryGetValue(out int sentenceCount);
        for (int i = 0; i < sentenceCount; i++)
        {
            context.AddOption<string>("Sentence " + i).Build();
        }

        choices = context.AddOption<int>("Choices").WithDefaultValue(0).Build();
        choices.TryGetValue(out int choiceCount);
        for (int i = 0; i < choiceCount; i++)
        {
            context.AddOption<string>($"Choice {i} Text").Build();
            context.AddOption<int>($"Humanity (Choice {i})").Build();
            context.AddOption<int>($"Undead (Choice {i})").Build();
        }


    }
}

[Serializable]
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
}

[Serializable]
public class ItemCheckNode : Node
{
    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        context.AddInputPort("in").Build();

        context.AddInputPort<Item>("Item").Build();

        context.AddOutputPort("Success").Build();
        context.AddOutputPort("Failure").Build();
    }
}
