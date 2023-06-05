using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DeCrawl.Utils;
using DeCrawl.Lootables;
using DeCrawl.Systems;

namespace DeCrawl.Primitives
{
    public delegate void InventoryChange(Lootable loot, InventoryEvent inventoryEvent, Vector3Int placement);

    public class UnifiedInventory<BagType> : MonoBehaviour, ICurrencyPurse where BagType : IInventoryBag
    {
        public static event InventoryChange OnInventoryChange;

        public List<BagType> Bags = new List<BagType>();
        public int NumberOfBags => Bags.Count;
        public int MaxBags = 2;

        protected HashSet<Lootable> Loots = new HashSet<Lootable>();

        public string LootsSummary(IEnumerable<Lootable> loots)
        {
            var s = string.Join(", ", loots.Select(v => v.Id));
            if (string.IsNullOrEmpty(s)) return "-EMPTY-";
            return s;
        }

        protected void Drop(Lootable lootable)
        {
            int offsetRows = 0;
            Bags.ForEach(rack => {
                rack.SetOccupancy(lootable.Coordinates, offsetRows, lootable.InventoryShape, false);
                offsetRows += rack.Rows;
            });
            Loots.Remove(lootable);
            Debug.Log($"Dropped up: {lootable.Id}");

            if (Bags.Contains(lootable.GetComponent<BagType>()))
            {
                Bags.Remove(lootable.GetComponent<BagType>());
            }

            if (lootable is Canister)
            {
                InvokeCurrencyChange(((Canister)lootable).CanisterType);
            }
        }

        protected void Pickup(Lootable lootable, Vector3Int placement)
        {
            var newLoot = !Has(loot => loot.Id == lootable.Id);

            int offsetRows = 0;
            Bags.ForEach(rack => {
                rack.SetOccupancy(placement, offsetRows, lootable.InventoryShape, true);
                offsetRows += rack.Rows;
            });

            Loots.Add(lootable);
            Debug.Log($"Picked up: {lootable.Id}");

            if (newLoot && lootable is Canister)
            {
                InvokeCurrencyChange(((Canister)lootable).CanisterType);
            }

        }

        public int MaxWidth => Bags.Max(bag => bag.Columns);
        public int MaxRowForShape(int shapeHeight) => Bags.Sum(bag => bag.Rows) - shapeHeight;        

        /// <summary>
        /// Pick up loot. This assumes that the loot wasn't in the inventory before.
        /// </summary>
        /// <param name="loot"></param>
        /// <param name="origin">First free position where it can be picked up to</param>
        /// <returns></returns>
        protected virtual bool CanPickupShape(Lootable loot, out Vector3Int origin)
        {
            var shape = loot.InventoryShape;
            int shapeHeight = shape.Max(offset => offset.y) + 1;
            var maxY = MaxRowForShape(shapeHeight);
            var inventorySlots = loot.InventorySlots();
            var maxX = MaxWidth;

            for (var y = 0; y <= maxY; y++)
            {
                for (var x = 0; x < maxX; x++)
                {
                    int racksOffset = 0;
                    int slottable = Bags.Sum(rack =>
                    {
                        var anchor = new Vector3Int(x, y - racksOffset);

                        if (rack.IsOutsideBefore(anchor, shapeHeight)) return 0;
                        if (rack.IsOutsideAfter(anchor))
                        {
                            racksOffset -= rack.Rows;
                            return 0;
                        }
                        
                        return rack.Slotable(anchor, loot.InventoryShape, out List<Vector2Int> violators);
                    });

                    if (slottable == loot.InventorySize)
                    {
                        origin = new Vector3Int(x, y);
                        return true;
                    }
                }
            }

            origin = Vector3Int.zero;
            return false;
        }

        /// <summary>
        /// Pick up (or move) to a specific location
        /// </summary>
        /// <param name="loot"></param>
        /// <param name="origin"></param>
        /// <returns></returns>
        protected virtual bool CanPickupShape(Lootable loot, Vector3Int origin)
        {
            if (origin.y < 0) return false;

            int shapeHeight = loot.InventoryShape.Max(offset => offset.y) + 1;
            var inventorySlots = loot.InventorySlots();
            var originXY = origin.XYVector2Int();

            if (MaxRowForShape(shapeHeight) < origin.y)
            {
                Debug.Log($"Shape too heigh to have origin row as {origin.y} ({shapeHeight})");
                return false;
            }

            var localOrigin = new Vector3Int(origin.x, origin.y, origin.z);

            return Bags.Sum(rack =>
            {
                // Not in the rack
                if (rack.IsOutsideBefore(localOrigin, shapeHeight)) return 0;
                if (rack.IsOutsideAfter(localOrigin))
                {
                    localOrigin.y -= rack.Rows;
                    return 0;
                }

                // Fully or partially in rack
                var slots = rack.Slotable(localOrigin, loot.InventoryShape, out List<Vector2Int> violators);
                if (slots != loot.InventorySize)
                {
                    // If all violators are just our old position we count them as slotable positions
                    if (violators.All(violator => inventorySlots.Contains(violator + originXY)))
                    {
                        return slots + violators.Count();
                    }

                    return 0;
                }
                return slots;
            }) == loot.InventorySize;
        }

