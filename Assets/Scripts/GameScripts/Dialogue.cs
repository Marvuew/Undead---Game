using Assets.Scripts.GameScripts;
using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Dialogue", menuName = "Scriptable Objects/Dialouge")]
public class Dialogue : ScriptableObject
{
    public string speaker;
    [TextArea(3,5)] public string text;
    public List<Choice> choices;
}
