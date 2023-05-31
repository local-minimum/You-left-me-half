using System.Collections.Generic;
using UnityEngine;
using DeCrawl.Primitives;
using DeCrawl.Systems;
using DeCrawl.Utils;

namespace DeCrawl.UI
{
    public abstract class AbstractInventoryUI : FindingSingleton<AbstractInventoryUI>
    {
        protected Dictionary<string, Lootable> Loots = new Dictionary<string, Lootable>();
        protected Dictionary<string, RectTransform> Transforms = new Dictionary<string, RectTransform>();

        protected Dictionary<Vector2Int, InventorySlotUI> Slots = new Dictionary<Vector2Int, InventorySlotUI>();

        protected void OnEnable()
        {
            Game.OnChangeStatus += Game_OnChangeStatus;

            InventorySlotUI.OnBeginHoverSlot += InventorySlotHUD_OnBeginHoverSlot;
            InventorySlotUI.OnEndHoverSlot += InventorySlotHUD_OnEndHoverSlot;

            InventorySlotUI.OnBeginDragLoot += InventorySlotHUD_OnBeginDragLoot;
            InventorySlotUI.OnDragLoot += InventorySlotHUD_OnDragLoot;
            InventorySlotUI.OnEndDragLoot += InventorySlotHUD_OnEndDragLoot;
        }

        protected void OnDisable()
        {
            Game.OnChangeStatus -= Game_OnChangeStatus;

            InventorySlotUI.OnBeginHoverSlot -= InventorySlotHUD_OnBeginHoverSlot;
            InventorySlotUI.OnEndHoverSlot -= InventorySlotHUD_OnEndHoverSlot;

            InventorySlotUI.OnBeginDragLoot -= InventorySlotHUD_OnBeginDragLoot;
            InventorySlotUI.OnDragLoot -= InventorySlotHUD_OnDragLoot;
            InventorySlotUI.OnEndDragLoot -= InventorySlotHUD_OnEndDragLoot;

        }

        private void Game_OnChangeStatus(GameStatus status, GameStatus oldStatus)
        {
            if (status == GameStatus.Playing)
            {
                enabled = true;
            } else if (status == GameStatus.CutScene || status == GameStatus.Paused || status == GameStatus.GameOver)
            {
                enabled = false;
            }
        }

        #region Loot
        /// <summary>
        /// Invokes an action over all slots occupied by an inventory loot
        /// </summary>
        /// <param name="lootId">The loot in question</param>
        /// <param name="action">The action to perform</param>
        protected void ApplyOverLootSlots(string lootId, System.Action<Vector2Int> action)
        {
            foreach (var coordinates in Loots[lootId].InventorySlots())
            {
                action(coordinates);
            }
        }

        protected void ApplyNewSlotState(Lootable loot, Vector3Int placement, InventorySlotUIState state)
        {
            foreach (var coordinates in loot.InventorySlots(placement))
            {
                var slot = Slots[coordinates];
                slot.State = state;
                slot.LootId = state == InventorySlotUIState.Free ? null : loot.Id;
            }

        }

        private void SlotPosition(Vector3Int coordinates, RectInt uiShape, RectTransform rt) => 
            SlotPosition(coordinates, uiShape, rt, Vector3.zero);

        protected abstract void SlotPosition(Vector3Int coordinates, RectInt uiShape, RectTransform rt, Vector3 dragOffset);

        abstract protected RectTransform CreateBagUI(Lootable loot, IInventoryBag bag, Vector3Int placement);
        abstract protected RectTransform CreateLootUI(Lootable loot, Vector3Int placement);
        abstract protected void PositionBag(int bagIndex, RectTransform rt);


        private void PutInInventory(Lootable loot, Vector3Int placement)
        {
            if (Loots.ContainsKey(loot.Id)) return;
                
            Loots.Add(loot.Id, loot);

            var bag = loot.GetComponent<IInventoryBag>();
            if (bag != null)
            {
                Transforms.Add(loot.Id, CreateBagUI(loot, bag, placement));
            }
            else
            {
                Transforms.Add(loot.Id, CreateLootUI(loot, placement));
                ApplyNewSlotState(loot, placement, InventorySlotUIState.Occupied);
            }
        }

        private void MoveLoot(Lootable loot, Vector3Int placement)
        {
            Debug.Log($"Inventory Move: {loot.Id}");
            var rectTransform = Transforms[loot.Id];
            if (loot.GetComponent<IInventoryBag>() != null)
            {
                PositionBag(placement.y, rectTransform);
            }
            else
            {

                ApplyNewSlotState(loot, loot.Coordinates, InventorySlotUIState.Free);
                SlotPosition(placement, loot.UIShape, rectTransform);
                ApplyNewSlotState(loot, placement, InventorySlotUIState.Occupied);
            }
        }

