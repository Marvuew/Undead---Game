using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Playground.GameScripts
{
    public enum Undead { Strigoi, Lamia, Draugr, Banshee, Myling, Nisse, Vaettir, Changeling, Fairy, WillOWisp }
    
    [CreateAssetMenu(fileName = "Clue", menuName = "Scriptable Objects/Clue")]
    public class Clue : ScriptableObject
    {
        public Vector3 position;
        [TextArea(1,5)] public string description;
        public List<Undead> undeadTypes;
    }
}
