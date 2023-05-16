using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using DeCrawl.Primitives;
using DeCrawl.Enemies;
using DeCrawl.Systems;
using DeCrawl.Lootables;

namespace YLHalf
{
    public class Inventory : UnifiedInventory<InventoryRack>
    {
        public static readonly int RackWidth = 8;
        public static readonly int RackHeight = 4;

        public static int RackLimit = 4;

        private void Start()
        {
            if (MaxBags > RackLimit)
            {
                Debug.LogError($"{name} claims too many rack slots");
            }
        }

        protected new void OnEnable()
        {
            base.OnEnable();

            PlayerController.OnPlayerMove += PlayerController_OnPlayerMove;
            BattleMaster.OnHitPlayer += BattleMaster_OnHitPlayer;
            EnemyBase.OnKillEnemy += Enemy_OnKillEnemy;
        }

        protected new void OnDisable()
        {
            base.OnDisable();

            PlayerController.OnPlayerMove -= PlayerController_OnPlayerMove;
            BattleMaster.OnHitPlayer -= BattleMaster_OnHitPlayer;
            EnemyBase.OnKillEnemy -= Enemy_OnKillEnemy;
        }

        private void Enemy_OnKillEnemy(GameObject enemy, int xpReward)
        {
            Receive(xpReward, CurrencyType.XP);
        }

        private void BattleMaster_OnHitPlayer(int amount)
        {
            Withdraw(amount, CurrencyType.Health);
        }

        private Vector3Int playerPosition;

        private void PlayerController_OnPlayerMove(Vector3Int position, CardinalDirection lookDirection)
        {
            playerPosition = position;
        }

        protected new bool HasConstraint(Lootable loot)
        {
            if (loot is Uplink && loot.Owner == LootOwner.Level)
            {
                if (Level.instance.GridBaseStatus(playerPosition.x, playerPosition.z) != GridEntity.InBound)
                {
                    Debug.Log("Tried to pick up uplink when not allowed");
                    return true;
                }
            }
            return false;
        }

        public bool RemoveOneCorruption(Vector3Int coordinates, System.Func<bool> effect, out int remaining)
        {
            int rackHeightsOffset = 0;
            for (int i = 0, l = Bags.Count; i < l; i++)
            {
                var rack = Bags[i];
                if (rack.ClearOneCorruption(coordinates + new Vector3Int(0, -rackHeightsOffset), effect, out remaining))
                {
                    return true;
                }
                rackHeightsOffset += rack.Rows;
            }

            remaining = -1;
            return false;
        }

        public int PlayerLevel { get => GetLoot<PlayerLevel>().Count(); }
        public int Repairs { get => GetLoot<Repair>().Count(); }
    }
}