        private void DropLoot(Lootable loot)
        {
            var lootId = loot.Id;
            Loots.Remove(lootId);
            var lootTransform = Transforms[lootId];
            Destroy(lootTransform.gameObject);
            Transforms.Remove(lootId);
            ApplyNewSlotState(loot, loot.Coordinates, InventorySlotUIState.Free);
            Debug.Log($"Inventory Drop: {lootId}");
        }

        protected void Inventory_OnInventoryChange(Lootable loot, InventoryEvent inventoryEvent, Vector3Int placement)
        {
            switch (inventoryEvent)
            {
                case InventoryEvent.PickUp:
                    PutInInventory(loot, placement);
                    break;
                case InventoryEvent.Drop:
                    DropLoot(loot);
                    break;
                case InventoryEvent.Move:
                    MoveLoot(loot, placement);
                    break;
            }
        }

        #endregion

        #region Hover
        protected Vector2Int hoverSlot { get; private set; }

        private void InventorySlotHUD_OnBeginHoverSlot(InventorySlotUI slot)
        {
            hoverSlot = slot.Coordinates;

            if (dragged != null)
            {
                var slotsOffset = slot.Coordinates - dragSlotStart;
                ApplyOverLootSlots(dragged, coordinates =>
                {
                    var offsetCoordinates = coordinates + slotsOffset;
                    if (Slots.ContainsKey(offsetCoordinates))
                    {
                        Slots[offsetCoordinates].Hover = true;
                    }
                });
            }
            else if (slot.LootId != null)
            {
                ApplyOverLootSlots(slot.LootId, coordinates => { Slots[coordinates].Hover = true; });
            }
            else if (slot.State == InventorySlotUIState.Special) // && inventory.Repairs > 0)
            {
                slot.Pulse();
            }
        }

        private void InventorySlotHUD_OnEndHoverSlot(InventorySlotUI slot)
        {
            hoverSlot = Vector2Int.left;
            if (dragged != null)
            {
                var slotsOffset = slot.Coordinates - dragSlotStart;
                ApplyOverLootSlots(dragged, coordinates =>
                {
                    var offsetCoordinates = coordinates + slotsOffset;
                    if (Slots.ContainsKey(offsetCoordinates))
                    {
                        Slots[offsetCoordinates].Hover = false;
                    }
                });
            }
            else if (slot.LootId != null)
            {
                ApplyOverLootSlots(slot.LootId, coordinates => { Slots[coordinates].Hover = false; });
            }
            else if (slot.Pulsing)
            {
                slot.StopPulsing();
            }
        }
        #endregion

        #region Dragging
        protected string dragged { get; private set; }
        protected Vector3 mouseDragStart { get; private set; }
        protected Vector2Int dragSlotStart { get; private set; }

        private void InventorySlotHUD_OnBeginDragLoot(string lootId)
        {
            dragged = lootId;
            dragSlotStart = hoverSlot;
            mouseDragStart = Input.mousePosition;            
        }

        private void InventorySlotHUD_OnDragLoot(string lootId)
        {
            var offset = Input.mousePosition - mouseDragStart;
            var rt = Transforms[lootId];
            var loot = Loots[lootId];
            SlotPosition(loot.Coordinates, loot.UIShape, rt, offset);
        }

        private void InventorySlotHUD_OnEndDragLoot(string lootId)
        {
            if (dragged != null)
            {
                var slotsOffset = hoverSlot - dragSlotStart;
                ApplyOverLootSlots(dragged, coordinates =>
                {
                    var offsetCoordinates = coordinates + slotsOffset;
                    if (Slots.ContainsKey(offsetCoordinates))
                    {
                        Slots[offsetCoordinates].Hover = false;
                    }
                });
            }
            var rt = Transforms[lootId];
            var loot = Loots[lootId];
            dragged = null;

            if (hoverSlot.x < 0)
            {
                if (!loot.Loot(LootOwner.Level))
                {
                    Debug.LogWarning($"Level never picked up {lootId}");
                }
                return;
            }

            var newSlot = loot.Coordinates.XYVector2Int() + (hoverSlot - dragSlotStart);
            if (!loot.Loot(LootOwner.Player, newSlot.XYVector3Int()))
            {
                SlotPosition(loot.Coordinates, loot.UIShape, rt);
            }
        }
        #endregion
    }
}
