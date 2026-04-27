using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Case and Clues/New Clue")]
public class Clue : ScriptableObject
{
    public Sprite sprite;
    public Vector3 position;
    [TextArea(3, 5)] public string description;
    public RuntimeDialogueGraph dialogueGraph;
    public List<UndeadType> undeadTypes;
    public ClueType clueType;
}

public enum ClueType
{
    Human,
    Item
}
