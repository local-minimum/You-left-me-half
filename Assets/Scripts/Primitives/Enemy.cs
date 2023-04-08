using System.Collections;
using System.Collections.Generic;
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
            return DefaultPatterns[Random.Range(0, DefaultPatterns.Length)];
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
