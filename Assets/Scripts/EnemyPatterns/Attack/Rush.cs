using System.Collections.Generic;
using UnityEngine;
using DeCrawl.Utils;
using DeCrawl.Primitives;

public class Rush : EnemyPattern
{
    public override bool Terminatable => true;

    List<(int, int)> attackPlan;

    public override bool Play()
    {
        if (GetAttackPlan(out attackPlan))
        {
            Debug.Log(Name);
            attackIdx = -1;
            playing = true;
            return true;
        }
        return false;
    }


    private bool GetAttackPlan(out List<(int, int)> path)
    {
        if ((Level.instance.PlayerPosition - movable.Position).AsDirection() != movable.LookDirection)
        {
            // Debug.Log("Not looking at player");
            path = new List<(int, int)>();
            return false;
        }

        var eligable = LineSearch.Search(
            new LineSearch.SearchParameters(
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
                            return enemy.AllowVirtualSpace && Level.instance.GridStatus(coords) != GridEntity.Other;
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

    public override bool Eligible => (!onlyFromBasicPosition || enemy.LastRegisteredBasicPosition.Equals(movable.Position)) && GetAttackPlan(out List<(int, int)> _);

    [SerializeField]
    bool onlyFromBasicPosition = true;

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
