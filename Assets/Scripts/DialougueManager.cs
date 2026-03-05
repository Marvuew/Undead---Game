using Assets.Scripts;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class DialougeManager : MonoBehaviour
{
    public static DialougeManager instance;
    public Dictionary<string, Conversation> converations;
    string ConvoKey;
    Node currentNode;

    public void InnitDialouge(string ConvoKey) 
    {
        if (!converations.TryGetValue(ConvoKey, out Conversation conversation))
            return;
        GameManager.instance.dialougeBox.SetActive(true);
        this.ConvoKey = ConvoKey;
        currentNode = conversation.dialouge[0];
    }

    public void UpdateDialouge(int choice /*from 0*/) 
    {
        if (converations.TryGetValue(ConvoKey, out Conversation value))
        {
            currentNode = value.GetNextNode(currentNode, choice);
            GameManager.instance.dialougeTxt.text = currentNode.text;
            GameManager.instance.dialougeTxt.text = currentNode.speaker;
            if (!currentNode.isleaf) 
            { 
                GameManager.instance.optionsBox.SetActive(true);
                GameManager.instance.optionATxt.text = value.GetNextNode(currentNode,0).text;
                GameManager.instance.optionATxt.text = value.GetNextNode(currentNode, 1).text;
            }
        }
    }
    public void AddNode(string convoKey, Node node) 
    {
        if (converations.ContainsKey(convoKey))
            converations[convoKey].dialouge.Add(node);
        else
            converations.Add(convoKey, new Conversation(node));
    }
}
