using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DeCrawl.Systems;
using DeCrawl.Primitives;

namespace FP
{
    public class LetterLootTable : LootTable
    {
        [SerializeField]
        LetterLoot LootPrefab;

        public override Lootable CreateLootById(string id)
        {
            var ch = LetterLoot.AsChar(id);
            if (ch != LetterLoot.NoLetterChar) {
                var loot = Instantiate(LootPrefab);
                loot.name = LetterLoot.AsId(ch);
                return loot;
            }

            Debug.LogError($"Attempted to create loot {id} but don't know how");
            return null;
        }
    }
}