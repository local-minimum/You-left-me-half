using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DeCrawl.Primitives;
using DeCrawl.UI;

namespace FP
{
    public class SingleSpaceOnlyBag : Lootable, IInventoryBag
    {
        [SerializeField]
        int rows = 5;
        [SerializeField]
        int cols = 6;

        bool[,] occupancy;
        public bool[,] Occupied
        {
            get
            {
                if (occupancy == null)
                {
                    occupancy = new bool[rows, cols];
                }

                return occupancy;
            }
        }

        public int Rows => rows;

        public int Columns => cols;

        public void ApplySlotState(InventorySlotUI slot, int localY, int localX)
        {
            slot.State = Occupied[localY, localX] ? InventorySlotUIState.Occupied : InventorySlotUIState.Free;
        }

        public bool IsOutsideAfter(Vector3Int localOrigin)
        {
            return localOrigin.y >= rows;
        }

        public bool IsOutsideBefore(Vector3Int localOrigin, int shapeHeight)
        {
            return localOrigin.y + shapeHeight < 0;
        }

        public bool IsSpecial(int y, int x) => false;        

        public void SetOccupancy(Vector3Int globalOrigin, int offsetBagsRows, Vector2Int[] offsets, bool value)
        {
            Occupied[globalOrigin.y, globalOrigin.x] = value;
        }

        public int Slotable(Vector3Int localOrigin, Vector2Int[] offsets, out List<Vector2Int> violators)
        {
            if (Occupied[localOrigin.y, localOrigin.x])
            {
                violators = new List<Vector2Int>() { Vector2Int.zero };
                return 1;
            }

            violators = new List<Vector2Int>();
            return 0;
        }
    }
}
