using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(Lootable))]
public class InventoryRack : MonoBehaviour
{        
    [System.Serializable]
    struct InitialCorruption
    {
        public Vector2Int Coordinates;
        public int Amount;
    }

    public bool[,] Occupied = new bool[Inventory.RackHeight, Inventory.RackWidth];    
    private int[,] Corruption = new int[Inventory.RackHeight, Inventory.RackWidth];

    [SerializeField] InitialCorruption[] initialCorruptions;

    private void Awake()
    {
        for (int i = 0; i < initialCorruptions.Length; i++)
        {
            var corruption = initialCorruptions[i];
            Corruption[corruption.Coordinates.y, corruption.Coordinates.x] = corruption.Amount;
        }
    }

    public int RackIndex;

    public bool IsOutsideRack(Vector3Int origin, int shapeHeight)
    {

        return origin.y + shapeHeight < RackIndex * Inventory.RackHeight ||
            origin.y >= (RackIndex + 1) * Inventory.RackHeight;
    }

    public bool IsSlotable(Vector3Int origin, Vector2Int[] offsets)
    {
        var yOffset = RackIndex * Inventory.RackHeight;
        return offsets
            .Select(offset => new Vector2Int(
                offset.x + origin.x, 
                offset.y + origin.y - yOffset
                ))
            .Where(coords => coords.y >= 0 && coords.y < Inventory.RackHeight && coords.x >= 0 && coords.x < Inventory.RackWidth)
            .All(coords => !Occupied[coords.y, coords.x] && Corruption[coords.y, coords.x] == 0);
    }
}
