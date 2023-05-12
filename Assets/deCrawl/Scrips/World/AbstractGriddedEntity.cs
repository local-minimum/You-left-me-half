using UnityEngine;
using DeCrawl.Primitives;

namespace DeCrawl.World
{
    /// <summary>
    /// Utility class for things that exists on the level grid
    /// </summary>
    [ExecuteInEditMode]
    public abstract class AbstractGriddedEntity<Entity, ClaimCondition> : MonoBehaviour
    {
        [SerializeField]
        Vector3Int gridPosition;

        abstract protected ILevel<Entity, ClaimCondition> level { get; }

        private void Awake()
        {
            var movable = GetComponent<AbstractMovingEntity<Entity, ClaimCondition>>();
            if (movable)
            {
                movable.OnMove += GriddedEntity_OnMove;
            }
        }

        private void OnDestroy()
        {
            var movable = GetComponent<AbstractMovingEntity<Entity, ClaimCondition>>();
            if (movable)
            {
                movable.OnMove -= GriddedEntity_OnMove;
            }
        }

        private void GriddedEntity_OnMove(string id, Vector3Int position, CardinalDirection lookDirection)
        {
            gridPosition = position;
        }


        private void Update()
        {
            if (!Application.isPlaying)
            {
                transform.position = level.AsWorldPosition(gridPosition);
            }
        }
    }
}
