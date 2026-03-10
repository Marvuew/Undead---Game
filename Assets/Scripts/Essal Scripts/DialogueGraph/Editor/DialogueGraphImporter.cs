using UnityEngine;
using UnityEditor.AssetImporters;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.GraphToolkit.Editor;
using System.Xml.Schema;
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

            var runtimeNode = new RunTimeDialogueNode { NodeID = nodeIDMap[iNode] };
            if (iNode is DialogueNode dialogueNode)
            {
                ProcessDialogueNode(dialogueNode, runtimeNode, nodeIDMap);
            }
            else if (iNode is ChoiceNode choiceNode)
            {
                ProcessChoiceNode(choiceNode, runtimeNode, nodeIDMap);
            }
            else if (iNode is ActionNode actionNode)
            {
                ProcessActionNode(actionNode, runtimeNode, nodeIDMap);
            }

            runtimeGraph.AllNodes.Add(runtimeNode);
        }

        ctx.AddObjectToAsset("RuntimeData", runtimeGraph);
        ctx.SetMainObject(runtimeGraph);
    }

    private void ProcessDialogueNode(DialogueNode node, RunTimeDialogueNode runtimeNode, Dictionary<INode, string> nodeIDMap)
    {
        runtimeNode.speaker = GetPortValue<Speaker>(node.GetInputPortByName("Speaker"));
        runtimeNode.DialogueText = GetPortValue<string>(node.GetInputPortByName("Dialogue"));
        runtimeNode.HumanityChange = GetPortValue<int>(node.GetInputPortByName("Humanity"));

        var nextNodePort = node.GetOutputPortByName("out")?.firstConnectedPort;
        if (nextNodePort != null)
        {
            runtimeNode.NextNodeID = nodeIDMap[nextNodePort.GetNode()];
        }
    }

    private void ProcessChoiceNode(ChoiceNode node, RunTimeDialogueNode runtimeNode, Dictionary<INode, string> nodeIDMap)
    {
        runtimeNode.speaker = GetPortValue<Speaker>(node.GetInputPortByName("Speaker"));
        runtimeNode.DialogueText = GetPortValue<string>(node.GetInputPortByName("Dialogue"));
        

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
    }

    private void ProcessActionNode(ActionNode node, RunTimeDialogueNode runtimeNode, Dictionary<INode, string> nodeIDMap)
    {
        runtimeNode.CorrectItem = GetPortValue<Item>(node.GetInputPortByName("Item"));
        runtimeNode.SuccesText = GetPortValue<string>(node.GetInputPortByName("Succes"));
        runtimeNode.FailureText = GetPortValue<string>(node.GetInputPortByName("Failure"));

        var nextNodePort = node.GetOutputPortByName("out")?.firstConnectedPort;
        if (nextNodePort != null)
        {
            runtimeNode.NextNodeID = nodeIDMap[nextNodePort.GetNode()];
        }

    }

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
