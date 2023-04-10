using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Sentry : EnemyPattern
{
    [SerializeField]
    bool dynamicSentryPos = false;

    [SerializeField]
    bool setSentryFromMove = false;

    [SerializeField]
    Vector3Int sentryPosition;

    [SerializeField]
    private FaceDirection[] LookDirections;

    [SerializeField]
    float minLookTime = 1;

    [SerializeField]
    float maxLookTime = 1.2f;

    [SerializeField]
    float turnTime = 0.4f;

    public override void Abort()
    {
        playing = false;
    }

    public override bool Eligible => dynamicSentryPos || sentryPosition == movable.Position;

    public override bool Play()
    {
        if (Eligible)
        {
            playing = true;
        }

        return playing;
    }

    public override bool Resume() => Play();    

    public override bool Terminatable => !easing;

    private void OnEnable()
    {
        if (setSentryFromMove)
        {
            movable.OnMove += Sentry_OnMove;
        }
    }

    private void OnDisable()
    {
        if (setSentryFromMove)
        {
            movable.OnMove -= Sentry_OnMove;
        }
    }

    private void Sentry_OnMove(string id, Vector3Int position, FaceDirection lookDirection)
    {
        sentryPosition = position;
    }

    Navigation GetNextNavigation()
    {
        var options = LookDirections.Where(d => d != movable.LookDirection).ToArray();
        if (options.Length == 0) return Navigation.None;
        var lookTarget = options[Random.Range(0, options.Length)];

        return NavigationExtensions.FromToRotation(movable.LookDirection, lookTarget);
    }

    float nextTurn;

    private void Update()
    {
        if (!playing || easing) return;

        if (enemy.SeesPlayer(SightMode.LOS, out List<(int, int)> path))
        {
            Debug.Log("Spotted");
            playing = false;
            return;
        } 

        if (Time.timeSinceLevelLoad < nextTurn) return;

        var instructions = movable.Navigate(
            GridEntity.Other,
            GetNextNavigation(),
            0,
            turnTime,
            (_, _) => {
                nextTurn = Time.timeSinceLevelLoad + Random.Range(minLookTime, maxLookTime);
            },
            false
        );

        StartCoroutine(Move(instructions));
    }
}
