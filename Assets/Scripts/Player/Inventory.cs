using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum InventoryEvent { PickUp, Drop, Move };

public delegate void InventoryChange(Lootable loot, InventoryEvent inventoryEvent, Vector3Int previousOrigin);

public class Inventory : MonoBehaviour
{
    public static event InventoryChange OnInventoryChange;

    public static readonly int RackWidth = 8;
    public static readonly int RackHeight = 4;

    public static int RackLimit = 4;

    public List<InventoryRack> Racks = new List<InventoryRack>();
    public int MaxRacks = 2;

    private void Start()
    {
        if (MaxRacks > RackLimit)
        {
            Debug.LogError($"{name} claims too many rack slots");
        }
    }

    private void OnEnable()
    {
        Lootable.OnLoot += Lootable_OnLoot;
    }

    private void OnDestroy()
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

    private void Drop(Lootable lootable)
    {
        Racks.ForEach(rack => rack.SetOccupancy(lootable.Coordinates, lootable.InventoryShape, false));
    }


    private void Pickup(Lootable lootable)
    {
        Racks.ForEach(rack => rack.SetOccupancy(lootable.Coordinates, lootable.InventoryShape, true));
    }

    private void Lootable_OnLoot(Lootable loot, LootEventArgs args)
    {
        if (loot.Owner == LootOwner.Player && args.Owner != LootOwner.Player)
        {
            Drop(loot);
            OnInventoryChange?.Invoke(loot, InventoryEvent.Drop, Vector3Int.zero);
        }

        if (args.Owner != LootOwner.Player || args.Consumed) return;

        InventoryEvent inventoryEvent = InventoryEvent.PickUp;
        var inventoryRack = loot.GetComponent<InventoryRack>();
        var previousCoordinates = loot.Coordinates;

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
                if (loot.Owner == LootOwner.Player)
                {
                    Drop(loot);
                    inventoryEvent = InventoryEvent.Move;
                }
                loot.Coordinates = args.Coordinates;                
            } else
            {
                return;
            }
        } else
        {
            if (CanPickupShape(loot.InventoryShape, out Vector3Int coordinates))
            {
                if (loot.Owner == LootOwner.Player)
                {
                    Drop(loot);
                    inventoryEvent = InventoryEvent.Move;
                }
                loot.Coordinates = coordinates;
            }
            else
            {
                return;
            }
        }

        loot.Owner = args.Owner;
        loot.transform.SetParent(transform);
        Pickup(loot);
        args.Consumed = true;

        OnInventoryChange?.Invoke(loot, inventoryEvent, previousCoordinates);
    }

    public bool Has(System.Func<Lootable, bool> predicate, out Lootable loot)
    {
        loot = GetComponentsInChildren<Lootable>().Where(predicate).FirstOrDefault();
        return loot != null;
    }
}
