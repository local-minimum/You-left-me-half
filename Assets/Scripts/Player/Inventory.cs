using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum InventoryEvent { PickUp, Drop, Move };

public delegate void InventoryChange(Lootable loot, InventoryEvent inventoryEvent, Vector3Int placement);
public delegate void CanisterChange(CanisterType type, int stored, int capacity);

public class Inventory : MonoBehaviour
{
    public static event CanisterChange OnCanisterChange;    
    public static event InventoryChange OnInventoryChange;

    public static readonly int RackWidth = 8;
    public static readonly int RackHeight = 4;

    public static int RackLimit = 4;

    public List<InventoryRack> Racks = new List<InventoryRack>();
    public int MaxRacks = 2;

    private bool alive { get; set; }

    private void Start()
    {
        if (MaxRacks > RackLimit)
        {
            Debug.LogError($"{name} claims too many rack slots");
        }
        alive = true;
    }

    private void OnEnable()
    {
        Lootable.OnLoot += Lootable_OnLoot;
        PlayerController.OnPlayerMove += PlayerController_OnPlayerMove;
        BattleMaster.OnHitPlayer += BattleMaster_OnHitPlayer;
        Enemy.OnKillEnemy += Enemy_OnKillEnemy;
    }

    private void OnDisable()
    {
        Lootable.OnLoot -= Lootable_OnLoot;
        PlayerController.OnPlayerMove -= PlayerController_OnPlayerMove;
        BattleMaster.OnHitPlayer -= BattleMaster_OnHitPlayer;
        Enemy.OnKillEnemy -= Enemy_OnKillEnemy;
    }

    private void Enemy_OnKillEnemy(Enemy enemy)
    {
        Receive(enemy.XPReward, CanisterType.XP);
    }

    private void BattleMaster_OnHitPlayer(int amount)
    {
        Withdraw(amount, CanisterType.Health);
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

    string LootsSummary(IEnumerable<Lootable> loots) { 
        var s = string.Join(", ", loots.Select(v => v.Id));
        if (string.IsNullOrEmpty(s)) return "-EMPTY-";
        return s;
    }
    
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
            if (loot.GetType() == typeof(Canister))
            {
                InvokeCanisterChange(((Canister)loot).CanisterType);
            }
        }

        if (args.Owner != LootOwner.Player || args.Consumed) return;

        InventoryEvent inventoryEvent = InventoryEvent.PickUp;
        var inventoryRack = loot.GetComponent<InventoryRack>();
        var placement = args.Coordinates;

        if (loot.GetType() == typeof(Uplink) && loot.Owner == LootOwner.Level)
        {
            if (Level.instance.GridBaseStatus(playerPosition.x, playerPosition.z) != GridEntity.InBound)
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

        if (inventoryEvent == InventoryEvent.PickUp && loot.GetType() == typeof(Canister))
        {
            InvokeCanisterChange(((Canister)loot).CanisterType);
        }

    }

    public bool RemoveOneCorruption(Vector3Int coordinates, out bool cleared)
    {
        for (int i = 0, l = Racks.Count; i<l; i++)
        {
            var rack = Racks[i];
            if (rack.ClearOneCorruption(coordinates, out cleared))
            {
                return true;
            }
        }

        cleared = false;
        return false;
    }

    public bool Has(System.Func<Lootable, bool> predicate, out Lootable loot)
    {
        loot = Loots.Where(predicate).FirstOrDefault();
        return loot != null;
    }

    private IEnumerable<T> GetLoot<T>() where T : Lootable => Loots
        .Where(loot => loot.GetType() == typeof(T))
        .Select(loot => (T)loot);

    private IEnumerable<Canister> GetXPCanisters() => GetLoot<Canister>()
        .Where(canister => canister.CanisterType == CanisterType.XP);
    private IEnumerable<Canister> GetHealthCanisters() => GetLoot<Canister>()
        .Where(canister => canister.CanisterType == CanisterType.XP);
    private IEnumerable<Canister> GetCanisters(CanisterType type) => GetLoot<Canister>()
        .Where(canister => canister.CanisterType == type);


    public int XP { get => GetXPCanisters().Sum(canister => canister.Stored); }
    public int Health { get => GetHealthCanisters().Sum(canister => canister.Stored); }

    
    public void InvokeCanisterChange(CanisterType type)
    {
        var (stored, capacity) = GetCanisters(type)
            .Aggregate((0, 0), (acc, canister) => (acc.Item1 + canister.Stored, acc.Item2 + canister.Capacity));
        
        OnCanisterChange?.Invoke(type, stored, capacity);
    }

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

        InvokeCanisterChange(type);
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
            } else
            {
                break;
            }
        }

        InvokeCanisterChange(type);
    }
}
