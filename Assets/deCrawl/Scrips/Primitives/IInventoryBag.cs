using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DeCrawl.UI;

namespace DeCrawl.Primitives
{
    public interface IInventoryBag
    {
        public bool[,] Occupied { get; }
        public int Rows { get; }
        public int Columns { get; }

        public bool IsSpecial(int y, int x);

        public void ApplySlotState(InventorySlotUI slot, int localY, int localX); 
    }
}
