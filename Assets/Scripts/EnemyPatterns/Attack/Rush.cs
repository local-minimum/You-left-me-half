using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rush : EnemyPattern
{
    public override bool Terminatable => true;

    public override bool Resume() => Play();

    List<(int, int)> attackPlan;

    public override bool Play()
    {
        Debug.Log("Rush");
        if (GetAttackPlan(out attackPlan))
        {
            playing = true;
            return true;
        }
        return false;
    }

    public override void Abort()
    {
        playing = false;
    }


    private bool GetAttackPlan(out List<(int, int)> path)
    {
        if ((Level.instance.PlayerPosition - movable.Position).AsDirection() != movable.LookDirection)
        {
            Debug.Log($"{Level.instance.PlayerPosition} - {movable.Position} => {(Level.instance.PlayerPosition - movable.Position).AsDirection()} != {movable.LookDirection}");
            path = new List<(int, int)>();
            return false;
        }

        var eligable = GraphSearch.LineSearch(
            new GraphSearch.LineSearchParameters(
                movable.Position.XZTuple(),
                Level.instance.PlayerPosition,
                (coords) =>
                {
                    switch (Level.instance.GridStatus(coords))
                    {
                        case GridEntity.Player:
                        case GridEntity.InBound:
                        case GridEntity.PlayerSpawn:
                            return true;
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
