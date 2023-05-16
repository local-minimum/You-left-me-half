using UnityEngine;

namespace DeCrawl.Primitives
{
    [System.Serializable]
    public class AttackStats : IAttackStats
    {
        [Range(1, 10)]
        public int attackRange = 1;
        public int minAttack = 4;
        public int maxAttack = 8;
        [Range(0, 1)]
        public float failChance = 0.05f;
        [Range(0, 1)]
        public float critChance = 0.1f;
        [Range(0, 1)]
        public float critFactor = 1.5f;
        [Range(0, 1)]
        public float failFactor = 0.5f;
        [Range(0, 2)]
        public float beforeCooldownSeconds = 0.5f;
        [Range(0, 10)]
        public float cooldownSeconds = 1f;

        public AttackMode Mode
        {
            get
            {
                var modeValue = Random.value;

                if (modeValue < critChance)
                {
                    return AttackMode.CritSuccess;
                }

                modeValue -= critChance;

                if (modeValue < failChance)
                {
                    if (Random.value < critChance)
                    {
                        return AttackMode.CritFail;
                    }
                    return AttackMode.Fail;
                }

                return AttackMode.Normal;
            }
        }

        public AttackMode GetAttack(out int amount)
        {
            amount = Random.Range(minAttack, maxAttack + 1);
            var mode = Mode;

            switch (mode)
            {
                case AttackMode.CritFail:
                case AttackMode.Fail:
                    amount = Mathf.FloorToInt(minAttack * failFactor);
                    break;
                case AttackMode.CritSuccess:
                    amount = Mathf.CeilToInt(maxAttack * critFactor);
                    break;
            }

            return mode;
        }
    }
}