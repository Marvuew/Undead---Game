using System;
using System.Collections.Generic;
using System.Text;
using static UnityEditor.Progress;

namespace Assets.Scripts.GameScripts
{
    class Player
    {
        private int humanity = 50;
        List<string> inventory;
        public int stamina;
        public event Action onHumanityChanged;


        public static Player instance { get; private set; }
        private Player() { }
        public void ChangeHumanity(int change) 
        { 
            humanity += change;
            onHumanityChanged?.Invoke();
        }
    }
}
