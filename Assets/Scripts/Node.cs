using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Assets.Scripts
{
    public class Node : ScriptableObject
    {
        public bool isleaf { get; }
        public string text { get;}
        public Node[] childern;
        public int reputationPoints; // 0-100
        public string speaker;

        public Node(string text, bool isleaf = false, Node[] childern = null, int reputationPoints = 0, string convoKey) 
        { 
            this.text = text;
            this.isleaf = isleaf;
            this.childern = childern;
            this.reputationPoints = reputationPoints;
            DialougeManager.instance.AddNode(convoKey,this);
        }
    }
}
