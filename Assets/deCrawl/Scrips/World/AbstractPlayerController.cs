using System.Collections.Generic;
using UnityEngine;
using DeCrawl.Primitives;

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

        [SerializeField]
        KeyCode forwardKey = KeyCode.W;
        [SerializeField]
        KeyCode backKey = KeyCode.S;
        [SerializeField]
        KeyCode leftKey = KeyCode.A;
        [SerializeField]
        KeyCode rightKey = KeyCode.D;
        [SerializeField]
        KeyCode turnCWKey = KeyCode.E;
        [SerializeField]
        KeyCode turnCCWKey = KeyCode.Q;

        AbstractMovingEntity<Entity, ClaimCondition> movableEntity;

        public Vector3Int Position { get; private set; }

        private void MovableEntity_OnMove(string id, Vector3Int position, CardinalDirection lookDirection)
        {
            Position = position;
            transform.position = Level.AsWorldPosition(position);
            transform.rotation = Quaternion.LookRotation(lookDirection.AsVector());
        }

        protected void OnEnable()
        {
            MasterOfEndings.OnEnding += MasterOfEndings_OnEnding;
        }

        protected void OnDisable()
        {
            MasterOfEndings.OnEnding -= MasterOfEndings_OnEnding;
        }

        private void MasterOfEndings_OnEnding(EndingType type, Ending ending)
        {
            enabled = false;
        }

        protected void Start()
        {
            movableEntity = GetComponent<AbstractMovingEntity<Entity, ClaimCondition>>();            
            movableEntity.OnMove += MovableEntity_OnMove;

            var lookDirection = Level.PlayerFirstSpawnDirection;
            var position = Level.PlayerFirstSpawnPosition;


            movableEntity.SetNewGridPosition(position, lookDirection);
            Level.ClaimPosition(PlayerEntity, position, ClaimCond);
            OnPlayerMove?.Invoke(position, lookDirection);
        }



        Navigation GetKeyPress(bool alsoKey = false)
        {
            if (Input.GetKeyDown(forwardKey) || alsoKey && Input.GetKey(forwardKey)) return Navigation.Forward;
            if (Input.GetKeyDown(backKey) || alsoKey && Input.GetKey(backKey)) return Navigation.Backward;
            if (Input.GetKeyDown(leftKey) || alsoKey && Input.GetKey(leftKey)) return Navigation.Left;
            if (Input.GetKeyDown(rightKey) || alsoKey && Input.GetKey(rightKey)) return Navigation.Right;
            if (Input.GetKeyDown(turnCWKey) || alsoKey && Input.GetKey(turnCWKey)) return Navigation.TurnCW;
            if (Input.GetKeyDown(turnCCWKey) || alsoKey && Input.GetKey(turnCCWKey)) return Navigation.TurnCCW;
            return Navigation.None;
        }


        bool isMoving = false;

        List<Navigation> navigationQueue = new List<Navigation>(2) { Navigation.None, Navigation.None };

        protected void Update()
        {
            var nav = GetKeyPress();
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

                if (QueueMovesOnButton && GetKeyPress(true) == nav)
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