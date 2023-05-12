using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DeCrawl.World;

namespace DeCrawl.Enemies.PatternEnemy
{
    public abstract class AbstractEnemyPattern<Entity, ClaimCondition> : MonoBehaviour
    {
        protected AbstractMovingEntity<Entity, ClaimCondition> movable;
        protected AbstractPatternEnemy<Entity, ClaimCondition> enemy;

        protected bool playing = false;

        [SerializeField]
        float selectionPriority = 1;

        public bool Playing => playing;

        public string Name
        {
            get => $"{enemy.name}: {name}";
        }


        private void Awake()
        {
            enemy = GetComponentInParent<AbstractPatternEnemy<Entity, ClaimCondition>>();
            movable = GetComponentInParent<AbstractMovingEntity<Entity, ClaimCondition>>();
        }

        protected bool easing = false;

        public virtual bool Terminatable => !easing;

        abstract public bool Eligible { get; }

        virtual public void Abort()
        {
            playing = false;
        }

        virtual public bool Resume() => Play();

        virtual public bool Play()
        {
            if (Eligible)
            {
                Debug.Log(Name);
                playing = true;
            }

            return playing;
        }

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
}
