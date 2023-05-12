using System.Collections.Generic;
using UnityEngine;
using DeCrawl.Primitives;

namespace DeCrawl.World
{    public interface ILevel<Entity, ClaimCondition>
    {
        public bool ClaimPosition(Entity claimer, Vector3Int position, ClaimCondition condition);

        public Vector3Int AsWorldPosition(Vector3Int gridPosition);
        public Vector3Int AsGridPosition(Vector3 worldPosition);

        public bool ReleasePosition(Entity owner, Vector3Int position);

        public CardinalDirection PlayerFirstSpawnDirection { get; }
        public Vector3Int PlayerFirstSpawnPosition { get; }

        public bool FindPathToPlayerFrom(
            (int, int) origin,
            int maxDepth,
            System.Func<Entity, bool> permissablePredicate,
            out List<(int, int)> path
        );
    }

}
