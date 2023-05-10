using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DeCrawl.World;

public class PlayerController : AbstractPlayerController<GridEntity, bool>
{
    protected override GridEntity PlayerEntity => GridEntity.Player;
    protected override bool ClaimCond  => !inventory.Has(loot => loot.GetType() == typeof(Uplink), out Lootable loot);

    private ILevel<GridEntity, bool> level;
    public override ILevel<GridEntity, bool> Level
    {
        get
        {
            if (level != null) return level;
            level = FindObjectOfType<Level>();
            return level;
        }
    }

    private Inventory inventory;

    private new void Start()
    {
        inventory = GetComponentInChildren<Inventory>();
        base.Start();
    }
}
