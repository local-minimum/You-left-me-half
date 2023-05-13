using System.Collections.Generic;
using UnityEngine;
using DeCrawl.Utils;
using DeCrawl.Primitives;

namespace DeCrawl.UI
{
    public class BattleHit : MonoBehaviour
    {
        [SerializeField]
        Color Normal;

        [SerializeField]
        Color Critical;

        [SerializeField]
        Color Fail;

        [SerializeField]
        int normalSize = 32;

        [SerializeField]
        int critSize = 42;

        [SerializeField]
        int failSize = 24;

        [SerializeField]
        float lifeTime = 0.8f;

        [SerializeField]
        AnimationCurve alphaOverLifetime;

        [SerializeField]
        AnimationCurve offsetOverLifeTime;

        [SerializeField]
        TMPro.TextMeshProUGUI ui;

        [SerializeField]
        Rect shape;

        public string Text
        {
            get => ui.text;
            set { ui.text = value; }
        }

        Color startColor;

        public AttackMode AttackMode
        {
            set
            {
                switch (value)
                {
                    case AttackMode.CritFail:
                    case AttackMode.Fail:
                        startColor = Fail;
                        break;
                    case AttackMode.Normal:
                        startColor = Normal;
                        break;
                    case AttackMode.CritSuccess:
                        startColor = Critical;
                        break;
                }

                ui.color = startColor;
            }
        }

        public void SetHit(int amount, AttackMode mode)
        {
            Text = amount.ToString();
            AttackMode = mode;
        }

        void SetPosition(float t)
        {
            var yOffset = offsetOverLifeTime.Evaluate(t);
            var anchor = new Vector2(x, yOffset);
            var rt = (RectTransform)transform;
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.offsetMin = shape.min;
            rt.offsetMax = shape.max;
        }

        void SetColor(float t)
        {
            ui.color = startColor.Clone(alphaOverLifetime.Evaluate(t));
        }


        float x;
        float t0;

        private void OnEnable()
        {
            x = Random.value;
            SetPosition(0);
            t0 = Time.timeSinceLevelLoad;
        }

        private void Update()
        {
            var t = (Time.timeSinceLevelLoad - t0) / lifeTime;
            if (t > 1f)
            {
                gameObject.SetActive(false);
                return;
            }

            SetPosition(t);
            SetColor(t);
        }
    }
}