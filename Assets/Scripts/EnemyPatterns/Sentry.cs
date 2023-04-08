using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    float minLook = 1;

    [SerializeField]
    float maxLook = 1.2f;

    public override void Abort()
    {
        playing = false;
    }

    public override void Play()
    {
        playing = true;
    }

    public override void Resume()
    {
        Play();
    }

    public override bool Terminatable => true;

    MovingEntity movable;

    private void OnEnable()
    {
        if (setSentryFromMove)
        {
            movable = GetComponentInParent<MovingEntity>();
            movable.OnMove += Sentry_OnMove;
        }
    }

    private void OnDisable()
    {
        if (movable != null)
        {
            movable.OnMove -= Sentry_OnMove;
        }
    }

    private void Sentry_OnMove(string id, Vector3Int position, FaceDirection lookDirection)
    {
        sentryPosition = position;
    }

    private void Update()
    {
        if (!playing) return;
    }
}
