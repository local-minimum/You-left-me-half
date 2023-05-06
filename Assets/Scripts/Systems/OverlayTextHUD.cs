using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverlayTextHUD : MonoBehaviour
{
    [SerializeField]
    GameObject Panel;

    [SerializeField]
    TMPro.TextMeshProUGUI TextField;

    bool started = false;

    void Start()
    {
        Panel.SetActive(false);
    }

    private void OnEnable()
    {
        MasterOfEndings.OnEnding += MasterOfEndings_OnEnding;
        Inventory.OnInventoryChange += Inventory_OnInventoryChange;
    }

    private void OnDisable()
    {
        MasterOfEndings.OnEnding -= MasterOfEndings_OnEnding;
        Inventory.OnInventoryChange -= Inventory_OnInventoryChange;
    }

    [SerializeField, Range(0, 3)]
    float levelUpTime = 1f;

    int playerLevel = 0;

    private void Inventory_OnInventoryChange(Lootable loot, InventoryEvent inventoryEvent, Vector3Int placement)
    {
        if (loot.GetType() == typeof(PlayerLevel))
        {
            if (inventoryEvent == InventoryEvent.PickUp)
            {
                playerLevel++;
            } else if (inventoryEvent == InventoryEvent.Drop)
            {
                playerLevel--;
            }

            if (started)
            {
                TextField.text = $"Level {playerLevel}";
                StartCoroutine(Splash(levelUpTime));
            }
        }
        
    }

    IEnumerator<WaitForSecondsRealtime> Splash(float duration)
    {
        Time.timeScale = 0;
        Panel.SetActive(true);
        yield return new WaitForSecondsRealtime(duration);
        Panel.SetActive(false);
        Time.timeScale = 1;
    }

    private void MasterOfEndings_OnEnding(EndingType type, Ending ending)
    {
        if (type == EndingType.Death)
        {
            switch (ending)
            {
                case Ending.NoHealth:
                    TextField.text = "Death by violence";
                    break;
                case Ending.NoHealthCanister:
                    TextField.text = "Death by curiosity";
                    break;
                case Ending.LostConnection:
                    TextField.text = "Connection Lost";
                    break;
            }
            Panel.SetActive(true);
        }
    }

    private void Update()
    {
        started = true;
    }
}
