using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTree
{
    public class DialogueNode
    {
        public string id;
        public string speaker;
        public string text;
        public string choiceLabel;
        public string itemReward;
        public string moveToRoom;
        public List<DialogueNode> Children = new List<DialogueNode>();
        
        public DialogueNode(string id, string speaker, string text, string choiceLabel = null, string itemReward = null, string moveToRoom = null)
        {
            this.id = id;
            this.speaker = speaker;
            this.text = text;
            this.choiceLabel = choiceLabel;
            this.itemReward = itemReward;
            this.moveToRoom = moveToRoom;
        }
        public int depth;
        public bool isLeaf => Children.Count == 0;

        public DialogueNode AddChild(DialogueNode child)
        {
            Children.Add(child);
            return this;
        }

    }

    public DialogueNode Root { get; private set; }
    public DialogueNode CurrentNode { get; private set; }
    public string LastTraversalInfo { get; private set; } = "";

    private Stack<string> pathStack = new Stack<string>();

    public DialogueTree(DialogueNode root)
    {
        Root = root;
        Root.depth = 0;
        CurrentNode = root;
    }

    public DialogueNode SelectChoice(int childIndex)
    {
        if (childIndex < 0 || childIndex >= CurrentNode.Children.Count)
        {
            LastTraversalInfo = $"Invalid choice index {childIndex}";
            return CurrentNode;
        }

        pathStack.Push(CurrentNode.id);
        CurrentNode = CurrentNode.Children[childIndex];

        LastTraversalInfo =
            $"Traversed: {pathStack.Peek()} → {CurrentNode.id}\n" +
            $"Current depth: {CurrentNode.depth}  |  " +
            $"Is leaf: {CurrentNode.isLeaf}  |  " +
            $"Children: {CurrentNode.Children.Count}\n" +
            $"Path: {string.Join(" → ", new List<string>(pathStack).ToArray())} → {CurrentNode.id}";

        return CurrentNode;
    }

    public void Reset()
    {
        CurrentNode = Root;
        pathStack.Clear();
        LastTraversalInfo = "Tree reset to root node.";
    }

    public static DialogueTree BuildScene2Tree()
    {
        //Root
        var root = new DialogueNode("root", "Rookie Officer: ",
            "'Morning Short-fang, a family has come down with a sickness and it won't seem to go away. I fear there is more to this than a human can do, so unfortunately our only hope is you.'"            );

        var offended = root.AddChild(new DialogueNode("Offended", "Dhampir: ",
            "'Careful there, my fangs aren't that short...'",
            "\"Offended\""));
        var letItSlide = root.AddChild(new DialogueNode("Let it slide", "Dhampir: ",
            "'Glad to be of service'",
            "\"Let it slide\""));

        return new DialogueTree(root);
    }
    }
