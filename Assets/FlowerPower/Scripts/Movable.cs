using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DeCrawl.World;

namespace FP
{
    public class Movable : AbstractMovingEntity<FPEntity, bool>
    {
        public override ILevel<FPEntity, bool> Level => NodeLevel.instance;

        public FPEntity entity = FPEntity.Player;

        public override void ClaimPosition()
        {
            Level.ClaimPosition(entity, Position, true);
        }
    }
}