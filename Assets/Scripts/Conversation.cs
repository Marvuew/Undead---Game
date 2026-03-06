using Assets.Scripts;
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Choice
{
    public string text;
    public DialogueNode nextNode;
    public int alignmentChange;
}
