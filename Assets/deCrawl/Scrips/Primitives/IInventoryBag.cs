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

        /// <summary>
        /// If shape falls outside and in a bag before this one
        /// </summary>
        /// <param name="localOrigin"></param>
        /// <param name="shapeHeight"></param>
        /// <returns></returns>
        public bool IsOutsideBefore(Vector3Int localOrigin, int shapeHeight);

        /// <summary>
        /// If shape falls outside and in a bag after this one
        /// </summary>
        /// <param name="localOrigin"></param>
        /// <param name="shapeHeight"></param>
        /// <returns></returns>
        public bool IsOutsideAfter(Vector3Int localOrigin);

        /// <summary>
        /// Reports how many of the offsets can be slotted in the current bag
        /// </summary>
        /// <param name="localOrigin">Reference position in bag coordinate systsem for the loot.
        /// This may be outside of the bag and still slot some offsets
        /// </param>
        /// <param name="offsets">Offsets relative to the orgin that the loot requires</param>
        /// <param name="violators">Coordinates in the bag coordinate system that are inside the bag
        /// and cannot be occupied by the loot.
        /// </param>
        /// <returns></returns>
        public int Slotable(Vector3Int localOrigin, Vector2Int[] offsets, out List<Vector2Int> violators);

        public void SetOccupancy(Vector3Int globalOrigin, int offsetBagsRows, Vector2Int[] offsets, bool value);
    }
}
