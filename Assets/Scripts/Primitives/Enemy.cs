using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
}
