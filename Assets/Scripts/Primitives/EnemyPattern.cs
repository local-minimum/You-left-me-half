using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyPattern : MonoBehaviour
{
    protected Enemy enemy;
    protected MovingEntity movable;
    protected bool playing = false;


    private void Awake()
    {
        enemy = GetComponentInParent<Enemy>();
        movable = GetComponentInParent<MovingEntity>();
    }

    [SerializeField]
    float selectionPriority = 1;

    public bool Playing => playing;

    abstract public bool Terminatable { get;  }
    abstract public bool Eligible { get; }
    abstract public void Abort();

    abstract public bool Resume();

    abstract public bool Play();

    protected bool easing = false;

    protected IEnumerator<WaitForSeconds> Move(NavInstructions navInstructions)
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
        }

        navInstructions.OnDone?.Invoke();

        easing = false;
    }
}
