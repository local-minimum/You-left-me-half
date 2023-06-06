using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DeCrawl.Systems;

namespace FP
{
    public class InventoryStatusAsMetadata : MonoBehaviour
    {
        Inventory inventory;

        private void OnEnable()
        {
            if (inventory == null) inventory = GetComponent<Inventory>();            

            Inventory.OnInventoryChange += Inventory_OnInventoryChange;
            SyncMetadata();
        }

        void SyncMetadata()
        {
            var health = inventory.Health;
            var healthCapacity = inventory.Capacity(CurrencyType.Health);
            var letters = inventory.Where(l => l is LetterLoot).Select(ll => ((LetterLoot)ll).Letter).OrderBy(ch => ch).ToArray();
            var lettersString = string.Join("", letters);
            if (string.IsNullOrEmpty(lettersString))
            {
                lettersString = "-Empty inventory-";
            }
            MetadataRecorder.instance.AuxInfo = $"H: {health}/{healthCapacity} {lettersString}";
        }

        private void Inventory_OnInventoryChange(DeCrawl.Primitives.Lootable loot, DeCrawl.Primitives.InventoryEvent inventoryEvent, Vector3Int placement)
        {
            SyncMetadata();
        }
    }
}
