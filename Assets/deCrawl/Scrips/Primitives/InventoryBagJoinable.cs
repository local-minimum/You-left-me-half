using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DeCrawl.Utils;

namespace DeCrawl.Primitives
{
    /// <summary>
    /// This bag type, when used in an inventory, allows for holding part of a loot
    /// and letting another bag hold the other part.
    /// </summary>
    public abstract class InventoryBagJoinable : MonoBehaviour
    {
        public bool[,] Occupied { get; private set; }
        
        public int Rows => Occupied.GetLength(0);
        public int Columns => Occupied.GetLength(1);

        public void InitOccupancy(int rows, int columns)
        {
            Occupied = new bool[rows, columns];
        }

        public IEnumerable<string> OccupancyAsStrings
        {
            get
            {
                if (Occupied == null) {
                    yield return "NOT INITIALIZED";
                    yield break;
                }

                var maxX = Columns;
                for (int y = 0, maxY = Rows; y < maxY; y++)
                {
                    var row = "";
                    for (int x = 0; x < maxX; x++)
                    {
                        row += (Occupied[y, x] ? "Y" : "N") + "\t";
                    }
                    yield return row;
                }
            }
        }

        public int BagIndex;

        /// <summary>
        /// If shape falls outside and in a bag before this one
        /// </summary>
        /// <param name="localOrigin"></param>
        /// <param name="shapeHeight"></param>
        /// <returns></returns>
        public bool IsOutsideBefore(Vector3Int localOrigin, int shapeHeight)
        {
            return localOrigin.y + shapeHeight < 0;
        }

        /// <summary>
        /// If shape falls outside and in a bag after this one
        /// </summary>
        /// <param name="localOrigin"></param>
        /// <param name="shapeHeight"></param>
        /// <returns></returns>
        public bool IsOutsideAfter(Vector3Int localOrigin, int shapeHeight)
        {
            return localOrigin.y >= Rows;
        }

        /// <summary>
        /// Converts offsets to coordinates and filters for those inside bag
        /// </summary>
        /// <param name="localOrigin"></param>
        /// <param name="offsets"></param>
        /// <returns></returns>
        protected IEnumerable<Vector2Int> ContainedCoordinates(Vector3Int localOrigin, Vector2Int[] offsets)
        {
            var maxY = Rows;
            var maxX = Columns;

            return offsets
                .Select(offset => new Vector2Int(localOrigin.x + offset.x, localOrigin.y + offset.y))
                .Where(coords => coords.y >= 0 && coords.y < maxY && coords.x >= 0 && coords.x < maxX);
        }

        abstract protected bool ExtraConditionAllowsSlotting(Vector2Int localCoords);

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
        public int Slotable(Vector3Int localOrigin, Vector2Int[] offsets, out List<Vector2Int> violators)
        {
            violators = new List<Vector2Int>();
            int slotable = 0;
            foreach(var coords in ContainedCoordinates(localOrigin, offsets))
            {
                var valid = !Occupied[coords.y, coords.x] && ExtraConditionAllowsSlotting(coords);
                if (valid)
                {
                    slotable++;
                } else
                {
                    violators.Add(coords - localOrigin.XYVector2Int());
                }
            }
            return slotable;
        }

        public void SetOccupancy(Vector3Int globalOrigin, int offsetBagsRows, Vector2Int[] offsets, bool value)
        {
            foreach (var slot in ContainedCoordinates(globalOrigin + new Vector3Int(0, -offsetBagsRows), offsets))
            {
                Occupied[slot.y, slot.x] = value;
            }
        }
    }
}
