using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public delegate void AttackPlayer(AttackMode mode, int amount);

public enum SightMode { Any, LOS, Area };

public class Enemy : MonoBehaviour
{
    public static event AttackPlayer OnAttackPlayer;

    [SerializeField]
    EnemyPattern[] DefaultPatterns;

    [SerializeField]
    EnemyPattern[] AttackPatterns;

    [SerializeField]
    int AreaAwareness = 1;

    [SerializeField]
    int LOSAwareness = 5;

    [SerializeField]
    public bool AllowVirtualSpace = true;

    MovingEntity movable;

    public readonly NullableItem<Vector3Int> LastRegisteredBasicPosition = new NullableItem<Vector3Int>();

    EnemyPattern RandomPattern
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

    EnemyPattern activePattern;

    private void Start()
    {
        movable = GetComponent<MovingEntity>();
        movable.SetNewGridPosition(Level.AsGridPosition(transform.position), transform.forward.AsDirection());
        activePattern = RandomPattern;
        if (!(activePattern?.Play() ?? false))
        {
            Debug.LogWarning("Failed to launch enemy pattern");
        }
    }

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

    bool DirectionTo((int, int) coords, out FaceDirection direction)
    {
        var offset = new Vector3Int(coords.Item1, 0, coords.Item2) - movable.Position;

        if ((offset.x == 0) == (offset.z == 0))
        {
            direction = FaceDirection.Down;
            return false;
        }

        direction = offset.AsDirection();
        return true;
    }

    public bool SeesPlayer(SightMode mode, out List<(int, int)> path)
    {        
        if (!Level.instance.FindPathToPlayerFrom(
            movable.Position.XZTuple(), 
            GetAwareness(mode), 
            (entity) => entity.BaseTypeIsInbound(AllowVirtualSpace),
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
                if (DirectionTo(coords, out FaceDirection dir)) {
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
        } else if (mode == SightMode.Area)
        {
            return path.Count() <= AreaAwareness;
        }
        return isLOS && path.Count() <= LOSAwareness;
    }

    private void Update()
    {
        if (activePattern == null || !activePattern.Playing)
        {
            activePattern = RandomPattern;
            activePattern?.Play();
        }
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
        if (type == EndingType.Death)
        {
            if (activePattern)
            {
                activePattern.enabled = false;
            }
            enabled = false;
        }
    }

    public void AttackPlayer(AttackStats attack)
    {
        var mode = attack.GetAttack(out int amount);
        OnAttackPlayer?.Invoke(mode, amount);
    }
}
