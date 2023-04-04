using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootableManifestation : MonoBehaviour
{
    Lootable lootable;
    bool hovered = false;

    private void Awake()
    {
        lootable = GetComponentInParent<Lootable>();
    }

    private void OnMouseEnter()
    {
        hovered = true;    
    }

    private void OnMouseExit()
    {
        hovered = false;
    }

    private void Update()
    {
        if (!hovered) return;

        if (Input.GetMouseButtonDown(0))
        {
            if (lootable.Owner != LootOwner.Level || (PlayerController.instance.Position - lootable.Coordinates).CheckerDitsance() > 1) return;

            if (!lootable.Loot(LootOwner.Player))
            {
                Debug.Log($"Player failed to pick up {lootable.Id}");
            }
        }
    }
}
