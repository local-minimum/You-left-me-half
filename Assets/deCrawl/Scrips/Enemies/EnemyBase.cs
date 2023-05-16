using System.Collections.Generic;
using UnityEngine;
using DeCrawl.Primitives;

namespace DeCrawl.Enemies
{
    public delegate void AttackPlayer(AttackMode mode, int amount);
    public delegate void KillEnemy(GameObject enemy, int xpReward);

    public abstract class EnemyBase : MonoBehaviour
    {
        [SerializeField]
        protected int health = 10;

        [SerializeField]
        protected int xpReward = 16;

        public enum SightMode { Any, LOS, Area };

        public static event AttackPlayer OnAttackPlayer;
        public static event KillEnemy OnKillEnemy;

        protected void Kill()
        {
            OnKillEnemy?.Invoke(gameObject, xpReward);

            Destroy(gameObject);
        }

        public void AttackPlayer(IAttackStats attack)
        {
            var mode = attack.GetAttack(out int amount);
            OnAttackPlayer?.Invoke(mode, amount);
        }

    }
}
