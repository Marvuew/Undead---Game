using UnityEngine;
using System.Collections.Generic;
[CreateAssetMenu(menuName = "Case and Clues/New Case")]
public class Case : ScriptableObject
{
    public Undead culprit;
    public List<Clue> clues = new List<Clue>();
}
