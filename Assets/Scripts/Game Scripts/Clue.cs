using System.Collections.Generic;
using UnityEngine;

public enum Undead { Strigoi, Lamia, Draugr, Banshee, Myling, Nisse, Vaettir, Changeling, Fairy, WillOWisp }

[CreateAssetMenu(menuName = "Case and Clues/New Clue")]
public class Clue : ScriptableObject
{
    public Vector3 position;
    public Sprite sprite;
    [TextArea(3, 5)] public string description;
    public RuntimeDialogueGraph dialogueGraph;
    public List<Undead> undeadTypes;
}
