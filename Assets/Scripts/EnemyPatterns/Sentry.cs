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

    bool easing = false;

    public override bool Terminatable => !easing;

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
        if (!playing || easing || Time.timeSinceLevelLoad < nextTurn) return;

        var instructions = movable.Navigate(
            GridEntity.Other,
            GetNextNavigation(),
            0,
            turnTime,
            (_, _) => { easing = false; },
            false
        );

        StartCoroutine(Move(instructions));
    }

    IEnumerator<WaitForSeconds> Move(NavInstructions navInstructions)
    {
        easing = true;

        if (navInstructions.enabled)
        {
            float start = Time.timeSinceLevelLoad;
            float progress = 0;
            float tick = Mathf.Max(0.02f, navInstructions.duration / 100f);
            while (progress < 1)
            {
                progress = (Time.timeSinceLevelLoad - start) / navInstructions.duration;
                navInstructions.Interpolate(progress);
                yield return new WaitForSeconds(tick);
            }

            navInstructions.OnDone();
        }

        nextTurn = Time.timeSinceLevelLoad + Random.Range(minLookTime, maxLookTime);

        easing = false;
    }
}
