using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DeCrawl.Primitives;

namespace DeCrawl.World
{
    public interface IGrid
    {
        public Vector3 AsWorldPosition(Vector3Int gridPosition);
        public Vector3Int AsGridPosition(Vector3 worldPosition);
        public void ResetState();

        public CardinalDirection PlayerFirstSpawnDirection { get; }
        public Vector3Int PlayerFirstSpawnPosition { get; }

    }
}
