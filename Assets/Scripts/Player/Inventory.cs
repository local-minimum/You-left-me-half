using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Inventory : MonoBehaviour
{
    public static readonly int RackWidth = 16;
    public static readonly int RackHeight = 4;

    public List<InventoryRack> Racks = new List<InventoryRack>();
    public int MaxRacks;

    private void OnEnable()
    {
        Lootable.OnLoot += Lootable_OnLoot;
    }

    private void OnDisable()
    {
        Lootable.OnLoot -= Lootable_OnLoot;
    }

    public int MaxRowForShape(int shapeHeight)
    {
        return Mathf.Min(Racks.Count, MaxRacks) * RackHeight - shapeHeight;
    }

    private bool CanPickupShape(Vector3Int origin, Vector2Int[] shape)
    {
        int shapeHeight = shape.Max(offset => offset.y) + 1;

        if (MaxRowForShape(shapeHeight) < origin.y) return false;

        return Racks.All(rack => rack.IsOutsideRack(origin, shapeHeight) || rack.IsSlotable(origin, shape));
    }
    
    private bool CanPickupShape(Vector2Int[] shape, out Vector3Int origin)
    {
        int shapeHeight = shape.Max(offset => offset.y) + 1;
        var maxY = MaxRowForShape(shapeHeight);

        for (var y = 0; y<=maxY; y++)
        {
            for (var x = 0; x<RackWidth; x++)
            {
                var anchor = new Vector3Int(x, y);
                if (Racks.All(rack => rack.IsOutsideRack(anchor, shapeHeight) || rack.IsSlotable(anchor, shape)))
                {
                    origin = anchor;
                    return true;
                }
            }
        }

        origin = Vector3Int.zero;
        return false;
    }


    private void Lootable_OnLoot(Lootable loot, LootEventArgs args)
    {
        if (args.Owner != LootOwner.Player || args.Consumed) return;

        var inventoryRack = loot.GetComponent<InventoryRack>();

        if (inventoryRack != null)
        {
            // TODO: This could possibly go wrong if save/load isn't in order...
            var racks = Racks.Count();
            if (racks < MaxRacks)
            {
                inventoryRack.RackIndex = racks;
                loot.Coordinates = new Vector3Int(0, racks);
                Racks.Add(inventoryRack);
            } else
            {
                return;
            }
        }
        else if (args.DefinedPosition)
        {
            if (CanPickupShape(args.Coordinates, loot.InventoryShape))
            {
                loot.Coordinates = args.Coordinates;                
            } else
            {
                return;
            }
        } else
        {
            if (CanPickupShape(loot.InventoryShape, out Vector3Int coordinates))
            {
                loot.Coordinates = coordinates;
            } else
            {
                return;
            }
        }

        loot.Owner = args.Owner;
        loot.transform.SetParent(transform);
        args.Consumed = true;
    }

    public bool Has(System.Func<Lootable, bool> predicate, out Lootable loot)
    {
        loot = GetComponentsInChildren<Lootable>().Where(predicate).FirstOrDefault();
        return loot != null;
    }
}
