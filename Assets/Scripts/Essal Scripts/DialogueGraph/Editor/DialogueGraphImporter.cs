using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Schema;
using Unity.GraphToolkit.Editor;
using UnityEditor.AssetImporters;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Events;

[ScriptedImporter(1, DialogueGraph.AssetExtension)]
public class DialogueGraphImporter : ScriptedImporter
{
   
    public override void OnImportAsset(AssetImportContext ctx)
    {
        DialogueGraph editorGraph = GraphDatabase.LoadGraphForImporter<DialogueGraph>(ctx.assetPath);
        RuntimeDialogueGraph runtimeGraph = ScriptableObject.CreateInstance<RuntimeDialogueGraph>();
        var nodeIDMap = new Dictionary<INode, string>();

        foreach (var node in editorGraph.GetNodes())
        {
            nodeIDMap[node] = Guid.NewGuid().ToString();
        }

        var startNode = editorGraph .GetNodes().OfType<StartNode>().FirstOrDefault();
        if (startNode != null)
        {
            var entryPort = startNode.GetOutputPorts().FirstOrDefault()?.firstConnectedPort;
            if(entryPort != null)
            {
                runtimeGraph.EntryNodeID = nodeIDMap[entryPort.GetNode()];
            }
        }

        foreach (var iNode in editorGraph.GetNodes())
        {
            if (iNode is StartNode || iNode is EndNode) continue;
            RuntimeNode runtimeNode = null;
            //var runtimeNode = new RunTimeDialogueNode { NodeID = nodeIDMap[iNode] };
            if (iNode is DialogueNode dialogueNode)
            {
                var node = new RuntimeDialogueNode { NodeID = nodeIDMap[iNode] };
                ProcessDialogueNode(dialogueNode, node, nodeIDMap);

                runtimeNode = node;
            }
            /*else if (iNode is ChoiceNode choiceNode)
            {
                var node = new RuntimeChoiceNode { NodeID = nodeIDMap[iNode]};
                ProcessChoiceNode(choiceNode, node, nodeIDMap);

                runtimeNode = node;

            }
            else if (iNode is ItemCheckNode actionNode)
            {
                var node = new RuntimeItemCheckNode { NodeID = nodeIDMap[iNode] };
                ProcessItemCheckNode(actionNode, node, nodeIDMap);

                runtimeNode = node;
            }*/

            runtimeGraph.AllNodes.Add(runtimeNode);
        }

        ctx.AddObjectToAsset("RuntimeData", runtimeGraph);
        ctx.SetMainObject(runtimeGraph);
    }

    /*private void ProcessDialogueNode(DialogueNode node, RuntimeDialogueNode runtimeNode, Dictionary<INode, string> nodeIDMap)
    {
        runtimeNode.speaker = GetPortValue<Speaker>(node.GetInputPortByName("Speaker"));

        for (int i = 0; i < runtimeNode.Dialogue.sentences.Length; i++)
        {
            runtimeNode.Dialogue.sentences[i] = GetPortValue<string>(node.GetInputPortByName("Dialogue"));
        }

        runtimeNode.HumanityChange = GetPortValue<int>(node.GetInputPortByName("Humanity"));

        var nextNodePort = node.GetOutputPortByName("out")?.firstConnectedPort;
        if (nextNodePort != null)
        {
            runtimeNode.NextNodeID = nodeIDMap[nextNodePort.GetNode()];
        }
    }*/

