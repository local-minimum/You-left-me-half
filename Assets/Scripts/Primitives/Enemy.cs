using System.Collections.Generic;
using UnityEngine;
using DeCrawl.Enemies.PatternEnemy;
using DeCrawl.World;

public class Enemy : AbstractPatternEnemy<GridEntity, bool>
{
    // Move to implementation of abstract thing
    [SerializeField]    
    public bool AllowVirtualSpace = true;

    public override bool claimCondition => AllowVirtualSpace;
    protected override ILevel<GridEntity, bool> level => Level.instance;

    protected override bool PermissableSearchPosition(GridEntity entity) => entity.BaseTypeIsInbound(claimCondition);

    private void OnEnable()
    {
        MasterOfEndings.OnEnding += MasterOfEndings_OnEnding;
        BattleMaster.OnHitMonster += BattleMaster_OnHitMonster;
    }

    private void OnDisable()
    {
        MasterOfEndings.OnEnding -= MasterOfEndings_OnEnding;
        BattleMaster.OnHitMonster -= BattleMaster_OnHitMonster;
    }

    private void BattleMaster_OnHitMonster(string monsterId, int amount)
    {
        if (monsterId != movable.Id) return;

        health -= amount;

        if (health <= 0)
        {
            Kill();

            Level.instance.ReleasePosition(GridEntity.Other, movable.Position);
        }
    }

    private void MasterOfEndings_OnEnding(EndingType type, Ending ending)
    {
        if (type == EndingType.Death)
        {
            if (activePattern)
            {
                activePattern.enabled = false;
            }
            enabled = false;
        }
    }
}
