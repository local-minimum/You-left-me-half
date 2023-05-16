using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DeCrawl.Utils;
using DeCrawl.Primitives;
using DeCrawl.UI;
using DeCrawl.Lootables;

namespace YLHalf
{
    public delegate void ClearCorruption(Vector2Int coordinates);

    public class InventoryUI : AbstractInventoryUI
    {
        public static event ClearCorruption OnClearCorruption;

        [SerializeField]
        Inventory inventory;

        protected new void OnEnable()
        {
            Inventory.OnInventoryChange += Inventory_OnInventoryChange;

            InventorySlotUI.OnClickSlot += InventorySlotHUD_OnClickSlot;

            base.OnEnable();
        }


        protected new void OnDisable()
        {
            Inventory.OnInventoryChange -= Inventory_OnInventoryChange;

            InventorySlotUI.OnClickSlot -= InventorySlotHUD_OnClickSlot;

            base.OnDisable();
        }

        private void InventorySlotHUD_OnClickSlot(InventorySlotUI slot)
        {
            if (
                inventory.Repairs > 0 &&
                inventory.RemoveOneCorruption(
                    slot.Coordinates.XYVector3Int(),
                    () =>
                    {
                        if (inventory.Has(l => l is Repair, out Lootable repair))
                        {
                            repair.Loot(LootOwner.None);
                            return true;
                        }
                        return false;
                    },
                    out int remaining
                )
            )
            {
                if (remaining == 0)
                {
                    slot.State = InventorySlotUIState.Free;
                    slot.RomanNumeralCount = 0;
                }
                else if (remaining > 0)
                {
                    slot.RomanNumeralCount = remaining;
                }
            }
        }

        override protected void PositionBag(int rackIndex, RectTransform rt)
        {
            var rect = GetRackRect(rackIndex);

            rt.anchorMin = rect.min;
            rt.anchorMax = rect.max;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private void SlotPosition(Vector3Int coordinates, RectInt uiShape, RectTransform rt) => SlotPosition(coordinates, uiShape, rt, Vector3.zero);
        override protected void SlotPosition(Vector3Int coordinates, RectInt uiShape, RectTransform rt, Vector3 dragOffset)
        {
            int rackIndex = coordinates.y / Inventory.RackHeight;

            var rackRect = GetRackRect(rackIndex);
            var slotsRect = new Rect(
                rackRect.xMin + RackPadding.xMin * rackRect.width,
                rackRect.yMin + RackPadding.yMin * rackRect.height,
                rackRect.width * RackPadding.width,
                rackRect.height * RackPadding.height
            );

            var slotWidth = slotsRect.width / Inventory.RackWidth;
            var slotHeight = slotsRect.height / Inventory.RackHeight;

            var lootRect = new Rect(
                slotsRect.xMin + slotWidth * (coordinates.x - uiShape.xMin),
                slotsRect.yMin + slotHeight * (Inventory.RackHeight - (coordinates.y + 1 + uiShape.height / 2)),
                slotWidth * uiShape.width,
                slotHeight * uiShape.height
            );

            rt.anchorMin = lootRect.min;
            rt.anchorMax = lootRect.max;
            rt.offsetMin = dragOffset;
            rt.offsetMax = dragOffset;
        }

        static readonly Rect InventoryPadding = new Rect(0.01f, 0.005f, 0.98f, 0.99f);

        private RectTransform CreateChild(string name, Texture2D tex)
        {
            var go = new GameObject(name);

            var image = go.AddComponent<Image>();
            image.preserveAspect = true;
            image.raycastTarget = false;

            var sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.one * 0.5f);
            sprite.name = name;
            image.sprite = sprite;

            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(transform);
            return rt;
        }

        private Rect GetRackRect(int rackIndex)
        {
            var rackHeight = InventoryPadding.height / Inventory.RackLimit;
            return new Rect(
                InventoryPadding.xMin,
                InventoryPadding.yMin + (Inventory.RackLimit - rackIndex - 1) * rackHeight,
                InventoryPadding.width,
                rackHeight
            );
        }

        [SerializeField]
        private InventorySlotUI slotPrefab;

        protected override RectTransform CreateBagUI(Lootable loot, IInventoryBag bag, Vector3Int placement)
        {
            var rackIndex = placement.y;

            var rt = CreateChild($"Rack: {rackIndex}", loot.texture);
            PositionBag(rackIndex, rt);

            for (
                int y = rackIndex * Inventory.RackHeight,
                yMax = (rackIndex + 1) * Inventory.RackHeight,
                localY = 0;
                y < yMax;
                y++, localY++
             )
            {
                for (int x = 0; x < Inventory.RackWidth; x++)
                {
                    var coords = new Vector2Int(x, y);
                    var slot = Instantiate(slotPrefab);

                    slot.gameObject.name = $"Slot {coords}";
                    slot.transform.SetParent(transform);
                    slot.Coordinates = coords;
                    bag.ApplySlotState(slot, localY, x);


                    SlotPosition(new Vector3Int(x, y), new RectInt(0, 0, 1, 1), slot.GetComponent<RectTransform>());

                    Slots.Add(coords, slot);
                }
            }
            return rt;
        }

        static readonly Rect RackPadding = new Rect(0.05f, 0f, 0.9f, 1f);

        
        override protected RectTransform CreateLootUI(Lootable loot, Vector3Int placement)
        {
            var rt = CreateChild($"Item: {loot.Id}", loot.texture);

            if (loot is Canister)
            {
                var canisterHUD = rt.gameObject.AddComponent<CanisterHUD>();
                canisterHUD.canister = loot as Canister;
            }
            else if (loot is Attack)
            {
                var lootHUD = rt.gameObject.AddComponent<AttackInventoryHUD>();
                lootHUD.attack = loot as Attack;
            }

            SlotPosition(placement, loot.UIShape, rt);
            return rt;
        }

    }
}
