using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DeCrawl.Primitives;
using DeCrawl.World;
using UnityEngine;
using System;
using System.Linq;

namespace FP {
    public enum FPEntity { Nothing, Enemy, Player }

    public class NodeLevel : FindingSingleton<NodeLevel>, ILevel<FPEntity, bool>
    {
        Dictionary<Vector3Int, LevelNode> lookup = new Dictionary<Vector3Int, LevelNode>();
        List<LevelNode> nodes = new List<LevelNode>();
        bool nodesInited = false;

        List<LevelNode> Nodes
        {
            get {
                if (!nodesInited)
                {
                    nodes.AddRange(GetComponentsInChildren<LevelNode>());
                    foreach (var node in nodes)
                    {
                        var c = node.Coordinates;
                        if (lookup.ContainsKey(c))
                        {
                            Debug.LogError($"{node.transform.position} shares same coordinates as {lookup[c].transform.position} namely {c}");
                        } else
                        {
                            lookup.Add(c, node);
                        }
                    }
                    nodesInited = true;
                }
                return nodes;
            }

        }

        LevelNode PlayerSpawn => Nodes.First(node => node.playerSpawn);                       
        
        public CardinalDirection PlayerFirstSpawnDirection => PlayerSpawn.PlayerSpawnDirection;

        public Vector3Int PlayerFirstSpawnPosition => PlayerSpawn.Coordinates;

        public Vector3Int AsGridPosition(Vector3 worldPosition)
        {           
            var scaled = worldPosition / LevelGridSize.Size;
            return new Vector3Int(Mathf.FloorToInt(scaled.x), Mathf.FloorToInt(scaled.y), Mathf.FloorToInt(scaled.z));

        }

        [SerializeField]
        Vector3 worldPostionOffset;

        public Vector3 AsWorldPosition(Vector3Int gridPosition) => (gridPosition + worldPostionOffset) * LevelGridSize.Size;

        LevelNode GetNode(Vector3Int coordinates) => Nodes.FirstOrDefault(node => node.Coordinates == coordinates);

        public bool ClaimPosition(FPEntity claimer, Vector3Int position, bool condition)
        {
            var node = GetNode(position);

            if (node == null) return false;

            if (claimer == FPEntity.Player && !node.CanBeReachedFrom(playerPosition))
            {
                Debug.Log($"Player can't react {node.Coordinates} from {playerPosition}");
                return false;
            }

            try
            {
                node.AddOccupant(claimer);
            } catch (System.ArgumentException)
            {
                return false;
            }

            return true;
        }

        public bool ClaimPositionForced(FPEntity claimer, Vector3Int position)
        {
            var node = GetNode(position);
            if (node == null) return false;
            if (node.Occupant != claimer)
            {
                node.ClearOccupants();
            }
            node.AddOccupant(claimer);
            return true;
        }

        public bool FindPathToPlayerFrom((int, int) origin, int maxDepth, Func<FPEntity, bool> permissablePredicate, out List<(int, int)> path)
        {
            throw new NotImplementedException();
        }

        public bool ReleasePosition(FPEntity owner, Vector3Int position) => GetNode(position)?.RemoveOccupant(owner) ?? false;

        private void OnEnable()
        {
            PlayerController.OnPlayerMove += PlayerController_OnPlayerMove;
        }

        private void OnDisable()
        {
            PlayerController.OnPlayerMove -= PlayerController_OnPlayerMove;
        }

        Vector3Int playerPosition;
        private void PlayerController_OnPlayerMove(Vector3Int position, CardinalDirection lookDirection)
        {
            // Debug.Log($"Got player position {position}");
            playerPosition = position;
        }

        public void ResetState()
        {
            Nodes.ForEach(node =>
            {
                node.ClearOccupants();
            });
        }
    }
}
