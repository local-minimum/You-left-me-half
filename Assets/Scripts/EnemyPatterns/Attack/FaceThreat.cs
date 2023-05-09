using System.Collections.Generic;
using UnityEngine;
using DeCrawl.Primitives;

public class FaceThreat : EnemyPattern
{
    [SerializeField]
    float turnTime = 1f;

    [SerializeField]
    float minTimeBetweenTurns = 0.4f;
    [SerializeField]
    float maxTimeBetweenTurns = 0.5f;

    public override bool Eligible => !enemy.SeesPlayer(SightMode.LOS, out List<(int, int)> _)
        && enemy.SeesPlayer(SightMode.Area, out List<(int, int)> _)
        && NavigationExtensions.FromToRotation(movable.LookDirection, FacePlayer) != Navigation.None;

    float nextTurn = 0f;

    CardinalDirection FacePlayer
    {
        get
        {
            return (Level.instance.PlayerPosition - movable.Position).AsDirection(true);
        }
    }

    private void Update()
    {
        if (!playing || easing) return;

        if (!Eligible)
        {
            playing = false;
            return;
        }

        if (Time.timeSinceLevelLoad < nextTurn) return;
        
        var nav = NavigationExtensions.FromToRotation(movable.LookDirection, FacePlayer);
        if (nav == Navigation.None)
        {
            playing = false;
            return;
        }

        var instructions = movable.Navigate(
            GridEntity.Other,
            nav,
            0,
            turnTime,
            (_, _) =>
            {
                nextTurn = Time.timeSinceLevelLoad + Random.Range(minTimeBetweenTurns, maxTimeBetweenTurns);
            },
            false
        ); ;

        StartCoroutine(Move(instructions));
    }
}
