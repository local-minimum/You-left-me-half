using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DeCrawl.Primitives;
using DeCrawl.Systems;

namespace YLHalf
{
    public class LevelTracker : FindingSingleton<LevelTracker>
    {
        [SerializeField]
        bool allowRepeatLastLevelConditions = true;

        [SerializeField]
        int[] xpNeededForLevel;

        [SerializeField]
        int[] tokensPerLevel;

        [SerializeField]
        Inventory inventory;

        private void OnEnable()
        {
            CurrencyTracker.OnChange += CurrencyTracker_OnChange;
        }

        private void OnDisable()
        {
            CurrencyTracker.OnChange -= CurrencyTracker_OnChange;
        }

        private void CurrencyTracker_OnChange(CurrencyType type, int available, int capacity)
        {
            if (type != CurrencyType.XP) return;
            var levelIndex = Mathf.Max(0, inventory.PlayerLevel - 1);

            if (levelIndex >= xpNeededForLevel.Length && !allowRepeatLastLevelConditions) return;

            var refLevel = Mathf.Min(levelIndex, xpNeededForLevel.Length - 1);

            if (available < xpNeededForLevel[refLevel]) return;

            inventory.Withdraw(xpNeededForLevel[refLevel], CurrencyType.XP);

            CreateLevelLoot();


            var newTokens = tokensPerLevel[Mathf.Min(refLevel, tokensPerLevel.Length - 1)];

            for (int i = 0; i < newTokens; i++)
            {
                CreateRepairLoot();
            }
        }

        void CreateLevelLoot()
        {
            var level = LootTable.instance.First<PlayerLevel>();
            level.Loot(LootOwner.Player);
        }

        void CreateRepairLoot()
        {
            var repair = LootTable.instance.First<Repair>();
            repair.Loot(LootOwner.Player);
        }
    }
}