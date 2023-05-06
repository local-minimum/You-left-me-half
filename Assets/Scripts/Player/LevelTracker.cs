using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    [SerializeField]
    PlayerLevel levelPrefab;

    [SerializeField]
    Repair repairPrefab;

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
        // TODO: LootTable doesn't know about players loot at start
        var nextLevel = LootTable.instance.Count<PlayerLevel>() + 1;
        var level = Instantiate(levelPrefab);
        level.name = $"PlayerLevel {nextLevel}";
        level.Loot(LootOwner.Player);
    }

    void CreateRepairLoot()
    {
        // TODO: LootTable not usable
        var nextRepairId = LootTable.instance.Count<Repair>() + 1;
        var repair = Instantiate(repairPrefab);
        repair.name = $"InventoryRepair {nextRepairId}";
        repair.Loot(LootOwner.Player);
    }
}
