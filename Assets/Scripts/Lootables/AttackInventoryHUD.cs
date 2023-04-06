using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public delegate void AttackEvent(Attack attack);

public class AttackInventoryHUD : HUDProgressionIcon
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


    void HandleClick()
    {
        if (Time.timeSinceLevelLoad - lastAttack > attack.cooldownSeconds)
        {
            lastAttack = Time.timeSinceLevelLoad;
            OnAttack?.Invoke(attack);
        }
    }

    private void Update()
    {
        if (!IsActionHud) return;

        progressImage.fillAmount = Mathf.Clamp01((Time.timeSinceLevelLoad - lastAttack) / attack.cooldownSeconds);
    }
}
