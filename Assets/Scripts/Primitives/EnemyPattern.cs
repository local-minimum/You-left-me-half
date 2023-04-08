using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyPattern : MonoBehaviour
{
    protected Enemy enemy;
    protected bool playing = false;


    private void Awake()
    {
        enemy = GetComponentInParent<Enemy>();
    }

    [SerializeField]
    float selectionPriority;

    public bool Playing => playing;

    abstract public bool Terminatable { get;  }
    abstract public bool Eligible { get; }
    abstract public void Abort();

    abstract public bool Resume();

    abstract public bool Play();    
}
