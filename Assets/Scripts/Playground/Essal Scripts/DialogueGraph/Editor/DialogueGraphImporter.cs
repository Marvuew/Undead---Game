using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Unity.GraphToolkit.Editor;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEditor.Networking.PlayerConnection;
using UnityEngine;

[ScriptedImporter(1, DialogueGraph.AssetExtension)]
public class DialogueGraphImporter : ScriptedImporter
{
    #region Importing Nodes
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
            // Handles Start and EndNode
            if (iNode is StartNode || iNode is EndNode) continue;
            RuntimeNode runtimeNode = null;
   
            // Handles DialogueNodeA
            if (iNode is DialogueNode dialogueNode)
            {
                var node = new RuntimeDialogueNode { NodeID = nodeIDMap[iNode] };

                ProcessDialogueNode(dialogueNode, node, nodeIDMap);

                if (node.Choices.Count > 0)
                {
                    foreach (var choice in node.Choices)
                    {
                        choice.ChoiceID = Guid.NewGuid().ToString();
                    }
                }

                runtimeNode = node;
            }
            // Handles AlignmentNode
            if (iNode is AlignmentNode alignmentNode)
            {
                var node = new RuntimeAlignmentNode { NodeID = nodeIDMap[iNode] };

                ProcessAlignmentNode(alignmentNode, node, nodeIDMap);

                runtimeNode = node;
            }

            if (iNode is ActionNode actionNode)
            {
                var node = new RuntimeActionNode { NodeID = nodeIDMap[iNode] };

                ProcessActionNode(actionNode, node, nodeIDMap);

                runtimeNode = node;
            }

            if (iNode is Randomizer randomizer)
            {
                var node = new RuntimeRandomizer { NodeID = nodeIDMap[iNode] };

                ProcessRandomizer(randomizer, node, nodeIDMap);

                runtimeNode = node;
            }

            if (iNode is ClueNode clueNode)
            {
                var node = new RuntimeClueNode { NodeID = nodeIDMap[iNode] };

                ProcessClueNode(clueNode, node, nodeIDMap);

                runtimeNode = node;
            }

            if (iNode is CallBackNode callBackNode)
            {
                var node = new RuntimeCallBackNode { NodeID = nodeIDMap[iNode] };

                ProcessCallbackNode(callBackNode, node, nodeIDMap);

                runtimeNode = node;
            }

