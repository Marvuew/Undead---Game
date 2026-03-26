using System;
using System.Collections.Generic;
using System.Text;

namespace Assets.Scripts.GameScripts
{
    [System.Serializable]
    public class Choice
    {
        public string text;
        public int humanityChange;
        public int undeadChange;
        public Dialogue nextDialogue;
    }
}
