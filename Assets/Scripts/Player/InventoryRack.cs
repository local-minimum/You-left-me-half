using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using DeCrawl.Primitives;
using DeCrawl.UI;

namespace YLHalf
{
    [RequireComponent(typeof(Lootable))]
    public abstract class InventoryRack : InventoryBagJoinable
    {
        public int[,] Corruption = new int[Inventory.RackHeight, Inventory.RackWidth];
        private void Awake()
        {
            InitOccupancy(Inventory.RackHeight, Inventory.RackWidth);
            ParseInitialCorruption();
        }

        public IEnumerable<string> CorruptionAsStrings
        {
            get
            {
                for (int y = 0; y < Inventory.RackHeight; y++)
                {
                    var row = "";
                    for (int x = 0; x < Inventory.RackWidth; x++)
                    {
                        row += Corruption[y, x].ToString() + "\t";
                    }
                    yield return row;
                }
            }
        }

        abstract protected string[] InitialCorruption { get; }

        void ParseInitialCorruption()
        {
            for (int y = 0; y < Inventory.RackHeight; y++)
            {
                var corruption = InitialCorruption[y];
                for (int x = 0; x < Inventory.RackWidth; x++)
                {
                    if (int.TryParse(corruption.Substring(x, 1), out int amount))
                    {
                        Corruption[y, x] = amount;
                    }
                }
            }
        }

        public bool ClearOneCorruption(Vector3Int localCoordinates, System.Func<bool> effect, out int remaining)
        {
            var slots = ContainedCoordinates(localCoordinates, new Vector2Int[] { Vector2Int.zero }).ToArray();
            if (slots.Length == 0)
            {
                remaining = -1;
                return false;
            }
            var slot = slots[0];
            if (Corruption[slot.y, slot.x] > 0)
            {
                if (effect())
                {
                    Corruption[slot.y, slot.x]--;
                    remaining = Corruption[slot.y, slot.x];
                    return true;
                }
                remaining = -1;
                return false;
            }
            remaining = -1;
            return false;
        }

        override public bool IsSpecial(int y, int x) => Corruption[y, x] == 0;

        public override void ApplySlotState(InventorySlotUI slot, int localY, int localX)
        {            
            if (Occupied[localY, localX])
            {
                slot.State = InventorySlotUIState.Occupied;
                slot.RomanNumeralCount = 0;
            }
            else if (Corruption[localY, localX] > 0)
            {
                slot.State = InventorySlotUIState.Special;
                slot.RomanNumeralCount = Corruption[localY, localX];
            }
            else
            {
                slot.State = InventorySlotUIState.Free;
                slot.RomanNumeralCount = 0;
            }
        }
    }
}