            runtimeGraph.AllNodes.Add(runtimeNode);
        }

        ctx.AddObjectToAsset("RuntimeData", runtimeGraph);
        ctx.SetMainObject(runtimeGraph);
    }
    #endregion

    #region Node Processing Methods
    private void ProcessDialogueNode(DialogueNode node, RuntimeDialogueNode runtimeNode, Dictionary<INode, string> nodeIDMap)
    {
        // Get the condition toggle and condition node if it exists
        node.GetNodeOptionByName(DialogueNode.IN_OPTION_CONDITION_TOGGLE).TryGetValue(out bool conditionToggle);
        // If the condition toggle is on and the nodeCondition field is not empty. Set the ConditionFailNodeID
        if (conditionToggle)
        {
            DialogueCondition condition = GetPortValue<DialogueCondition>(node.GetInputPortByName(DialogueNode.IN_PORT_CONDITION_NODE));
            var nodeCondition = node.GetInputPortByName(DialogueNode.IN_PORT_CONDITION_NODE);

            if (nodeCondition != null)
            {
                runtimeNode.NodeCondition = condition;
            }
            else
            {
                runtimeNode.NodeCondition = null;
            }
            
            runtimeNode.NodeCondition = condition;
            var conditionNodePort = node.GetOutputPortByName(DialogueNode.OUT_PORT_CONDITION_FAIL)?.firstConnectedPort;
            if (conditionNodePort != null)
            {
                runtimeNode.ConditionFailNodeID = nodeIDMap[conditionNodePort.GetNode()];
            }
        }

        // Handle Mark As Read Option
        node.GetNodeOptionByName(DialogueNode.IN_OPTION_MARK_AS_READ).TryGetValue(out bool markAsRead);
        if (markAsRead)
        {
            runtimeNode.MarkAsRead = markAsRead;
            var markAsReadPort = node.GetOutputPortByName(DialogueNode.OUT_PORT_MARK_AS_READ)?.firstConnectedPort;
            if (markAsReadPort != null)
            {
                runtimeNode.MarkAsReadNodeID = nodeIDMap[markAsReadPort.GetNode()];
            }
        }

        var port = node.GetInputPortByName(DialogueNode.IN_PORT_SPEAKER);

        if (port != null)
        {
            runtimeNode.Speaker = GetPortValue<DialogueSpeaker>(port);
        }
        else
        {
            runtimeNode.Speaker = null;
        }

        // Handle Typing Speed
        runtimeNode.TypingSpeed = GetPortValue<TypingSpeed>(node.GetInputPortByName(DialogueNode.IN_PORT_TYPING_SPEED));

        // Handle Emotion
        runtimeNode.Emotion = GetPortValue<Emotion>(node.GetInputPortByName(DialogueNode.IN_PORT_EMOTION));

        // Handle Sentence Count
        node.GetNodeOptionByName(DialogueNode.IN_OPTION_SENTENCE_COUNT).TryGetValue(out int sentenceCount);
        for (int i = 0; i < sentenceCount; i++)
        {
            string sentence = GetPortValue<string>(node.GetInputPortByName(DialogueNode.IN_OPTION_SENTENCE + i));
            runtimeNode.Dialogue.Add(sentence);
        }

        // Handle Choice Count
        node.GetNodeOptionByName(DialogueNode.IN_OPTION_CHOICE_COUNT).TryGetValue(out int choiceCount);
        for (int i = 0; i < choiceCount; i++)
        {
            //Requirements
            if (conditionToggle)
            {
                DialogueCondition condition = GetPortValue<DialogueCondition>(node.GetInputPortByName(DialogueNode.IN_PORT_CONDITION_CHOICE + i));
                string choiceText = GetPortValue<string>(node.GetInputPortByName(DialogueNode.IN_OPTION_CHOICE_TEXT + i));
                var choiceOutputPort = node.GetOutputPortByName(DialogueNode.OUT_PORT_CHOICE + i);
                var choiceData = new ChoiceData { ChoiceText = choiceText, DestinationNodeID = choiceOutputPort.firstConnectedPort != null ? nodeIDMap[choiceOutputPort.firstConnectedPort.GetNode()] : null, Condition = condition};
                runtimeNode.Choices.Add(choiceData);
            }
            else
            {
                string choiceText = GetPortValue<string>(node.GetInputPortByName(DialogueNode.IN_OPTION_CHOICE_TEXT + i));
                var choiceOutputPort = node.GetOutputPortByName(DialogueNode.OUT_PORT_CHOICE + i);
                var choiceData = new ChoiceData { ChoiceText = choiceText, DestinationNodeID = choiceOutputPort.firstConnectedPort != null ? nodeIDMap[choiceOutputPort.firstConnectedPort.GetNode()] : null};
                runtimeNode.Choices.Add(choiceData);
            }

        }

        // Handle Callbacks
        node.GetNodeOptionByName(DialogueNode.IN_OPTION_CALLBACK_COUNT).TryGetValue(out int callbackCount);
        for (int i = 0; i < callbackCount; i++)
        {
            // 1. Get the data from ports
            string callbackSentence = GetPortValue<string>(node.GetInputPortByName(DialogueNode.IN_PORT_CALLBACK_SENTENCE + i));
            int index = GetPortValue<int>(node.GetInputPortByName(DialogueNode.IN_PORT_CALLBACK_INDEX + i));
            bool replace = GetPortValue<bool>(node.GetInputPortByName(DialogueNode.IN_PORT_CALLBACK_REPLACE_TOGGLE + i));
            Callback callback = GetPortValue<Callback>(node.GetInputPortByName(DialogueNode.IN_PORT_CALLBACKS + i));

            // 2. CRITICAL: Check if the callback object exists
            if (callback != null)
            {
                var CallbackData = new CallbackData
                {
                    CallbackAsset = callback,
                    Sentence = callbackSentence,
                    Index = index,
                    Replace = replace

                };

                runtimeNode.Callbacks.Add(CallbackData);
                
            }
            else
            {
                Debug.LogWarning($"Callback at index {i} in node {node} is null. Skipping.");
            }
        }


        // Handles finding the nextnodeID
        if (choiceCount == 0)
        {
            var nextNodePort = node.GetOutputPortByName(DialogueNode.OUT_PORT)?.firstConnectedPort;
            if (nextNodePort != null)
            {
                runtimeNode.NextNodeID = nodeIDMap[nextNodePort.GetNode()];
            }
        }
    }

    private void ProcessAlignmentNode(AlignmentNode node, RuntimeAlignmentNode runtimeNode, Dictionary<INode, string> nodeIDMap)
    {
        runtimeNode.HumanityChange = GetPortValue<int>(node.GetInputPortByName(AlignmentNode.IN_OPTION_HUMANITY));
        runtimeNode.UndeadChange = GetPortValue<int>(node.GetInputPortByName(AlignmentNode.IN_OPTION_UNDEAD));

        var nextNodePort = node.GetOutputPortByName(AlignmentNode.OUT_PORT)?.firstConnectedPort;
        if (nextNodePort != null)
        {
            runtimeNode.NextNodeID = nodeIDMap[nextNodePort.GetNode()];
        }
    }

    private void ProcessActionNode(ActionNode node, RuntimeActionNode runtimeNode, Dictionary<INode, string> nodeIDMap)
    {
        runtimeNode.Action = GetPortValue<DialogueAction>(node.GetInputPortByName(ActionNode.IN_PORT_ACTION));

        var nextNodePort = node.GetOutputPortByName(ActionNode.OUT_PORT)?.firstConnectedPort;
        if (nextNodePort != null)
        {
            runtimeNode.NextNodeID = nodeIDMap[nextNodePort.GetNode()];
        }
    }

    private void ProcessRandomizer(Randomizer node, RuntimeRandomizer runtimeNode, Dictionary<INode, string> nodeIDMap)
    {
        var outPutPort = node.GetOutputPortByName(Randomizer.OUT_PORT);

        List<IPort> ports = new List<IPort>();

        outPutPort.GetConnectedPorts(ports);

        if (ports.Count == 0)
        {
            Debug.LogWarning("There is no connected ports to the randomizer");
            return;
        }

        foreach (var port in ports)
        {
            runtimeNode.randomNextNodeID.Add(nodeIDMap[port.GetNode()]);
        }
    }

    private void ProcessClueNode(ClueNode node, RuntimeClueNode runtimeNode, Dictionary<INode, string> nodeIDMap)
    {
        runtimeNode.clue = GetPortValue<Clue>(node.GetInputPortByName(ClueNode.IN_PORT_CLUE));

        var nextNodePort = node.GetOutputPortByName(ActionNode.OUT_PORT)?.firstConnectedPort;
        if (nextNodePort != null)
        {
            runtimeNode.NextNodeID = nodeIDMap[nextNodePort.GetNode()];
        }
    }

    private void ProcessCallbackNode(CallBackNode node, RuntimeCallBackNode runtimeNode, Dictionary<INode, string> nodeIDMap)
    {
        runtimeNode.callback = GetPortValue<Callback>(node.GetInputPortByName(CallBackNode.IN_PORT_CALLBACK));

        var nextNodePort = node.GetOutputPortByName(ActionNode.OUT_PORT)?.firstConnectedPort;
        if (nextNodePort != null)
        {
            runtimeNode.NextNodeID = nodeIDMap[nextNodePort.GetNode()];
        }
    }

    #endregion

    #region Helper Functions
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
    #endregion

#region Legacy Importer Code
/*private void ProcessActionNode(ActionNode node, RuntimeActionNode runtimeNode, Dictionary<INode, string> nodeIDMap)
{
    node.GetNodeOptionByName("Event To Trigger").TryGetValue(out GameEvent Action);

    runtimeNode.Action = Action;

    var nextNodePort = node.GetOutputPortByName("out")?.firstConnectedPort;
    if (nextNodePort != null)
    {
        runtimeNode.NextNodeID = nodeIDMap[nextNodePort.GetNode()];
    }
}*/


/*if (iNode is ActionNode actionNode)
{
    var node = new RuntimeActionNode { NodeID = nodeIDMap[iNode] };

    ProcessActionNode(actionNode, node, nodeIDMap);

    runtimeNode = node;
}*/

/*// Handles InteractionNode
if (iNode is InteractionNode interactionNode)
{
    var node = new InteractionNode 
}*/

//var runtimeNode = new RunTimeDialogueNode { NodeID = nodeIDMap[iNode] };

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

#endregion
