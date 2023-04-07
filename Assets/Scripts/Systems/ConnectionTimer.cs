using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectionTimer : MonoBehaviour
{
    [SerializeField]
    TMPro.TextMeshProUGUI TextField;

    [SerializeField]
    Color startColor;

    [SerializeField]
    Color endColor;

    [SerializeField]
    float blinkLastSeconds = 5;

    [SerializeField]
    float blinkDuration = 0.5f;

    [SerializeField]
    int blinkShowRatio = 3;

    private void Start()
    {
        HandleUplink(null, InventoryEvent.PickUp);
    }

    private void OnEnable()
    {
        Inventory.OnInventoryChange += Inventory_OnInventoryChange;
        MasterOfEndings.OnEnding += MasterOfEndings_OnEnding;
    }

    private void OnDisable()
    {
        Inventory.OnInventoryChange -= Inventory_OnInventoryChange;
        MasterOfEndings.OnEnding -= MasterOfEndings_OnEnding;
    }

    private void MasterOfEndings_OnEnding(EndingType type, Ending ending)
    {
        enabled = false;
    }

    float timerStart;
    float duration;
    bool disconnected = false;

    void HandleUplink(Uplink uplink, InventoryEvent evt)  {
        if (evt == InventoryEvent.Drop)
        {
            timerStart = Time.timeSinceLevelLoad;
            duration = uplink.GraceSeconds;
            disconnected = true;
            TextField.gameObject.SetActive(true);
            TextField.enabled = true;
        } else if (evt == InventoryEvent.PickUp)
        {
            disconnected = false;
            TextField.gameObject.SetActive(false);
        }
    }

    private void Inventory_OnInventoryChange(Lootable loot, InventoryEvent inventoryEvent, Vector3Int placement)
    {
        if (loot is Uplink) HandleUplink((Uplink)loot, inventoryEvent);
    }

    private void Update()
    {
        if (!disconnected) return;
        
        var remaining = Mathf.Max(0, duration - (Time.timeSinceLevelLoad - timerStart));

        TextField.text = remaining.ToString("00.0");
        TextField.color = Color.Lerp(startColor, endColor, 1 - remaining / duration);

        if (remaining < blinkLastSeconds)
        {            
            TextField.enabled = (Mathf.FloorToInt(blinkShowRatio * remaining / blinkDuration) % blinkShowRatio) != 0;
        }

        if (remaining == 0)
        {
            MasterOfEndings.Instance.TriggerDisconnect();
        }
    }
}
