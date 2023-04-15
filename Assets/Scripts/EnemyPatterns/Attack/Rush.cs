using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rush : EnemyPattern
{
    public override bool Terminatable => true;

    List<(int, int)> attackPlan;

    public override bool Play()
    {
        if (GetAttackPlan(out attackPlan))
        {
            Debug.Log("Rush");
            playing = true;
            return true;
        }
        return false;
    }


    private bool GetAttackPlan(out List<(int, int)> path)
    {
        if ((Level.instance.PlayerPosition - movable.Position).AsDirection() != movable.LookDirection)
        {
            // Debug.Log($"{Level.instance.PlayerPosition} - {movable.Position} => {(Level.instance.PlayerPosition - movable.Position).AsDirection()} != {movable.LookDirection}");
            path = new List<(int, int)>();
            return false;
        }

        var eligable = GraphSearch.LineSearch(
            new GraphSearch.LineSearchParameters(
                movable.Position.XZTuple(),
                Level.instance.PlayerPosition,
                (coords) =>
                {
                    if (coords == movable.Position.XZTuple()) return true;

                    switch (Level.instance.GridBaseStatus(coords))
                    {
                        case GridEntity.InBound:
                            return Level.instance.GridStatus(coords) != GridEntity.Other;
                        case GridEntity.VirtualSpace:
                            return enemy.SeeThroughVirtual && Level.instance.GridStatus(coords) != GridEntity.Other;
                        default:
                            return false;
                    }
                }
            ),
            out path
        );

        // Debug.Log(string.Join(" > ", path));

        return eligable;
    }

    public override bool Eligible => GetAttackPlan(out List<(int, int)> _);

    [SerializeField]
    float stepTime = 0.2f;
    int attackIdx = -1;

    private void Update()
    {
        if (!playing || easing) return;

        var idx = attackPlan.IndexOf(movable.Position.XZTuple());

        if (idx < 0 || idx == attackPlan.Count - 1 || idx == attackIdx)
        {
            Abort();
            return;
        }

        attackIdx = idx;

        var instructions = movable.Navigate(
            GridEntity.Other,
            Navigation.Forward,
            stepTime,
            0,
            (_, _) => { },
            false
        );

        StartCoroutine(Move(instructions));
    }
}
