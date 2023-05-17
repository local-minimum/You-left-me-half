using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DeCrawl.World;

namespace FP
{
    public class PlayerController : AbstractPlayerController<FPEntity, bool>
    {
        public override ILevel<FPEntity, bool> Level => NodeLevel.instance;

        protected override FPEntity PlayerEntity => FPEntity.Player;

        protected override bool ClaimCond => true;
    }
}