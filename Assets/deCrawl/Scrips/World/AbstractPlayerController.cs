using System.Collections.Generic;
using UnityEngine;
using DeCrawl.Primitives;
using DeCrawl.Systems;

namespace DeCrawl.World
{
    public delegate void PlayerMove(Vector3Int position, CardinalDirection lookDirection);


    public abstract class AbstractPlayerController<Entity, ClaimCondition> : FindingSingleton<AbstractPlayerController<Entity, ClaimCondition>>
    {
        abstract public ILevel<Entity, ClaimCondition> Level { get; }
        abstract protected Entity PlayerEntity { get; }
        abstract protected ClaimCondition ClaimCond { get;  }

        [SerializeField]
        bool QueueMovesOnButton = true;

        public static event PlayerMove OnPlayerMove;

        AbstractMovingEntity<Entity, ClaimCondition> movableEntity;

        public Vector3Int Position { get; private set; }

        private void MovableEntity_OnMove(string id, Vector3Int position, CardinalDirection lookDirection)
        {
            Position = position;
            transform.position = Level.AsWorldPosition(position);
            transform.rotation = Quaternion.LookRotation(lookDirection.AsVector());
        }

        protected void Start()
        {
            movableEntity = GetComponent<AbstractMovingEntity<Entity, ClaimCondition>>();            
            movableEntity.OnMove += MovableEntity_OnMove;

            Game.OnChangeStatus += Game_OnChangeStatus;
            NavigationAllowed = Game.Status == GameStatus.Playing;

            DungeonInput.OnInput += DungeonInput_OnInput;

            var lookDirection = Level.PlayerFirstSpawnDirection;
            var position = Level.PlayerFirstSpawnPosition;


            movableEntity.SetNewGridPosition(position, lookDirection);            
            Level.ClaimPositionForced(PlayerEntity, position);            
            OnPlayerMove?.Invoke(position, lookDirection);
        }

        Navigation AsNavigation(DungeonInput.InputEvent input)
        {
            switch (input)
            {
                case DungeonInput.InputEvent.MoveForward:
                    return Navigation.Forward;
                case DungeonInput.InputEvent.MoveBackwards:
                    return Navigation.Backward;
                case DungeonInput.InputEvent.StrafeLeft:
                    return Navigation.Left;
                case DungeonInput.InputEvent.StrafeRight:
                    return Navigation.Right;
                case DungeonInput.InputEvent.TurnClockWise:
                    return Navigation.TurnCW;
                case DungeonInput.InputEvent.TurnCounterClockWise:
                    return Navigation.TurnCCW;
                default:
                    return Navigation.None;
            }
        }

        Navigation HeldNav = Navigation.None;

        private void DungeonInput_OnInput(DungeonInput.InputEvent input, DungeonInput.InputType type)
        {
            var nav = AsNavigation(input);

            if (HeldNav != Navigation.None && (nav != HeldNav || DungeonInput.OverlappingTypes(type, DungeonInput.InputType.Up)))
            {
                HeldNav = Navigation.None;
            }
            else if (type == DungeonInput.InputType.Held)
            {
                HeldNav = nav;
            }

            if (!NavigationAllowed || teleporting || !DungeonInput.OverlappingTypes(type, DungeonInput.InputType.Down)) return;

            switch (nav)
            {
                case Navigation.None:
                    return;
                default:
                    if (isMoving)
                    {
                        navigationQueue[1] = nav;
                    }
                    else
                    {
                        navigationQueue[0] = nav;
                        StartCoroutine(Move());
                    }
                    break;
            }
        }

        protected bool NavigationAllowed { get; private set; }

        private void Game_OnChangeStatus(GameStatus status, GameStatus oldStatus)
        {
            NavigationAllowed = status == GameStatus.Playing;    
        }

        [SerializeField]
        float teleportDelay = 0.1f;

        bool teleporting = false;
        public void Teleport(Vector3Int position, CardinalDirection direction)
        {
            if (Level.ClaimPositionForced(PlayerEntity, position))
            {
                StartCoroutine(_Teleport(position, direction));
            }
        }

        IEnumerator<WaitForSeconds> _Teleport(Vector3Int position, CardinalDirection direction)
        {
            teleporting = true;
            Debug.Log($"Teleporting from {movableEntity.Position}");
            yield return new WaitForSeconds(teleportDelay);

            Level.ReleasePosition(PlayerEntity, movableEntity.Position);
            movableEntity.SetNewGridPosition(position, direction);
            OnPlayerMove?.Invoke(position, direction);
            Debug.Log($"Teleported to {position}");
            teleporting = false;
        }


        bool isMoving = false;

        List<Navigation> navigationQueue = new List<Navigation>(2) { Navigation.None, Navigation.None };

        [SerializeField, Range(0, 2)]
        float turnTime = 0.4f;
        [SerializeField, Range(0, 2)]
        float moveTime = 0.5f;

        IEnumerator<WaitForSeconds> Move()
        {
            isMoving = true;
            int moveIndex = 0;
            while (true)
            {
                if (teleporting || !NavigationAllowed)
                {
                    if (!NavigationAllowed)
                    {
                        for (int i =0; i<navigationQueue.Count; i++)
                        {
                            navigationQueue[i] = Navigation.None;
                        }                        
                    }
                    yield return new WaitForSeconds(0.02f);
                    continue;
                }

                Navigation nav = navigationQueue[moveIndex];
                if (nav == Navigation.None)
                {
                    break;
                }
                navigationQueue[moveIndex] = Navigation.None;

                var navInstructions = movableEntity.Navigate(
                    PlayerEntity,
                    nav,
                    moveTime,
                    turnTime,
                    (newPosition, newLookDirection) => OnPlayerMove?.Invoke(newPosition, newLookDirection),
                    ClaimCond
                );

                if (navInstructions.enabled)
                {
                    float start = Time.timeSinceLevelLoad;
                    float progress = 0;
                    float tick = Mathf.Max(0.02f, navInstructions.duration / 100f);
                    while (progress < 1)
                    {
                        progress = (Time.timeSinceLevelLoad - start) / navInstructions.duration;
                        navInstructions.Interpolate(progress);
                        yield return new WaitForSeconds(tick);
                    }

                    navInstructions.OnDone();
                }
                else
                {
                    break;
                }

                if (QueueMovesOnButton && HeldNav == nav)
                {
                    // Repeat move
                    navigationQueue[moveIndex] = nav;
                }
                else
                {
                    moveIndex = 1;
                }
            }

            isMoving = false;
        }
    }
}