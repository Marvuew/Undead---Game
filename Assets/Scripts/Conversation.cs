using Assets.Scripts;
using UnityEngine;
using System.Collections.Generic;

public class Conversation
{
    public List<Node> dialouge { get; private set; }
    int layer;
    public Conversation(List<Node> dialouge)
    {
        this.dialouge = dialouge;
    }
    public Conversation(Node node)
    {
        dialouge = new List<Node>();
        dialouge.Add(node);
    }
    public Node GetNextNode(Node node, int choice) 
    {
        return node.childern[choice];
    }
}
