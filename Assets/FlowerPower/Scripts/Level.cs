using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DeCrawl.Primitives;
using DeCrawl.World;
using System;
using System.Linq;

namespace FP {
    public enum FPEntity { Nothing, Enemy, Player }

    public class Level : FindingSingleton<Level>, ILevel<FPEntity, bool>
    {
        List<LevelNode> nodes = new List<LevelNode>();

        LevelNode PlayerSpawn => nodes.First(node => node.playerSpawn);
        public CardinalDirection PlayerFirstSpawnDirection => PlayerSpawn.PlayerSpawnDirection;

        public Vector3Int PlayerFirstSpawnPosition => PlayerSpawn.Coordinates;

        public Vector3Int AsGridPosition(Vector3 worldPosition)
        {           
            var scaled = worldPosition / LevelGridSize.Size;
            return new Vector3Int(Mathf.RoundToInt(scaled.x), Mathf.RoundToInt(scaled.y), Mathf.RoundToInt(scaled.z));

        }

        public Vector3Int AsWorldPosition(Vector3Int gridPosition) => gridPosition * LevelGridSize.Size;

        LevelNode GetNode(Vector3Int coordinates) => nodes.FirstOrDefault(node => node.Coordinates == coordinates);

        public bool ClaimPosition(FPEntity claimer, Vector3Int position, bool condition)
        {
            var node = GetNode(position);
            if (node == null) return false;

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
    }
}
