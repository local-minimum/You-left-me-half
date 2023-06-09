using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DeCrawl.Primitives;
using DeCrawl.World;
using DeCrawl.Utils;

namespace DeCrawl.Enemies.PatternEnemy
{
    public abstract class AbstractPatternEnemy<Entity, ClaimCondition> : EnemyBase
    {
        #region Awareness
        [SerializeField]
        int AreaAwareness = 1;

        [SerializeField]
        int LOSAwareness = 5;

        int GetAwareness(SightMode mode)
        {
            switch (mode)
            {
                case SightMode.LOS:
                    return LOSAwareness;
                case SightMode.Area:
                    return AreaAwareness;
                default:
                    return Mathf.Max(LOSAwareness, AreaAwareness);
            }

        }
        #endregion

        #region Patterns
        [SerializeField]
        AbstractEnemyPattern<Entity, ClaimCondition>[] DefaultPatterns;

        [SerializeField]
        AbstractEnemyPattern<Entity, ClaimCondition>[] AttackPatterns;

        AbstractEnemyPattern<Entity, ClaimCondition> RandomPattern
        {
            get
            {
                var options = AttackPatterns.Where(p => p.Eligible).ToArray();
                if (options.Length > 0)
                {
                    return options[Random.Range(0, options.Length)];
                }
                options = DefaultPatterns.Where(p => p.Eligible).ToArray();
                if (options.Length == 0)
                {
                    return null;
                }
                return options[Random.Range(0, options.Length)];
            }
        }

        protected AbstractEnemyPattern<Entity, ClaimCondition> activePattern;
        #endregion

        #region Navigation
        public readonly NullableItem<Vector3Int> LastRegisteredBasicPosition = new NullableItem<Vector3Int>();

        bool DirectionTo((int, int) coords, out CardinalDirection direction)
        {
            var offset = new Vector3Int(coords.Item1, 0, coords.Item2) - movable.Position;

            if ((offset.x == 0) == (offset.z == 0))
            {
                direction = CardinalDirection.Down;
                return false;
            }

            direction = offset.AsDirection();
            return true;
        }

        abstract protected bool PermissableSearchPosition(Entity entity);

        public bool SeesPlayer(SightMode mode, out List<(int, int)> path)
        {
            if (!level.FindPathToPlayerFrom(
                movable.Position.XZTuple(),
                GetAwareness(mode),
                (entity) => PermissableSearchPosition(entity),
                out path
            ))
            {
                if (path != null)
                {
                    path.RemoveAt(0);
                }
                return false;
            }

            path.RemoveAt(0);

            var isLOS = (path.GroupBy(coords => coords.Item1).Count() == 1 || path.GroupBy(coords => coords.Item2).Count() == 1) &&
                path.All(coords => {
                    if (DirectionTo(coords, out CardinalDirection dir))
                    {
                        return dir == movable.LookDirection;
                    }
                    return false;
                });

            if (mode == SightMode.Any)
            {
                if (isLOS)
                {
                    return path.Count() <= LOSAwareness;
                }

                return path.Count() <= AreaAwareness;
            }
            else if (mode == SightMode.Area)
            {
                return path.Count() <= AreaAwareness;
            }
            return isLOS && path.Count() <= LOSAwareness;
        }
        #endregion


        abstract public ClaimCondition claimCondition { get; }

        abstract protected ILevel<Entity, ClaimCondition> level { get; }

        protected AbstractMovingEntity<Entity, ClaimCondition> movable;

        private void Start()
        {
            movable = GetComponent<AbstractMovingEntity<Entity, ClaimCondition>>();
            movable.SetNewGridPosition(level.AsGridPosition(transform.position), transform.forward.AsDirection());
            activePattern = RandomPattern;
            if (!(activePattern?.Play() ?? false))
            {
                Debug.LogWarning("Failed to launch enemy pattern");
            }
        }

        private void Update()
        {
            if (activePattern == null || !activePattern.Playing)
            {
                activePattern = RandomPattern;
                activePattern?.Play();
            }
        }
    }
}