using UnityEngine;
using DeCrawl.Primitives;

namespace DeCrawl.World
{
    public struct NavInstructions
    {
        public bool enabled;
        public bool failed;
        public System.Action<float> Interpolate;
        public System.Action OnDone;
        public float duration;
        public Vector3Int targetPosition;
        public CardinalDirection targetLookDirection;

        public NavInstructions(
            System.Action<float> interpolate,
            System.Action onDone,
            float duration,
            Vector3Int targetPosition,
            CardinalDirection targetLookDirection
        )
        {
            enabled = true;
            failed = false;
            Interpolate = interpolate;
            OnDone = onDone;
            this.duration = duration;
            this.targetPosition = targetPosition;
            this.targetLookDirection = targetLookDirection;
        }

        public static NavInstructions NoNavigation
        {
            get
            {
                var intructions = new NavInstructions();
                intructions.enabled = false;
                return intructions;
            }
        }

        public static NavInstructions FailedMove
        {
            get
            {
                var intructions = new NavInstructions();
                intructions.enabled = false;
                intructions.failed = true;
                return intructions;
            }
        }

    }


    public delegate void MoveEvent(string id, Vector3Int position, CardinalDirection lookDirection);

    /// <summary>
    /// Component for all things moving about on a level grid
    /// </summary>
    public abstract class AbstractMovingEntity<Entity, ClaimCondition> : IdentifiableEntity
    {
        abstract public ILevel<Entity, ClaimCondition> Level { get; }

        public event MoveEvent OnMove;

        public Vector3Int Position { get; private set; }
        public CardinalDirection LookDirection { get; private set; }

        public void SetNewGridPosition(Vector3Int position, CardinalDirection lookDirection)
        {
            Position = position;
            LookDirection = lookDirection;
            OnMove?.Invoke(Id, position, lookDirection);
        }

        Vector3Int GetNavigationTraget(Navigation navigation)
        {
            switch (navigation)
            {
                case Navigation.Forward:
                    return Position + LookDirection.AsIntVector();
                case Navigation.Backward:
                    return Position + LookDirection.Invert().AsIntVector();
                case Navigation.Left:
                    return Position + LookDirection.RotateCCW().AsIntVector();
                case Navigation.Right:
                    return Position + LookDirection.RotateCW().AsIntVector();
                default:
                    return Position;
            }
        }

        /// <summary>
        /// Query level if position can be reserved / entered by player
        /// </summary>
        /// <param name="navigation">Relative navigation</param>
        /// <returns></returns>
        bool ClaimSpot(Entity entity, Navigation navigation, Vector3Int target, ClaimCondition allowEnterVirtualSpaces)
        {
            if (navigation.Translates())
            {
                if (Level.ClaimPosition(entity, target, allowEnterVirtualSpaces))
                {
                    return true;
                }
                return false;
            }
            return true;
        }

        static readonly bool InstaReleaseOnClaim = true;

        public NavInstructions Navigate(
            Entity entity,
            Navigation nav,
            float moveTime,
            float turnTime,
            System.Action<Vector3Int, CardinalDirection> onComplete,
            ClaimCondition allowEnterVirtualSpaces
        )
        {
            if (nav.Translates())
            {
                Vector3 origin = transform.position;
                Vector3Int gridTarget = GetNavigationTraget(nav);

                if (ClaimSpot(entity, nav, gridTarget, allowEnterVirtualSpaces))
                {
                    if (InstaReleaseOnClaim)
                    {
                        Level.ReleasePosition(entity, Position);
                    }

                    Vector3 target = Level.AsWorldPosition(gridTarget);

                    System.Action<float> interpolate = (float progress) => { transform.position = Vector3.Lerp(origin, target, progress); };
                    System.Action onDone = () =>
                    {
                        if (!InstaReleaseOnClaim)
                        {
                            Level.ReleasePosition(entity, Position);
                        }
                        SetNewGridPosition(gridTarget, LookDirection);
                        if (onComplete != null) onComplete(gridTarget, LookDirection);

                    };
                    return new NavInstructions(interpolate, onDone, moveTime, gridTarget, LookDirection);
                }
                else
                {
                    return NavInstructions.FailedMove;
                }

            }
            else if (nav.Rotates())
            {
                Quaternion originRotation = transform.rotation;

                var targetLookDirection = nav.AsDirection(LookDirection);
                var targetRotation = targetLookDirection.AsRotation();

                System.Action<float> interpolate = (float progress) => { transform.rotation = Quaternion.Lerp(originRotation, targetRotation, progress); };
                System.Action onDone = () =>
                {
                    SetNewGridPosition(Position, targetLookDirection);
                    if (onComplete != null) onComplete(Position, targetLookDirection);
                };

                return new NavInstructions(interpolate, onDone, turnTime, Position, targetLookDirection);
            }

            return NavInstructions.NoNavigation;
        }
    }
}