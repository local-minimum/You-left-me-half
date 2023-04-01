using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    FaceDirection lookDirection;
    Vector3Int position;

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

    private void Awake()
    {
        movableEntity = GetComponent<MovingEntity>();
        inventory = GetComponent<Inventory>();
        movableEntity.OnMove += MovableEntity_OnMove;
    }

    private void OnDestroy()
    {
        movableEntity.OnMove -= MovableEntity_OnMove;
    }

    private void MovableEntity_OnMove(string id, Vector3Int position, FaceDirection lookDirection)
    {
        this.position = position;
        transform.position = Level.AsWorldPosition(position);
        this.lookDirection = lookDirection;
        transform.rotation = Quaternion.LookRotation(lookDirection.AsVector());
    }

    void Start()
    {
        lookDirection = Level.instance.PlayerSpawnDirection;
        position = Level.instance.PlayerFirstSpawnPosition;

        transform.rotation = Quaternion.LookRotation(lookDirection.AsVector());
        transform.position = Level.AsWorldPosition(position);

        Level.instance.ClaimPosition(GridEntity.Player, position, AllowEnterVirtualSpaces);
    }

    Navigation GetKeyPress()
    {
        if (Input.GetKeyDown(forwardKey)) return Navigation.Forward;
        if (Input.GetKeyDown(backKey)) return Navigation.Backward;
        if (Input.GetKeyDown(leftKey)) return Navigation.Left;
        if (Input.GetKeyDown(rightKey)) return Navigation.Right;
        if (Input.GetKeyDown(turnCWKey)) return Navigation.TurnCW;
        if (Input.GetKeyDown(turnCCWKey)) return Navigation.TurnCCW;
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
    /// <summary>
    /// Query level if position can be reserved / entered by player
    /// </summary>
    /// <param name="navigation">Relative navigation</param>
    /// <returns></returns>
    bool ClaimSpot(Navigation navigation, Vector3Int target)
    {            
        if (navigation.Translates())
        {
            if (Level.instance.ClaimPosition(GridEntity.Player, target, AllowEnterVirtualSpaces))
            {
                return true;
            }
            return false;
        }
        return true;
    }

    void PathBlocked()
    {

    }

    Vector3Int GetNavigationTraget(Navigation navigation)
    {
        // Debug.Log($"{navigation} from {position} looking {lookDirection}");
        switch (navigation)
        {
            case Navigation.Forward:
                return position + lookDirection.AsIntVector();
            case Navigation.Backward:
                return position + lookDirection.Invert().AsIntVector();
            case Navigation.Left:
                return position + lookDirection.RotateCCW().AsIntVector();
            case Navigation.Right:
                return position + lookDirection.RotateCW().AsIntVector();
            default:
                return position;
        }
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

            System.Action<float> action = null;
            System.Action afterAction = null;

            float duration = 0;

            if (nav.Translates())
            {
                Vector3 origin = transform.position;
                Vector3Int gridTarget = GetNavigationTraget(nav);

                if (ClaimSpot(nav, gridTarget))
                {

                    Vector3 target = Level.AsWorldPosition(gridTarget);
                    duration = moveTime;

                    action = (float progress) => { transform.position = Vector3.Lerp(origin, target, progress); };
                    afterAction = () => { 
                        Level.instance.ReleasePosition(GridEntity.Player, position);
                        movableEntity?.SetNewGridPosition(gridTarget, lookDirection);
                    };
                }
                else
                {
                    PathBlocked();
                }

            }
            else if (nav.Rotates())
            {
                Quaternion origin = transform.rotation;
                var startRotation = origin.eulerAngles.y;
                var endRotation = startRotation + ((nav == Navigation.TurnCW) ? 90 : -90);
                Quaternion target = Quaternion.Euler(0, endRotation, 0);

                duration = turnTime;
                action = (float progress) => { transform.rotation = Quaternion.Lerp(origin, target, progress); };
                afterAction = () => {                         
                    movableEntity?.SetNewGridPosition(position, nav.asDirection(lookDirection));
                };
            }

            if (action != null)
            {
                float start = Time.timeSinceLevelLoad;
                float progress = 0;
                while (progress < 1)
                {
                    progress = (Time.timeSinceLevelLoad - start) / duration;
                    action(progress);
                    yield return new WaitForSeconds(Mathf.Max(0.02f, duration / 100f));
                }

                if (afterAction != null) afterAction();
            }


            moveIndex = 1;
        }


        isMoving = false;
    }
}