    private void ProcessDialogueNode(DialogueNode node, RuntimeDialogueNode runtimeNode, Dictionary<INode, string> nodeIDMap)
    {
        //-----------------------------PROCESS SPEAKER--------------------------------//
       runtimeNode.speaker = GetPortValue<Speaker>(node.GetInputPortByName("Speaker"));

        //----------------------------PROCESS SENTENCES------------------------------//
        node.GetNodeOptionByName("Sentences").TryGetValue(out int sentenceCount);
        for (int i = 0; i < sentenceCount; i++)
        {
            string sentenceName = "Sentence " + i;
            node.GetNodeOptionByName(sentenceName).TryGetValue(out string sentence);
            runtimeNode.Dialogue.Add(sentence);
        }

        node.GetNodeOptionByName("Choices").TryGetValue(out int choiceCount);
        for (int i = 0; i < choiceCount; i++)
        {
            string choiceName = $"Choice {i} Text";
            string undeadName = $"Undead (Choice {i})";
            string humanityName = $"Undead (Choice {i})";

            string choiceOutputName = $"Choice {i}";

            node.GetNodeOptionByName(choiceName).TryGetValue(out string choicetext);
            node.GetNodeOptionByName(undeadName).TryGetValue(out int undead);
            node.GetNodeOptionByName(humanityName).TryGetValue(out int humanity);

            var choiceOutputPort = node.GetOutputPortByName(choiceOutputName);
            var choiceData = new ChoiceData { ChoiceText = choicetext, HumanityChange = humanity, UndeadChange = undead, DestinationNodeID = choiceOutputPort.firstConnectedPort != null ? nodeIDMap[choiceOutputPort.firstConnectedPort.GetNode()] : null };
            runtimeNode.Choices.Add(choiceData);  
        }

        //---------------------------FIND NEXT NODEID-----------------------------//
        if (choiceCount == 0)
        {
            var nextNodePort = node.GetOutputPortByName("out")?.firstConnectedPort;
            if (nextNodePort != null)
            {
                runtimeNode.NextNodeID = nodeIDMap[nextNodePort.GetNode()];
            }
        }
    }

    /*private void ProcessChoiceNode(ChoiceNode node, RuntimeChoiceNode runtimeNode, Dictionary<INode, string> nodeIDMap)
    {
        runtimeNode.speaker = GetPortValue<Speaker>(node.GetInputPortByName("Speaker"));      

        var choiceOutputPorts = node.GetOutputPorts().Where(p => p.name.StartsWith("Choice "));

        foreach (var outputPort in choiceOutputPorts)
        {
            var index = outputPort.name.Substring("Choice ".Length);
            var textPort = node.GetInputPortByName($"Choice Text {index}");
            var humanityPort = node.GetInputPortByName($"Humanity - Choice {index}");

            var choiceData = new ChoiceData
            {
                ChoiceText = GetPortValue<string>(textPort),
                DestinationNodeID = outputPort.firstConnectedPort != null ? nodeIDMap[outputPort.firstConnectedPort.GetNode()] : null,
                HumanityChange = GetPortValue<int>(humanityPort),
                
            };

            runtimeNode.Choices.Add(choiceData);
        }    
    }*/

    /*private void ProcessItemCheckNode(ItemCheckNode node, RuntimeItemCheckNode runtimeNode, Dictionary<INode, string> nodeIDMap)
    {
        runtimeNode.RequiredItem = GetPortValue<Item>(node.GetInputPortByName("Item"));

        var successNodePort = node.GetOutputPortByName("Success")?.firstConnectedPort;
        if (successNodePort != null) 
        {
            runtimeNode.SuccessNodeID = nodeIDMap[successNodePort.GetNode()];
        }

        var failureNodePort = node.GetOutputPortByName("Failure")?.firstConnectedPort;
        if (failureNodePort != null)
        {
            runtimeNode.FailureNodeID = nodeIDMap[failureNodePort.GetNode()];
        }

    }*/

    private T GetPortValue<T>(IPort port)
    {
        if (port == null) return default;

        if (port.isConnected)
        {
            if (port.firstConnectedPort.GetNode() is IVariableNode variableNode)
            {
                variableNode.variable.TryGetDefaultValue(out T value);
                return value;
            }
        }

        port.TryGetValue(out T fallbackValue);
        return fallbackValue;

    }
}
