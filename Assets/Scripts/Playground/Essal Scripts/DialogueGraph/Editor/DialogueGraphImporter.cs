using PlasticGui.WorkspaceWindow.Merge;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Net;
using Unity.GraphToolkit.Editor;
using Unity.VisualScripting;
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
        RuntimeDialogueGraph runtimeGraph = ScriptableObject.CreateInstance<RuntimeDialogueGraph>(); // MAKE AN INSTANCE OF RUNTIME DIALOGUE GRAPH
        var nodeIDMap = new Dictionary<INode, string>(); // CREATE NODE DICTIONARY

        foreach (var node in editorGraph.GetNodes())
        {
            nodeIDMap[node] = Guid.NewGuid().ToString(); // CREATE A UNIQUE ID FOR ALL NODES AND ADD THEM TO THE DICTIONARY
        }

        var startNode = editorGraph .GetNodes().OfType<StartNode>().FirstOrDefault(); // FIND THE FIRST NODE
        if (startNode != null)
        {
            var entryPort = startNode.GetOutputPorts().FirstOrDefault()?.FirstConnectedPort;
            if(entryPort != null)
            {
                runtimeGraph.EntryNodeID = nodeIDMap[entryPort.GetNode()]; // FIND THE ENTRY NODE AND ADD THE ENTRY NODE ID
            }
        }

        foreach (var iNode in editorGraph.GetNodes())
        {
            if (iNode is StartNode || iNode is EndNode) continue; // SKIP STARTNODE AND ENDNODE
            RuntimeNode runtimeNode = null; // MAKE AN INSTANCE OF RUNTIME NODE
   

            if (iNode is DialogueNode dialogueNode) // HANDLE DIALOGUE NODE
            {
                var node = new RuntimeDialogueNode { NodeID = nodeIDMap[iNode] }; // MAKE AN INSTANCE OF THE RUNTIME : ADD THE UNIQUE GUID TO THE NODEID

                ProcessDialogueNode(dialogueNode, node, nodeIDMap); // CALL THE PROCESS FUNCTION OF THE NODE

                runtimeNode = node; // SET THE RUNTIMENODE TO THE RUNTIME INSTANCE
            }
            
            // CONTINUE FOR THE REST OF NODES

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

            if (iNode is TalkWillingnessNode talkWillingnessNode)
            {
                var node = new RuntimeTalkWillingnessNode { NodeID = nodeIDMap[iNode] };

                ProcessTalkWillingnessNode(talkWillingnessNode, node, nodeIDMap);

                runtimeNode = node;
            }

            if (iNode is ConditionNode conditionNode)
            {
                var node = new RuntimeConditionNode { NodeID = nodeIDMap[iNode] };

                ProcessConditionNode(conditionNode, node, nodeIDMap);

                runtimeNode = node;
            }

            if (iNode is ChoiceNode choiceNode)
            {
                var node = new RuntimeChoiceNode { NodeID = nodeIDMap[iNode] };
                
                ProcessChoiceNode(choiceNode, node, nodeIDMap);

                runtimeNode = node;
            }

            if (iNode is SoundNode soundNode)
            {
                var node = new RuntimeSoundNode { NodeID = nodeIDMap[iNode] };

                ProcessSoundNode(soundNode, node, nodeIDMap);

                runtimeNode = node;
            }

            if (iNode is FadeNode fadeNode)
            {
                var node = new RuntimeFadeNode { NodeID = nodeIDMap[iNode] };

                ProcessFadeNode(fadeNode, node, nodeIDMap);

                runtimeNode = node;
            }

            runtimeGraph.AllNodes.Add(runtimeNode); // THEN ADD IT TO THE LIST OF ALLNODES
        }

        ctx.AddObjectToAsset("RuntimeData", runtimeGraph);
        ctx.SetMainObject(runtimeGraph);
    }
    #endregion

    #region Node Processing Methods
    private void ProcessDialogueNode(DialogueNode node, RuntimeDialogueNode runtimeNode, Dictionary<INode, string> nodeIDMap)
    {
        // Mark as Read
        if (node.GetNodeOptionByName(DialogueNode.IN_OPTION_MARK_AS_READ).TryGetValue(out bool markAsRead) && markAsRead)
        {
            runtimeNode.MarkAsRead = true;
            var markAsReadPort = node.GetOutputPortByName(DialogueNode.OUT_PORT_MARK_AS_READ)?.FirstConnectedPort;
            if (markAsReadPort != null)
                runtimeNode.MarkAsReadNodeID = nodeIDMap[markAsReadPort.GetNode()];
        }

        // Speaker (Safe Check)
        var speakerPort = node.GetInputPortByName(DialogueNode.IN_PORT_SPEAKER);
        runtimeNode.Speaker = speakerPort != null ? GetPortValue<DialogueSpeaker>(speakerPort) : null;

        // Typing Speed & Emotion
        runtimeNode.TypingSpeed = GetPortValue<TypingSpeed>(node.GetInputPortByName(DialogueNode.IN_PORT_TYPING_SPEED));
        runtimeNode.Emotion = GetPortValue<Emotion>(node.GetInputPortByName(DialogueNode.IN_PORT_EMOTION));

        // Sentences
        if (node.GetNodeOptionByName(DialogueNode.IN_OPTION_SENTENCE_COUNT).TryGetValue(out int sentenceCount))
        {
            for (int i = 0; i < sentenceCount; i++)
            {
                var sPort = node.GetInputPortByName(DialogueNode.IN_OPTION_SENTENCE + i);
                if (sPort != null)
                    runtimeNode.Dialogue.Add(GetPortValue<string>(sPort));
            }
        }

        // Next Node
        var nextNodePort = node.GetOutputPortByName(DialogueNode.OUT_PORT)?.FirstConnectedPort;
        if (nextNodePort != null)
            runtimeNode.NextNodeID = nodeIDMap[nextNodePort.GetNode()];
    }

    private void ProcessAlignmentNode(AlignmentNode node, RuntimeAlignmentNode runtimeNode, Dictionary<INode, string> nodeIDMap)
    {
        runtimeNode.HumanityChange = GetPortValue<int>(node.GetInputPortByName(AlignmentNode.IN_OPTION_HUMANITY));
        runtimeNode.UndeadChange = GetPortValue<int>(node.GetInputPortByName(AlignmentNode.IN_OPTION_UNDEAD));

        var nextNodePort = node.GetOutputPortByName(AlignmentNode.OUT_PORT)?.FirstConnectedPort;
        if (nextNodePort != null)
        {
            runtimeNode.NextNodeID = nodeIDMap[nextNodePort.GetNode()];
        }
    }

    private void ProcessActionNode(ActionNode node, RuntimeActionNode runtimeNode, Dictionary<INode, string> nodeIDMap)
    {
        runtimeNode.Action = GetPortValue<DialogueAction>(node.GetInputPortByName(ActionNode.IN_PORT_ACTION));

        var nextNodePort = node.GetOutputPortByName(ActionNode.OUT_PORT)?.FirstConnectedPort;
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
        runtimeNode.description = GetPortValue<string>(node.GetInputPortByName(ClueNode.IN_PORT_DESCRIPTION));
        node.GetNodeOptionByName(ClueNode.IN_OPTION_UNDEADTYPE_COUNT).TryGetValue(out int undeadCount);
        if (undeadCount > 0)
        {
            for (int i = 1; i < undeadCount + 1; i++)
            {
                UndeadType type = GetPortValue<UndeadType>(node.GetInputPortByName(ClueNode.IN_PORT_UNDEADTYPE + i));
                runtimeNode.typePointers.Add(type);
            }
        }

        var nextNodePort = node.GetOutputPortByName(ActionNode.OUT_PORT)?.FirstConnectedPort;
        if (nextNodePort != null)
        {
            runtimeNode.NextNodeID = nodeIDMap[nextNodePort.GetNode()];
        }
    }

    private void ProcessCallbackNode(CallBackNode node, RuntimeCallBackNode runtimeNode, Dictionary<INode, string> nodeIDMap)
    {
        runtimeNode.callback = GetPortValue<Callback>(node.GetInputPortByName(CallBackNode.IN_PORT_CALLBACK));

        var nextNodePort = node.GetOutputPortByName(ActionNode.OUT_PORT)?.FirstConnectedPort;
        if (nextNodePort != null)
        {
            runtimeNode.NextNodeID = nodeIDMap[nextNodePort.GetNode()];
        }
    }

    private void ProcessTalkWillingnessNode(TalkWillingnessNode node, RuntimeTalkWillingnessNode runtimeNode, Dictionary<INode, string> nodeIDMap)
    {
        runtimeNode.Speaker = GetPortValue<DialogueSpeaker>(node.GetInputPortByName(TalkWillingnessNode.IN_PORT_SPEAKER));
        runtimeNode.IsWillingToTalk = GetPortValue<TalkWillingNessEnum>(node.GetInputPortByName(TalkWillingnessNode.IN_PORT_TALKWILLINGNESS_TOGGLE));

        var nextNodePort = node.GetOutputPortByName(ActionNode.OUT_PORT)?.FirstConnectedPort;
        if (nextNodePort != null)
        {
            runtimeNode.NextNodeID = nodeIDMap[nextNodePort.GetNode()];
        }
    }

    private void ProcessChoiceNode(ChoiceNode node, RuntimeChoiceNode runtimeNode, Dictionary<INode, string> nodeIDMap)
    {
        if (!node.GetNodeOptionByName(ChoiceNode.IN_OPTION_CHOICE_COUNT).TryGetValue(out int choiceCount)) return;

        // Check once if conditions are even enabled for this node
        node.GetNodeOptionByName(ChoiceNode.IN_OPTION_CONDITIONS).TryGetValue(out bool showConditions);

        for (int i = 1; i <= choiceCount; i++)
        {
            // Use the helper to get the port safely
            var textPort = node.GetInputPortByName(ChoiceNode.IN_PORT_CHOICES + i);
            string choiceText = textPort != null ? GetPortValue<string>(textPort) : "";

            var choiceOutputPort = node.GetOutputPortByName(ChoiceNode.OUT_PORT + i);
            string destinationID = (choiceOutputPort?.FirstConnectedPort != null)
                ? nodeIDMap[choiceOutputPort.FirstConnectedPort.GetNode()]
                : null;

            var choiceData = new ChoiceData
            {
                ChoiceText = choiceText,
                DestinationNodeID = destinationID,
                ChoiceID = $"{runtimeNode.NodeID}_choice_{i}"
            };

            // BUG FIX: Only try to get the condition option if showConditions is true
            if (showConditions)
            {
                var optionProperty = node.GetNodeOptionByName(ChoiceNode.IN_OPTION_CHOICE_CONDITION_TYPE + i);
                if (optionProperty != null && optionProperty.TryGetValue(out ConditionOptions option) && option != ConditionOptions.NONE)
                {
                    choiceData.condition = option;

                    // Safely grab the port values based on the option
                    switch (option)
                    {
                        case ConditionOptions.ALIGNMENT:
                            choiceData.choiceHumanityCondtion = GetPortValue<int>(node.GetInputPortByName(ChoiceNode.IN_PORT_HUMANITY_CONDITION + i));
                            choiceData.choiceUndeadCondtion = GetPortValue<int>(node.GetInputPortByName(ChoiceNode.IN_PORT_UNDEAD_CONDITION + i));
                            break;
                        case ConditionOptions.CLUE:
                            choiceData.choiceConditionClue = GetPortValue<Clue>(node.GetInputPortByName(ChoiceNode.IN_PORT_CLUE_CONDITION + i));
                            break;
                        case ConditionOptions.WILLING_TO_TALK:
                            choiceData.choiceConditionSpeaker = GetPortValue<DialogueSpeaker>(node.GetInputPortByName(ChoiceNode.IN_PORT_IS_WILLING_TO_TALK_CONDITION + i));
                            break;
                        case ConditionOptions.CALLBACK:
                            choiceData.choiceConditionCallback = GetPortValue<Callback>(node.GetInputPortByName(ChoiceNode.IN_PORT_CALLBACK_CONDITION + i));
                            break;
                    }
                }
            }
            runtimeNode.choices.Add(choiceData);
        }
    }

    private void ProcessConditionNode(ConditionNode node, RuntimeConditionNode runtimeNode, Dictionary<INode, string> nodeIDMap)
    {
        // 1. SAFELY get the condition type option
        // Ensure you use the exact string defined in your ConditionNode class!
        var conditionOption = node.GetNodeOptionByName(ConditionNode.IN_OPTION_CONDITION_TYPE); // Adjust string if needed

        if (conditionOption != null && conditionOption.TryGetValue(out ConditionOptions option))
        {
            runtimeNode.condition = option;

            // 2. SAFELY grab values only if the ports exist
            switch (option)
            {
                case ConditionOptions.ALIGNMENT:
                    runtimeNode.humanity = GetPortValueSafe<int>(node, ConditionNode.IN_PORT_HUMANITY_CONDITION);
                    runtimeNode.undead = GetPortValueSafe<int>(node, ConditionNode.IN_PORT_UNDEAD_CONDITION);
                    break;
                case ConditionOptions.CLUE:
                    runtimeNode.clue = GetPortValueSafe<Clue>(node, ConditionNode.IN_PORT_CLUE_CONDITION);
                    break;
                case ConditionOptions.WILLING_TO_TALK:
                    runtimeNode.TalkWillingnessTarget = GetPortValueSafe<DialogueSpeaker>(node, ConditionNode.IN_PORT_IS_WILLING_TO_TALK_CONDITION);
                    break;
                case ConditionOptions.CALLBACK:
                    // Note: Ensure your RuntimeConditionNode has a 'callback' field of type Callback
                    runtimeNode.callback = GetPortValueSafe<Callback>(node, ConditionNode.IN_PORT_CALLBACK_CONDITION);
                    break;
            }
        }

        // 3. Handle Output Connections
        var successPort = node.GetOutputPortByName(ConditionNode.OUT_PORT_SUCCESS)?.FirstConnectedPort;
        var failPort = node.GetOutputPortByName(ConditionNode.OUT_PORT_FAIL)?.FirstConnectedPort;

        if (successPort != null && nodeIDMap.ContainsKey(successPort.GetNode()))
            runtimeNode.SuccessNodeID = nodeIDMap[successPort.GetNode()];

        if (failPort != null && nodeIDMap.ContainsKey(failPort.GetNode()))
            runtimeNode.FailNodeID = nodeIDMap[failPort.GetNode()];
    }

    private void ProcessSoundNode(SoundNode node, RuntimeSoundNode runtimeNode, Dictionary<INode, string> nodeIDMap)
    {
        runtimeNode.clip = GetPortValue<AudioClip>(node.GetInputPortByName(SoundNode.IN_PORT_AUDIOCLIP));
        var nextNodePort = node.GetOutputPortByName(SoundNode.OUT_PORT)?.FirstConnectedPort;
        if (nextNodePort != null)
        {
            runtimeNode.NextNodeID = nodeIDMap[nextNodePort.GetNode()];
        }
    }

    private void ProcessFadeNode(FadeNode node, RuntimeFadeNode runtimeNode, Dictionary<INode, string> nodeIDMap)
    {
        runtimeNode.duration = GetPortValue<float>(node.GetInputPortByName(FadeNode.IN_PORT_DURATION));
        runtimeNode.stayBlackDuration = GetPortValue<float>(node.GetInputPortByName(FadeNode.IN_PORT_STAYBLACKDURATION));
        runtimeNode.color = GetPortValue<Color>(node.GetInputPortByName(FadeNode.IN_PORT_COLOR));
        runtimeNode.blockSpaceDuringFade = GetPortValue<bool>(node.GetInputPortByName(FadeNode.IN_PORT_BLOCKSPACE));

        var nextNodePort = node.GetOutputPortByName(SoundNode.OUT_PORT)?.FirstConnectedPort;
        if (nextNodePort != null)
        {
            runtimeNode.NextNodeID = nodeIDMap[nextNodePort.GetNode()];
        }
    }

    // A helper method to prevent NullRefs on ports
    private T GetPortValueSafe<T>(Node node, string portName)
    {
        var port = node.GetInputPortByName(portName);
        if (port == null) return default;
        return GetPortValue<T>(port);
    }

    private T GetPortValue<T>(IPort port)
    {
        if (port == null) return default;

        if (port.IsConnected)
        {
            if (port.FirstConnectedPort.GetNode() is IVariableNode variableNode)
            {
                // Use the correct property name from IVariableNode (capital 'Variable')
                if (variableNode.Variable != null && variableNode.Variable.TryGetDefaultValue(out T value))
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
