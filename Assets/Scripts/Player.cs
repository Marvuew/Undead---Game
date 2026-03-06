using System;
using System.Collections.Generic;
using System.Text;

namespace Assets.Scripts
{
    class Player
    {
        public static Player Instance { get; private set; }
        private Player() { }
        public int alignment = 50;
        public List<string> inventory;
        public void AddToInventory(string item)  { inventory.Add(item); }
        public void RemoveFromInventory(string item) { inventory.Remove(item); }
    }
}
