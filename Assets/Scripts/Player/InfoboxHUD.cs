using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InfoboxHUD : MonoBehaviour
{
    [SerializeField]
    TMPro.TextMeshProUGUI ui;

    private void OnEnable()
    {
        Inventory.OnInventoryChange += Inventory_OnInventoryChange;
    }

    private void OnDisable()
    {
        Inventory.OnInventoryChange -= Inventory_OnInventoryChange;
    }

    private int level = 0;
    private int tokens = 0;

    private void Inventory_OnInventoryChange(Lootable loot, InventoryEvent inventoryEvent, Vector3Int placement)
    {
        if (loot.GetType() == typeof(PlayerLevel))
        {
            if (inventoryEvent == InventoryEvent.PickUp)
            {
                level++;
            } else if (inventoryEvent == InventoryEvent.Drop)
            {
                level--;
            }

            ui.text = InfoText;
        } else if (loot.GetType() == typeof(Repair))
        {
            if (inventoryEvent == InventoryEvent.PickUp)
            {
                tokens++;
            } else if (inventoryEvent == InventoryEvent.Drop)
            {
                tokens--;
            }
            ui.text = InfoText;
        }
    }


    string InfoText
    {
        get => $"Level: {level}\nInventory Repairs: {tokens}";
    }
}
