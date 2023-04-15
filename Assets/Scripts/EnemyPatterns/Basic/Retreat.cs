using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Retreat : EnemyPattern
{
    float walkDuration = 0.6f;
    float turnDuration = 0.4f;

    public override bool Eligible
    {
        get => RetreatStrategy(out List<(int, int)> _);
    }

    private bool RetreatStrategy(out List<(int, int)> path)
    {
        if (!enemy.LastRegisteredBasicPosition.HasItem)
        {
            path = new List<(int, int)>();
            return false;
        }

        var coords = enemy.LastRegisteredBasicPosition.Item;

        if (coords == movable.Position)
        {
            path = new List<(int, int)>();
            return false;
        }

        return Level.instance.FindPathFromTo(
            movable.Position.XZTuple(),
            coords.XZTuple(),
            -1,
            (entity) => entity.IsClaimable(enemy.AllowVirtualSpace),
            out path
        );
    }

    int pathIndex = -1;
    List<(int, int)> retreatPath = new List<(int, int)>();

    private void Update()
    {
        if (!playing || easing) return;


        if (pathIndex < 0 && !RetreatStrategy(out retreatPath))
        {
            playing = false;
            return;
        }

        pathIndex = retreatPath.IndexOf(movable.Position.XZTuple());

        if (pathIndex < 0 || pathIndex == retreatPath.Count - 1)
        {
            pathIndex = -1;
            playing = false;
            return;
        }

        var nextPosition = retreatPath[pathIndex + 1];

        var neededDirection = nextPosition.Subtract(movable.Position).XZVector3().AsDirection();
        
        if (neededDirection != movable.LookDirection)
        {
            var instructions = movable.Navigate(
                GridEntity.Other,
                NavigationExtensions.FromToRotation(movable.LookDirection, neededDirection),
                walkDuration,
                turnDuration,
                (_, _) => { },
                enemy.AllowVirtualSpace
            );
            StartCoroutine(Move(instructions));
        } else
        {
            var instructions = movable.Navigate(
                GridEntity.Other,
                Navigation.Forward,
                walkDuration,
                turnDuration,
                (_, _) => { },
                enemy.AllowVirtualSpace
            );
            StartCoroutine(Move(instructions));
        }
    }
}
