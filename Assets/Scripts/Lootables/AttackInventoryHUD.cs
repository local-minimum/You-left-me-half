using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DeCrawl.UI;

namespace YLHalf
{
    public delegate void AttackEvent(Attack attack);

    public class AttackInventoryHUD : ProgressionIcon
    {
        public static event AttackEvent OnAttack;
        public Attack attack;
        public bool IsActionHud = false;

        float lastAttack;

        private void Start()
        {
            Configure();
            if (IsActionHud) SetupActionInteractions();
        }

        private void OnEnable()
        {
            MasterOfEndings.OnEnding += MasterOfEndings_OnEnding;
        }

        private void OnDisable()
        {
            MasterOfEndings.OnEnding -= MasterOfEndings_OnEnding;
        }

        private void MasterOfEndings_OnEnding(EndingType type, Ending ending)
        {
            enabled = false;
        }

        void Configure() => Configure(attack.textureProgress, attack.textureOverlay, attack.fillMethod);


        void SetupActionInteractions()
        {
            var trigger = gameObject.AddComponent<EventTrigger>();
            var entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerClick;
            entry.callback.AddListener(_ => HandleClick());
            trigger.triggers.Add(entry);

            lastAttack = Time.timeSinceLevelLoad;
        }

        float ActionStatus
        {
            get
            {
                return Mathf.Clamp01((Time.timeSinceLevelLoad - lastAttack - attack.attackStats.beforeCooldownSeconds) / attack.attackStats.cooldownSeconds);
            }
        }

        void HandleClick()
        {
            if (ActionStatus == 1)
            {
                lastAttack = Time.timeSinceLevelLoad;
                OnAttack?.Invoke(attack);
            }
        }

        private void Update()
        {
            if (!IsActionHud) return;

            progressImage.fillAmount = ActionStatus;
        }
    }
}