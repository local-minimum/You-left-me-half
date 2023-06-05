using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using DeCrawl.Utils;
using DeCrawl.Primitives;
using DeCrawl.Enemies;
using DeCrawl.World;

namespace DeCrawl.Systems
{
    public class PositionRecorder : FindingSingleton<PositionRecorder>, StateSaver
    {
        [System.Serializable]
        private struct StateDto
        {
            public PositionRecordDto[] records;

            public StateDto(PositionRecordDto[] records)
            {
                this.records = records;
            }
        }


        [System.Serializable]
        private struct PositionRecordDto
        {
            public string id;
            public Vector3Int position;
            public CardinalDirection lookDirection;
            public bool alive;

            public PositionRecordDto(string id, Vector3Int position, CardinalDirection lookDirection, bool alive)
            {
                this.id = id;
                this.position = position;
                this.lookDirection = lookDirection;
                this.alive = alive;
            }
        }

        Dictionary<string, Vector3Int> positions = new Dictionary<string, Vector3Int>();
        Dictionary<string, CardinalDirection> lookDirections = new Dictionary<string, CardinalDirection>();
        Dictionary<string, bool> aliveStatus = new Dictionary<string, bool>();
        bool listening = true;
 
        private new void Awake()
        {
            base.Awake();

            if (instance == this)
            {
                foreach (var entity in InterfaceFinder.FindMonoBehavioursWithIMovingEntity())
                {
                    entity.OnMove += PositionRecorder_OnMove;
                }

                EnemyBase.OnKillEnemy += EnemyBase_OnKillEnemy;
            }
        }

        private void EnemyBase_OnKillEnemy(GameObject enemy, int xpReward)
        {
            var entity = enemy.GetComponent<IMovingEntity>();
            if (entity != null)
            {
                aliveStatus[entity.Id] = false;
            }
        }

        public string[] GetEntities(Vector3Int position) => positions
            .Where(kvp => kvp.Value == position)
            .Select(kvp => kvp.Key)
            .ToArray();


        private new void OnDestroy()
        {
            foreach (var entity in InterfaceFinder.FindMonoBehavioursWithIMovingEntity())
            {
                entity.OnMove -= PositionRecorder_OnMove;
            }

            EnemyBase.OnKillEnemy -= EnemyBase_OnKillEnemy;
            base.OnDestroy();
        }

        private void PositionRecorder_OnMove(string id, Vector3Int position, CardinalDirection lookDirection)
        {
            if (!listening) return;

            positions[id] = position;
            lookDirections[id] = lookDirection;
            aliveStatus[id] = true;
        }

        public void ResetStored()
        {
            positions.Clear();
            lookDirections.Clear();
            aliveStatus.Clear();
        }

        public string SerializeState()
        {
            var records = positions.Keys
                .Select(id => new PositionRecordDto(id, positions[id], lookDirections[id], aliveStatus[id]))
                .ToArray();

            return JsonUtility.ToJson(new StateDto(records));
        }

        public void DeserializeState(string json)
        {
            var records = JsonUtility.FromJson<StateDto>(json).records;
            for (int i = 0; i < records.Length; i++)
            {
                var state = records[i];
                positions[state.id] = state.position;
                lookDirections[state.id] = state.lookDirection;
                aliveStatus[state.id] = state.alive;
            }

            RestorePositions();
        }

        private void RestorePositions()
        {
            listening = false;

            InterfaceFinder.FindMonoBehaviourWithIGrid()?.ResetState();

            foreach (var entity in InterfaceFinder.FindMonoBehavioursWithIMovingEntity())
            {
                var id = entity.Id;
                if (!aliveStatus.GetValueOrDefault(id, false))
                {
                    Debug.Log($"Removing Entity: {id} because it is not alive");
                    entity.RemoveFromGame();                    
                }
                else if (positions.ContainsKey(id))
                {
                    Debug.Log($"Restoring Entity: {id} to position {positions[id]}");
                    entity.SetNewGridPosition(positions[id], lookDirections[id]);
                    entity.ClaimPosition();
                } else
                {
                    Debug.Log($"Defaulting Entity: {id} not recorded in save");
                }
            }

            listening = true;
        }
    }
}