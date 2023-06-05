using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using DeCrawl.Primitives;
using DeCrawl.Lootables;

namespace FP
{
    public class Inventory : UnifiedInventory<SingleSpaceOnlyBag>
    {        
        protected override bool CanPickupShape(Lootable loot, out Vector3Int origin)
        {
            if (loot is Canister)
            {
                origin = Vector3Int.zero;
                return true;
            }

            return base.CanPickupShape(loot, out origin);
        }

        protected override bool CanPickupShape(Lootable loot, Vector3Int origin)
        {
            if (loot is Canister)
            {
                return true;
            }

            return base.CanPickupShape(loot, origin);
        }

        public IEnumerable<T> FilterHas<T>(
            System.Func<Lootable, T, bool> predicate,
            IEnumerable<T> options
        ) => options.Where(value => Loots.Any(loot => predicate(loot, value)));

        public IEnumerable<Lootable> Where(
            System.Func<Lootable, bool> predicate
        ) => Loots.Where(predicate);
    }
}