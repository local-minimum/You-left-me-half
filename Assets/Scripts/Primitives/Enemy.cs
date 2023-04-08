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

    private void Start()
    {
        movable = GetComponent<MovingEntity>();
        movable.SetNewGridPosition(Level.AsGridPosition(transform.position), transform.forward.AsDirection());
    }
}
