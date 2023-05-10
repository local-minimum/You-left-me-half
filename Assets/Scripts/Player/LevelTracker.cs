using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DeCrawl.Primitives;

public class LevelTracker : MonoBehaviour
{
    [SerializeField]
    bool allowRepeatLastLevelConditions = true;

    [SerializeField]
    int[] xpNeededForLevel;

    [SerializeField]
    int[] tokensPerLevel;

    [SerializeField]
    Inventory inventory;

    static LevelTracker instance { get; set; }

    private void Awake()
    {
        if (instance == null) { instance = this; }
        else if (instance != this) Destroy(this);
    }

    private void OnEnable()
    {
        Inventory.OnCanisterChange += Inventory_OnCanisterChange;
    }

    private void OnDisable()
    {
        Inventory.OnCanisterChange -= Inventory_OnCanisterChange;
    }

    private void OnDestroy()
    {
        if (instance == this) instance = null;
    }

    private void Inventory_OnCanisterChange(CanisterType type, int stored, int capacity)
    {
        if (type != CanisterType.XP) return;
        var levelIndex = Mathf.Max(0, inventory.PlayerLevel - 1);
                
        if (levelIndex >= xpNeededForLevel.Length && !allowRepeatLastLevelConditions) return;

        var refLevel = Mathf.Min(levelIndex, xpNeededForLevel.Length - 1);

        if (stored < xpNeededForLevel[refLevel]) return;
        
        inventory.Withdraw(xpNeededForLevel[refLevel], CanisterType.XP);
        
        CreateLevelLoot();


        var newTokens = tokensPerLevel[Mathf.Min(refLevel, tokensPerLevel.Length - 1)];

        for (int i = 0; i<newTokens; i++)
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
