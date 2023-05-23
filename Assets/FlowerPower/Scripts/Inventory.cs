using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DeCrawl.Primitives;
using DeCrawl.Lootables;

namespace FP
{
    public class Inventory : UnifiedInventory<SingleSpaceOnlyBag>
    {
        new protected bool HasConstraint(Lootable loot) => Has(l => l.Id == loot.Id);
        
        new protected bool CanPickupShape(Lootable loot, out Vector3Int origin)
        {
            if (loot is Canister)
            {
                origin = Vector3Int.zero;
                return true;
            }

            return base.CanPickupShape(loot, out origin);
        }
    }
}