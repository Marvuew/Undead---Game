using System;
using System.Collections.Generic;
using System.Text;

namespace Assets.Scripts
{
    class Player
    {
        public int Alignment = 50;
        public List<string> inventory;
        public void AddToInventory(string item)  { inventory.Add(item); }
        public void RemoveFromInventory(string item) { inventory.Remove(item); }
    }
}
