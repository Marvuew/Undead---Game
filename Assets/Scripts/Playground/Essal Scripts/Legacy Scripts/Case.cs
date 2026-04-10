using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Playground.GameScripts
{
    [CreateAssetMenu(fileName = "Case", menuName = "Scriptable Objects/Case")]
    public class Case : ScriptableObject
    {
        public List<Clue> clues;
        public Undead culprit;
    }
}
