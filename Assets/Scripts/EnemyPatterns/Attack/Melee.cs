using System.Collections.Generic;
using UnityEngine;
using DeCrawl.Utils;
using DeCrawl.Primitives;

namespace YLHalf
{
    public class Melee : EnemyPattern
    {
        public int reach = 1;
        public bool mayAttackVirtualSpace = true;
        public AttackStats attack;

        public override bool Eligible
        {
            get
            {
                var offset = Level.instance.PlayerPosition - movable.Position;
                if (offset.ManhattanMagnitude() > reach) return false;
                if (offset.AsDirection() != movable.LookDirection) return false;
                if (reach == 1) return true;

                return LineSearch.Search(
                    new LineSearch.SearchParameters(
                        movable.Position.XZTuple(),
                        Level.instance.PlayerPosition,
                        (coords) =>
                        {
                            if (coords == movable.Position.XZTuple() || coords == Level.instance.PlayerPosition.XZTuple()) return true;
                            switch (Level.instance.GridBaseStatus(coords))
                            {
                                case GridEntity.InBound:
                                    return true;
                                case GridEntity.VirtualSpace:
                                    return mayAttackVirtualSpace;
                                default:
                                    return false;
                            }
                        }
                    ),
                    out List<(int, int)> _
                );
            }
        }

        private void Update()
        {
            if (!playing || easing) return;

            StartCoroutine(Attack());
        }

        IEnumerator<WaitForSeconds> Attack()
        {
            easing = true;
            var playerPosition = Level.instance.PlayerPosition;
            yield return new WaitForSeconds(attack.beforeCooldownSeconds);
            if (playerPosition == Level.instance.PlayerPosition)
            {
                enemy.AttackPlayer(attack);
            }
            yield return new WaitForSeconds(attack.cooldownSeconds);
            easing = false;
            Abort();
        }
    }
}