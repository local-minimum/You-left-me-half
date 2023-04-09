using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum SightMode { Any, LOS, Area };

public class Enemy : MonoBehaviour
{
    [SerializeField]
    EnemyPattern[] DefaultPatterns;

    [SerializeField]
    EnemyPattern[] AttackPatterns;

    [SerializeField]
    int AreaAwareness = 1;

    [SerializeField]
    int LOSAwareness = 5;

    [SerializeField]
    bool SeeThroughVirtual = true;

    MovingEntity movable;

    EnemyPattern RandomDefaultPattern
    {
        get
        {
            var options = DefaultPatterns.Where(p => p.Eligible).ToArray();
            return options[Random.Range(0, options.Length)];
        }
    }

    private void Start()
    {
        movable = GetComponent<MovingEntity>();
        movable.SetNewGridPosition(Level.AsGridPosition(transform.position), transform.forward.AsDirection());
        if (!RandomDefaultPattern.Play())
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

    public bool SeesPlayer(SightMode mode, out List<(int, int)> path)
    {
        if (!Level.instance.FindPathToPlayerFrom(
            movable.Position.XZTuple(), 
            GetAwareness(mode), 
            (entity) => entity.IsInbound(SeeThroughVirtual),
            out path
        ))
        {
            return false;
        }

        if (mode == SightMode.Any)
        {
            if (path.GroupBy(coords => coords.Item1).Count() == 1 || path.GroupBy(coords => coords.Item2).Count() == 1)
            {
                return path.Count() < LOSAwareness;
            }

            return path.Count() < AreaAwareness;
        } else if (mode == SightMode.Area)
        {
            return path.Count() < AreaAwareness;
        }
        return path.Count() < LOSAwareness;
    }
}
