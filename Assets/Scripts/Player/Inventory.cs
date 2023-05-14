using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using DeCrawl.Utils;
using DeCrawl.Primitives;
using DeCrawl.Enemies;
using DeCrawl.Systems;

namespace YLHalf
{
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
            BattleMaster.OnHitPlayer += BattleMaster_OnHitPlayer;
            EnemyBase.OnKillEnemy += Enemy_OnKillEnemy;
        }

        private void OnDisable()
        {
            Lootable.OnLoot -= Lootable_OnLoot;
            PlayerController.OnPlayerMove -= PlayerController_OnPlayerMove;
            BattleMaster.OnHitPlayer -= BattleMaster_OnHitPlayer;
            EnemyBase.OnKillEnemy -= Enemy_OnKillEnemy;
        }

        private void Enemy_OnKillEnemy(GameObject enemy, int xpReward)
        {
            Receive(xpReward, CurrencyType.XP);
        }

        private void BattleMaster_OnHitPlayer(int amount)
        {
            Withdraw(amount, CurrencyType.Health);
        }

        private Vector3Int playerPosition;
        private void PlayerController_OnPlayerMove(Vector3Int position, CardinalDirection lookDirection)
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
            var originXY = origin.XYVector2Int();

            if (MaxRowForShape(shapeHeight) < origin.y) return false;

            var localOrigin = new Vector3Int(origin.x, origin.y, origin.z);

            return Racks.Sum(rack =>
            {
                // Not in the rack
                if (rack.IsOutsideBefore(localOrigin, shapeHeight)) return 0;
                if (rack.IsOutsideAfter(localOrigin, shapeHeight))
                {
                    localOrigin.y -= rack.Rows;
                    return 0;
                }

                // Fully or partially in rack
                var slots = rack.Slotable(localOrigin, loot.InventoryShape, out List<Vector2Int> violators);
                if (slots != loot.InventoryShape.Length)
                {
                    if (violators.All(violator => inventorySlots.Contains(violator + originXY)))
                    {
                        return slots + violators.Count();
                    }
                    Debug.Log($"{violators.Count} violators");
                    return 0;
                }
                return slots;
            }) == loot.InventoryShape.Length;
        }

        private bool CanPickupShape(Lootable loot, out Vector3Int origin)
        {
            var shape = loot.InventoryShape;
            int shapeHeight = shape.Max(offset => offset.y) + 1;
            var maxY = MaxRowForShape(shapeHeight);
            var inventorySlots = loot.InventorySlots();
            Debug.Log($"{loot.Id}: {maxY} x {RackWidth}");
            for (var y = 0; y <= maxY; y++)
            {
                for (var x = 0; x < RackWidth; x++)
                {
                    int racksOffset = 0;
                    int slottable = Racks.Sum(rack =>
                    {
                        var anchor = new Vector3Int(x, y - racksOffset);

                        if (rack.IsOutsideBefore(anchor, shapeHeight)) return 0;
                        if (rack.IsOutsideAfter(anchor, shapeHeight))
                        {
                            racksOffset -= rack.Rows;
                            return 0;
                        }
                        return rack.Slotable(anchor, loot.InventoryShape, out List<Vector2Int> violators);
                    });

                    if (slottable == loot.InventoryShape.Length)
                    {
                        origin = new Vector3Int(x, y);
                        return true;
                    } else
                    {
                        Debug.Log($"{loot.Id}: ({y}, {x}) = {slottable} / {loot.InventoryShape.Length}");
                    }
                }
            }

            origin = Vector3Int.zero;
            return false;
        }

        HashSet<Lootable> Loots = new HashSet<Lootable>();

        string LootsSummary(IEnumerable<Lootable> loots)
        {
            var s = string.Join(", ", loots.Select(v => v.Id));
            if (string.IsNullOrEmpty(s)) return "-EMPTY-";
            return s;
        }

        private void Drop(Lootable lootable)
        {
            int offsetRows = 0;
            Racks.ForEach(rack => {
                rack.SetOccupancy(lootable.Coordinates, offsetRows, lootable.InventoryShape, false);
                offsetRows += rack.Rows;
            });
            Loots.Remove(lootable);
        }


        private void Pickup(Lootable lootable, Vector3Int placement)
        {
            int offsetRows = 0;
            Racks.ForEach(rack => {
                rack.SetOccupancy(placement, offsetRows, lootable.InventoryShape, true);
                offsetRows += rack.Rows;
            });
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
                    inventoryRack.BagIndex = racks;
                    args.Coordinates = new Vector3Int(0, racks);
                    args.DefinedPosition = true;
                    Racks.Add(inventoryRack);
                }
                else
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
                }
                else
                {
                    // Reset to previous position
                    args.Consumed = true;
                    args.Coordinates = loot.Coordinates;
                    placement = loot.Coordinates;
                    Drop(loot);
                    inventoryEvent = InventoryEvent.Move;
                }
            }
            else
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

        public bool RemoveOneCorruption(Vector3Int coordinates, System.Func<bool> effect, out int remaining)
        {
            int rackHeightsOffset = 0;
            for (int i = 0, l = Racks.Count; i < l; i++)
            {
                var rack = Racks[i];
                if (rack.ClearOneCorruption(coordinates + new Vector3Int(0, -rackHeightsOffset), effect, out remaining))
                {
                    return true;
                }
                rackHeightsOffset += rack.Rows;
            }

            remaining = -1;
            return false;
        }

        public bool Has(System.Func<Lootable, bool> predicate, out Lootable firstMatch)
        {
            firstMatch = Loots.Where(predicate).FirstOrDefault();
            return firstMatch != null;
        }

        private IEnumerable<T> GetLoot<T>() where T : Lootable => Loots
            .Where(loot => loot.GetType() == typeof(T))
            .Select(loot => (T)loot);

        private IEnumerable<Canister> GetXPCanisters() => GetLoot<Canister>()
            .Where(canister => canister.CanisterType == CurrencyType.XP);
        private IEnumerable<Canister> GetHealthCanisters() => GetLoot<Canister>()
            .Where(canister => canister.CanisterType == CurrencyType.XP);
        private IEnumerable<Canister> GetCanisters(CurrencyType type) => GetLoot<Canister>()
            .Where(canister => canister.CanisterType == type);


        public int XP { get => GetXPCanisters().Sum(canister => canister.Stored); }
        public int Health { get => GetHealthCanisters().Sum(canister => canister.Stored); }

        public int PlayerLevel { get => GetLoot<PlayerLevel>().Count(); }
        public int Repairs { get => GetLoot<Repair>().Count(); }

        public void InvokeCanisterChange(CurrencyType type)
        {
            var (stored, capacity) = GetCanisters(type)
                .Aggregate((0, 0), (acc, canister) => (acc.Item1 + canister.Stored, acc.Item2 + canister.Capacity));


            CurrencyTracker.Update(type, stored, capacity);
            
        }

        public void Receive(int amount, CurrencyType type)
        {
            var canisters = GetCanisters(type).ToArray();

            for (int i = 0; i < canisters.Length; i++)
            {
                var canister = canisters[i];

                if (canister.Receive(amount, out int remaining))
                {
                    amount = remaining;
                }
                else
                {
                    break;
                }
            }

            InvokeCanisterChange(type);
        }

        public void Withdraw(int amount, CurrencyType type)
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

            InvokeCanisterChange(type);
        }
    }
}