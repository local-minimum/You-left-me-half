using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct NavInstructions
{
    public bool enabled;
    public bool failed;
    public System.Action<float> Interpolate;
    public System.Action OnDone;
    public float duration;
    public Vector3Int targetPosition;
    public FaceDirection targetLookDirection;

    public NavInstructions(
        System.Action<float> interpolate, 
        System.Action onDone, 
        float duration, 
        Vector3Int targetPosition,
        FaceDirection targetLookDirection
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

    public static NavInstructions NoNavigation {
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


public delegate void MoveEvent(string id, Vector3Int position, FaceDirection lookDirection);

/// <summary>
/// Component for all things moving about on a level grid
/// </summary>
public class MovingEntity : MonoBehaviour
{
    [SerializeField, Tooltip("Leave empty to use game object name")]
    string id;

    public event MoveEvent OnMove;

    public string Id
    {
        get
        {
            return string.IsNullOrEmpty(id) ? name : id;
        }
    }

    public Vector3Int Position { get; private set; }
    public FaceDirection LookDirection { get; private set; }

    public void SetNewGridPosition(Vector3Int position, FaceDirection lookDirection)
    {
        this.Position = position;
        this.LookDirection = lookDirection;
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
    bool ClaimSpot(GridEntity entity, Navigation navigation, Vector3Int target, bool allowEnterVirtualSpaces)
    {
        if (navigation.Translates())
        {
            if (Level.instance.ClaimPosition(entity, target, allowEnterVirtualSpaces))
            {
                return true;
            }
            return false;
        }
        return true;
    }

    public NavInstructions Navigate(
        GridEntity entity,
        Navigation nav,
        float moveTime, 
        float turnTime,
        System.Action<Vector3Int, FaceDirection> onComplete,
        bool allowEnterVirtualSpaces
    )
    {
        if (nav.Translates())
        {
            Vector3 origin = transform.position;
            Vector3Int gridTarget = GetNavigationTraget(nav);

            if (ClaimSpot(entity, nav, gridTarget, allowEnterVirtualSpaces))
            {

                Vector3 target = Level.AsWorldPosition(gridTarget);

                System.Action<float> interpolate = (float progress) => { transform.position = Vector3.Lerp(origin, target, progress); };
                System.Action onDone = () => {
                    Level.instance.ReleasePosition(entity, Position);
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

            var targetLookDirection = nav.asDirection(LookDirection);
            var targetRotation = targetLookDirection.AsRotation();

            System.Action<float> interpolate = (float progress) => { transform.rotation = Quaternion.Lerp(originRotation, targetRotation, progress); };
            System.Action onDone = () => {
                SetNewGridPosition(Position, targetLookDirection);
                if (onComplete != null) onComplete(Position, targetLookDirection);
            };

            return new NavInstructions(interpolate, onDone, turnTime, Position, targetLookDirection);
        }

        return NavInstructions.NoNavigation;
    }
}
