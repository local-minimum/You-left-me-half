using UnityEngine;
using DeCrawl.Primitives;

namespace DeCrawl.World
{
    public delegate void MoveEvent(string id, Vector3Int position, CardinalDirection lookDirection);

    public interface IMovingEntity
    {        
        public string Id { get; }
        public Vector3Int Position { get;  }
        public CardinalDirection LookDirection { get;  }

        public void SetNewGridPosition(Vector3Int position, CardinalDirection lookDirection);

        public event MoveEvent OnMove;
    }
}
