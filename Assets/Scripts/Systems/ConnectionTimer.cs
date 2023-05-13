using System.Collections.Generic;
using UnityEngine;
using DeCrawl.Primitives;
using DeCrawl.UI;

namespace YLHalf
{
    public class ConnectionTimer : CountDownTimer
    {
        private void Start()
        {
            HandleUplink(null, InventoryEvent.PickUp);
        }

        private void OnEnable()
        {
            Inventory.OnInventoryChange += Inventory_OnInventoryChange;
            MasterOfEndings.OnEnding += MasterOfEndings_OnEnding;
            OnTimesUp += ConnectionTimer_OnTimesUp;
        }

        private void OnDisable()
        {
            Inventory.OnInventoryChange -= Inventory_OnInventoryChange;
            MasterOfEndings.OnEnding -= MasterOfEndings_OnEnding;
            OnTimesUp -= ConnectionTimer_OnTimesUp;
        }

        private void ConnectionTimer_OnTimesUp()
        {
            MasterOfEndings.instance.TriggerDisconnect();
        }

        private void MasterOfEndings_OnEnding(EndingType type, Ending ending)
        {
            enabled = false;
        }

        void HandleUplink(Uplink uplink, InventoryEvent evt)
        {
            if (evt == InventoryEvent.Drop)
            {
                StartCountDown(uplink.GraceSeconds);
            }
            else if (evt == InventoryEvent.PickUp)
            {
                StopCountDown(true);                
            }
        }

        private void Inventory_OnInventoryChange(Lootable loot, InventoryEvent inventoryEvent, Vector3Int placement)
        {
            if (loot is Uplink) HandleUplink((Uplink)loot, inventoryEvent);
        }
    }
}