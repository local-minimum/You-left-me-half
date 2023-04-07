using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootableManifestation : WorldClickable
{
    Lootable lootable;

    private void Awake()
    {
        lootable = GetComponentInParent<Lootable>();
    }

    protected override bool PreClickCheckRefusal() => !lootable.enabled;

    protected override bool RefuseClick() =>
        lootable.Owner != LootOwner.Level ||
        (PlayerController.instance.Position - lootable.Coordinates).CheckerDitsance() > 1;

    protected override void OnClick()
    {
        if (!lootable.Loot(LootOwner.Player))
        {
            Debug.Log($"Player failed to pick up {lootable.Id}");
        }
    }
}
