using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct NavInstructions
{
    public bool enabled;
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
        Interpolate = interpolate;
        OnDone = onDone;
        this.duration = duration;
        this.targetPosition = targetPosition;
        this.targetLookDirection = targetLookDirection;
    }

    public static NavInstructions NoMove {
        get
        {
            var intructions = new NavInstructions();
            intructions.enabled = false;
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

    Vector3Int position;
    FaceDirection lookDirection;

    public void SetNewGridPosition(Vector3Int position, FaceDirection lookDirection)
    {
        this.position = position;
        this.lookDirection = lookDirection;
        OnMove?.Invoke(Id, position, lookDirection);
    }

    Vector3Int GetNavigationTraget(Navigation navigation)
    {
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
                    Level.instance.ReleasePosition(GridEntity.Player, position);
                    SetNewGridPosition(gridTarget, lookDirection);
                    onComplete(gridTarget, lookDirection);
                    
                };
                return new NavInstructions(interpolate, onDone, moveTime, gridTarget, lookDirection);
            }
            else
            {
                return NavInstructions.NoMove;
            }

        }
        else if (nav.Rotates())
        {
            Quaternion originRotation = transform.rotation;

            var targetLookDirection = nav.asDirection(lookDirection);
            var targetRotation = targetLookDirection.AsRotation();

            System.Action<float> interpolate = (float progress) => { transform.rotation = Quaternion.Lerp(originRotation, targetRotation, progress); };
            System.Action onDone = () => {
                SetNewGridPosition(position, targetLookDirection);
                onComplete(position, targetLookDirection);
            };

            return new NavInstructions(interpolate, onDone, turnTime, position, targetLookDirection);
        }

        return NavInstructions.NoMove;
    }
}
