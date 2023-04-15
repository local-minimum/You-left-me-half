using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void PlayerMove(Vector3Int position, FaceDirection lookDirection);

public class PlayerController : MonoBehaviour
{
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

    MovingEntity movableEntity;
    Inventory inventory;
    public static PlayerController instance { get; set; }    

    public Vector3Int Position { get; private set; }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        } else if (instance == null)
        {
            instance = this;
        }
        movableEntity = GetComponent<MovingEntity>();
        inventory = GetComponentInChildren<Inventory>();
        movableEntity.OnMove += MovableEntity_OnMove;
    }

    private void OnDestroy()
    {
        if (instance = this)
        {
            instance = null;
        }
        movableEntity.OnMove -= MovableEntity_OnMove;
    }

    private void MovableEntity_OnMove(string id, Vector3Int position, FaceDirection lookDirection)
    {
        Position = position;
        transform.position = Level.AsWorldPosition(position);
        transform.rotation = Quaternion.LookRotation(lookDirection.AsVector());
    }

    private void OnEnable()
    {
        MasterOfEndings.OnEnding += MasterOfEndings_OnEnding;
    }

    private void OnDisable()
    {
        MasterOfEndings.OnEnding -= MasterOfEndings_OnEnding;
    }

    private void MasterOfEndings_OnEnding(EndingType type, Ending ending)
    {
        enabled = false;
    }

    void Start()
    {
        var lookDirection = Level.instance.PlayerSpawnDirection;
        var position = Level.instance.PlayerFirstSpawnPosition;


        movableEntity.SetNewGridPosition(position, lookDirection);
        Level.instance.ClaimPosition(GridEntity.Player, position, AllowEnterVirtualSpaces);
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

    void Update()
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

    bool AllowEnterVirtualSpaces
    {
        get => !inventory.Has(loot => loot.GetType() == typeof(Uplink), out Lootable loot);
    }

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
                GridEntity.Player,
                nav,
                moveTime,
                turnTime,
                (newPosition, newLookDirection) => OnPlayerMove?.Invoke(newPosition, newLookDirection),
                AllowEnterVirtualSpaces
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

            if (QueueMovesOnButton && GetKeyPress(true) == nav)
            {
                // Repeat move
                navigationQueue[moveIndex] = nav;
            } else
            {
                moveIndex = 1;
            }           
        }

        isMoving = false;
    }
}
