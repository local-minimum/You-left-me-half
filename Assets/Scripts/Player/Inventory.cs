using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum InventoryEvent { PickUp, Drop, Move };

public delegate void InventoryChange(Lootable loot, InventoryEvent inventoryEvent, Vector3Int placement);

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
        PlayerController.OnPlayerMove += PlayerController_OnPlayerMove;
    }

    private void OnDisable()
    {
        Lootable.OnLoot -= Lootable_OnLoot;
        PlayerController.OnPlayerMove -= PlayerController_OnPlayerMove;
    }

    private Vector3Int playerPosition;
    private void PlayerController_OnPlayerMove(Vector3Int position, FaceDirection lookDirection)
    {
        playerPosition = position;
    }

    public int MaxRowForShape(int shapeHeight)
    {
        return Mathf.Min(Racks.Count, MaxRacks) * RackHeight - shapeHeight;
    }

    private bool CanPickupShape(Lootable loot, Vector3Int origin)
    {
        if (origin.y < 0) return false;

        int shapeHeight = loot.InventoryShape.Max(offset => offset.y) + 1;
        var inventorySlots = loot.InventorySlots();
        var originXY = origin.XY();

        if (MaxRowForShape(shapeHeight) < origin.y) return false;

        return Racks.All(rack => {
            if (rack.IsOutsideRack(origin, shapeHeight)) return true;
            if (!rack.IsSlotable(origin, loot.InventoryShape, out List<Vector2Int> offsets)) {
                Debug.Log($"{offsets.Count} violators");
                return offsets.All(offset => inventorySlots.Contains(offset + originXY));                
            }
            return true;
        });
    }
    
    private bool CanPickupShape(Lootable loot, out Vector3Int origin)
    {
        var shape = loot.InventoryShape;
        int shapeHeight = shape.Max(offset => offset.y) + 1;
        var maxY = MaxRowForShape(shapeHeight);
        var inventorySlots = loot.InventorySlots();

        for (var y = 0; y<=maxY; y++)
        {
            for (var x = 0; x<RackWidth; x++)
            {
                var anchor = new Vector3Int(x, y);
                var anchorXY = anchor.XY();

                if (Racks.All(rack => {
                    if (rack.IsOutsideRack(anchor, shapeHeight)) return true;
                    return rack.IsSlotable(anchor, loot.InventoryShape, out List<Vector2Int> offsets);
                }))
                {
                    origin = anchor;
                    return true;
                }
            }
        }

        origin = Vector3Int.zero;
        return false;
    }

    HashSet<Lootable> Loots = new HashSet<Lootable>();

    private void Drop(Lootable lootable)
    {
        Racks.ForEach(rack => rack.SetOccupancy(lootable.Coordinates, lootable.InventoryShape, false));
        Loots.Remove(lootable);
    }


    private void Pickup(Lootable lootable, Vector3Int placement)
    {
        Racks.ForEach(rack => rack.SetOccupancy(placement, lootable.InventoryShape, true));
        Loots.Add(lootable);
    }

    private void Lootable_OnLoot(Lootable loot, LootEventArgs args)
    {
        if (Loots.Contains(loot) && args.Owner != LootOwner.Player)
        {
            Drop(loot);
            OnInventoryChange?.Invoke(loot, InventoryEvent.Drop, Vector3Int.zero);
        }

        if (args.Owner != LootOwner.Player || args.Consumed) return;

        InventoryEvent inventoryEvent = InventoryEvent.PickUp;
        var inventoryRack = loot.GetComponent<InventoryRack>();
        var placement = args.Coordinates;

        if (loot.GetType() == typeof(Uplink) && loot.Owner == LootOwner.Level)
        {
            if (Level.instance.GridBoundsStatus(playerPosition.x, playerPosition.z) != GridEntity.InBound)
            {
                Debug.Log("Tried to pick up uplink when not allowed");
                return;
            }
        }

        if (inventoryRack != null)
        {
            // TODO: This could possibly go wrong if save/load isn't in order...
            var racks = Racks.Count();
            if (racks < MaxRacks)
            {
                inventoryRack.RackIndex = racks;
                args.Coordinates = new Vector3Int(0, racks);
                args.DefinedPosition = true;
                Racks.Add(inventoryRack);
            } else
            {
                return;
            }
        }
        else if (args.DefinedPosition)
        {            
            if (CanPickupShape(loot, args.Coordinates))
            {
                if (loot.Owner == LootOwner.Player)
                {
                    Drop(loot);
                    inventoryEvent = InventoryEvent.Move;
                }
            } else
            {
                return;
            }
        } else
        {
            if (CanPickupShape(loot, out Vector3Int coordinates))
            {
                if (loot.Owner == LootOwner.Player)
                {
                    Drop(loot);
                    inventoryEvent = InventoryEvent.Move;
                }
                args.Coordinates = coordinates;
                args.DefinedPosition = true;
                placement = coordinates;
            }
            else
            {
                return;
            }
        }

        loot.transform.SetParent(transform);
        Pickup(loot, placement);
        args.Consumed = true;

        OnInventoryChange?.Invoke(loot, inventoryEvent, placement);
    }

    public bool Has(System.Func<Lootable, bool> predicate, out Lootable loot)
    {
        loot = Loots.Where(predicate).FirstOrDefault();
        return loot != null;
    }

    private IEnumerable<T> GetLootsByType<T>() => Loots.Where(loot => loot.GetType() == typeof(T)) as IEnumerable<T>;
    private IEnumerable<Canister> GetXPCanisters() => GetLootsByType<Canister>().Where(canister => canister.CanisterType == CanisterType.XP);
    private IEnumerable<Canister> GetHealthCanisters() => GetLootsByType<Canister>().Where(canister => canister.CanisterType == CanisterType.XP);
    private IEnumerable<Canister> GetCanisters(CanisterType type) => GetLootsByType<Canister>().Where(canister => canister.CanisterType == type);


    public int XP { get => GetXPCanisters().Sum(canister => canister.Stored); }
    public int Health { get => GetHealthCanisters().Sum(canister => canister.Stored); }

    public void Receive(int amount, CanisterType type)
    {
        var canisters = GetCanisters(type).ToArray();
        for (int i=0; i<canisters.Length; i++)
        {
            var canister = canisters[i];
            if (canister.Receive(amount, out int remaining))
            {
                amount = remaining;
            } else
            {
                break;
            }
        }
    }

    public void Withdraw(int amount, CanisterType type)
    {
        var canisters = GetCanisters(type).ToArray();
        for (int i = 0; i < canisters.Length; i++)
        {
            var canister = canisters[i];
            if (canister.Withdraw(amount, out int remaining))
            {
                amount = remaining;
            }
            else
            {
                break;
            }
        }
    }
}
