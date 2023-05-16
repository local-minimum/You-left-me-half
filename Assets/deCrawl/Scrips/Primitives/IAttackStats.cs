using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DeCrawl.Primitives
{
    public interface IAttackStats
    {
        public AttackMode GetAttack(out int amount);
    }
}