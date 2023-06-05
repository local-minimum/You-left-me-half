using System.Collections.Generic;
using UnityEngine;

namespace DeCrawl.World
{    public interface ILevel<Entity, ClaimCondition>: IGrid
    {
        public bool ClaimPosition(Entity claimer, Vector3Int position, ClaimCondition condition);
        public bool ClaimPositionForced(Entity claimer, Vector3Int position);

        public bool ReleasePosition(Entity owner, Vector3Int position);

        public bool FindPathToPlayerFrom(
            (int, int) origin,
            int maxDepth,
            System.Func<Entity, bool> permissablePredicate,
            out List<(int, int)> path
        );
    }

}
