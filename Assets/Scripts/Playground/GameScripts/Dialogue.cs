using Assets.Scripts.GameScripts;
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Dialogue", menuName = "Scriptable Objects/Dialouge")]
public class Dialogue : ScriptableObject
{
    public string speaker;
    [TextArea(3,5)] public string text;
    public float typingDelay = 0.05f;
    public bool isShaking;
    public List<Choice> choices;
    
}
