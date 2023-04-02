using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(Lootable))]
public abstract class InventoryRack : MonoBehaviour
{        
    public bool[,] Occupied = new bool[Inventory.RackHeight, Inventory.RackWidth];    
    private int[,] Corruption = new int[Inventory.RackHeight, Inventory.RackWidth];

    public IEnumerable<string> CorruptionAsStrings
    {
        get
        {
            for (int y = 0; y<Inventory.RackHeight; y++)
            {
                var row = "";
                for (int x = 0; x<Inventory.RackWidth; x++)
                {
                    row += Corruption[y, x].ToString() + "\t";
                }
                yield return row;
            }
        }
    }

    public IEnumerable<string> OccupancyAsStrings
    {
        get
        {
            for (int y = 0; y < Inventory.RackHeight; y++)
            {
                var row = "";
                for (int x = 0; x < Inventory.RackWidth; x++)
                {
                    row += (Occupied[y, x] ? "Y" : "N") + "\t";
                }
                yield return row;
            }
        }
    }


    abstract protected string[] InitialCorruption { get; }

    private void Awake()
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

    public int RackIndex;

    public bool IsOutsideRack(Vector3Int origin, int shapeHeight)
    {

        return origin.y + shapeHeight < RackIndex * Inventory.RackHeight ||
            origin.y >= (RackIndex + 1) * Inventory.RackHeight;
    }

    private IEnumerable<Vector2Int> TransformToInternalCoordinates(Vector3Int origin, Vector2Int[] offsets)
    {
        var yOffset = RackIndex * Inventory.RackHeight;
        return offsets
            .Select(offset => new Vector2Int(
                offset.x + origin.x,
                offset.y + origin.y - yOffset
                ))
            .Where(coords => coords.y >= 0 && coords.y < Inventory.RackHeight && coords.x >= 0 && coords.x < Inventory.RackWidth);
    }

    public bool IsSlotable(Vector3Int origin, Vector2Int[] offsets)
    {
        return TransformToInternalCoordinates(origin, offsets)
            .All(coords => !Occupied[coords.y, coords.x] && Corruption[coords.y, coords.x] == 0);
    }

    public void SetOccupancy(Vector3Int origin, Vector2Int[] offsets, bool value)
    {
        var internalSlots = TransformToInternalCoordinates(origin, offsets).ToArray();
        for (int i = 0; i<internalSlots.Length; i++)
        {
            var slot = internalSlots[i];
            Occupied[slot.y, slot.x] = value;
        }
    }
}
