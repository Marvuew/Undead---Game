using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Assets.Scripts
{
    [CreateAssetMenu(menuName = "Dialogue/Node")]
    public class DialogueNode : ScriptableObject
    {
        public string speaker;
        [TextArea(3, 5)]
        public string text;
        public List<Choice> choices = new List<Choice>();
    }
}