        protected bool HasConstraint(Lootable loot) => false;

        private void Lootable_OnLoot(Lootable loot, LootEventArgs args)
        {
            if (Loots.Contains(loot) && args.Owner != LootOwner.Player)
            {
                Debug.Log($"Dropping {loot.Id}");
                Drop(loot);
                OnInventoryChange?.Invoke(loot, InventoryEvent.Drop, Vector3Int.zero);
            }

            if (args.Owner != LootOwner.Player || args.Consumed) return;
            if (HasConstraint(loot))
            {
                Debug.Log($"Refuse pick up of {loot.Id} due to constraint");
                return;
            }

            InventoryEvent inventoryEvent = InventoryEvent.PickUp;
            var newBag = loot.GetComponent<BagType>();
            var placement = args.Coordinates;

            if (newBag != null)
            {
                // TODO: This could possibly go wrong if save/load isn't in order...
                var racks = NumberOfBags;
                if (racks < MaxBags)
                {
                    args.Coordinates = new Vector3Int(0, racks);
                    args.DefinedPosition = true;
                    Bags.Add(newBag);
                }
                else
                {
                    Debug.Log($"Refused pickup {loot.Id} due because no more bags allowed");
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
                else if (loot.Owner == LootOwner.Player)
                {
                    // Reset to previous position
                    args.Consumed = true;
                    args.Coordinates = loot.Coordinates;
                    placement = loot.Coordinates;
                    Drop(loot);
                    inventoryEvent = InventoryEvent.Move;
                } else
                {
                    Debug.LogError($"Cannot pick up {loot.Id} at {args.Coordinates}");
                    return;
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
                    Debug.LogWarning($"No space in inventory for {loot.Id}");
                    return;
                }
            }

            loot.transform.SetParent(transform);
            Pickup(loot, placement);
            args.Consumed = true;

            OnInventoryChange?.Invoke(loot, inventoryEvent, placement);           
        }

        public bool Has(System.Func<Lootable, bool> predicate) => Loots.Any(predicate);

        public bool Has(System.Func<Lootable, bool> predicate, out Lootable firstMatch)
        {
            firstMatch = Loots.Where(predicate).FirstOrDefault();
            return firstMatch != null;
        }

        protected IEnumerable<T> GetLoot<T>() where T : Lootable => Loots
            .Where(loot => loot is T)
            .Select(loot => (T)loot);

        protected void OnEnable()
        {
            Lootable.OnLoot += Lootable_OnLoot;
        }

        protected void OnDisable()
        {
            Lootable.OnLoot -= Lootable_OnLoot;
        }

        #region Currency Purse
        protected IEnumerable<Canister> GetCanisters(CurrencyType type) => GetLoot<Canister>()
            .Where(canister => canister.CanisterType == type);

        public int XP { get => GetCanisters(CurrencyType.XP).Sum(canister => canister.Stored); }
        public int Health { get => GetCanisters(CurrencyType.Health).Sum(canister => canister.Stored); }
        public int Money { get => GetCanisters(CurrencyType.Money).Sum(canister => canister.Stored); }

        public void SetCurrencyHeld(CurrencyType type, int amount, int capacity)
        {
            // capacity is ignored since it's handled by canister in this implementation
            foreach (var canister in GetCanisters(type))
            {
                canister.Empty();
                if (amount > 0)
                {
                    canister.Receive(amount, out amount);
                }
            }
            if (amount > 0)
            {
                Debug.LogError($"{amount} {type} could not be stored in inventory, out of capacity");
            }
        }

        public void InvokeCurrencyChange(CurrencyType type)
        {
            var (stored, capacity) = GetCanisters(type)
                .Aggregate((0, 0), (acc, canister) => (acc.Item1 + canister.Stored, acc.Item2 + canister.Capacity));


            Debug.Log($"{type} set to {stored} / {capacity}");
            CurrencyTracker.Update(type, stored, capacity);
        }

        public int Receive(int amount, CurrencyType type)
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

            InvokeCurrencyChange(type);

            return amount;
        }

        public int Withdraw(int amount, CurrencyType type)
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

            InvokeCurrencyChange(type);
            return amount;
        }
    }
    #endregion
}
