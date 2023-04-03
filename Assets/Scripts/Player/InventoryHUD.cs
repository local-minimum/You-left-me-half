using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;


public class InventoryHUD : MonoBehaviour
{
    public static InventoryHUD instance { get; private set; }

    Dictionary<string, Lootable> Loots = new Dictionary<string, Lootable>();
    Dictionary<string, RectTransform> Transforms = new Dictionary<string, RectTransform>();

    [SerializeField]
    InventorySlotHUD slotPrefab;

    Dictionary<Vector2Int, InventorySlotHUD> Slots = new Dictionary<Vector2Int, InventorySlotHUD>();

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        if (instance == null) instance = this;
    }

    private void OnDestroy()
    {
        if (instance = this) instance = null;
    }

    private void OnEnable()
    {
        Inventory.OnInventoryChange += Inventory_OnInventoryChange;
        InventorySlotHUD.OnBeginHoverSlot += InventorySlotHUD_OnBeginHoverSlot;
        InventorySlotHUD.OnEndHoverSlot += InventorySlotHUD_OnEndHoverSlot;
        InventorySlotHUD.OnBeginDragLoot += InventorySlotHUD_OnBeginDragLoot;
        InventorySlotHUD.OnDragLoot += InventorySlotHUD_OnDragLoot;
        InventorySlotHUD.OnEndDragLoot += InventorySlotHUD_OnEndDragLoot;
    }

    private void OnDisable()
    {
        Inventory.OnInventoryChange -= Inventory_OnInventoryChange;
        InventorySlotHUD.OnBeginHoverSlot -= InventorySlotHUD_OnBeginHoverSlot;
        InventorySlotHUD.OnEndHoverSlot -= InventorySlotHUD_OnEndHoverSlot;
        InventorySlotHUD.OnBeginDragLoot -= InventorySlotHUD_OnBeginDragLoot;
        InventorySlotHUD.OnDragLoot -= InventorySlotHUD_OnDragLoot;
        InventorySlotHUD.OnEndDragLoot -= InventorySlotHUD_OnEndDragLoot;
    }

    Vector3 mouseDragStart;

    private void InventorySlotHUD_OnBeginDragLoot(string lootId)
    {
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
        var rt = Transforms[lootId];
        var loot = Loots[lootId];
        SlotPosition(loot.Coordinates, loot.UIShape, rt);
    }

    private void InventorySlotHUD_OnBeginHoverSlot(InventorySlotHUD slot)
    {
        var coordinates = Loots[slot.LootId].InventorySlots.ToArray();
        for (int i = 0; i < coordinates.Length; i++)
        {
            Slots[coordinates[i]].Hover = true;
        }
    }

    private void InventorySlotHUD_OnEndHoverSlot(InventorySlotHUD slot)
    {
        var coordinates = Loots[slot.LootId].InventorySlots.ToArray();
        for (int i = 0; i < coordinates.Length; i++)
        {
            Slots[coordinates[i]].Hover = false;
        }
    }

    private void PositionRack(int rackIndex, RectTransform rt)
    {
        var rect = GetRackRect(rackIndex);

        rt.anchorMin = rect.min;
        rt.anchorMax = rect.max;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private void SlotPosition(Vector3Int coordinates, RectInt uiShape, RectTransform rt) => SlotPosition(coordinates, uiShape, rt, Vector3.zero);
    private void SlotPosition(Vector3Int coordinates, RectInt uiShape, RectTransform rt, Vector3 dragOffset)
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
            InventoryPadding.yMin + (Inventory.RackLimit -  rackIndex - 1) * rackHeight,
            InventoryPadding.width,
            rackHeight
        );
    }

    private RectTransform CreateRackUI(Lootable loot, InventoryRack rack)
    {
        var rackIndex = loot.Coordinates.y;

        var rt = CreateChild($"Rack: {rackIndex}", loot.texture);
        PositionRack(rackIndex, rt);

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

                if (rack.Occupied[localY, x])
                {
                    slot.State = InventorySlotHUDState.Occupied;
                } else if (rack.Corruption[localY, x] > 0)
                {
                    slot.State = InventorySlotHUDState.Corrupted;
                } else 
                {
                    slot.State = InventorySlotHUDState.Free;
                }

                SlotPosition(new Vector3Int(x, y), new RectInt(0, 0, 1, 1), slot.GetComponent<RectTransform>());

                Slots.Add(coords, slot);
            }
        }
        return rt;
    }

    static readonly Rect RackPadding = new Rect(0.05f, 0f, 0.9f, 1f);

    private RectTransform CreateLootUI(Lootable loot)
    {
        var rt = CreateChild($"Item: {loot.Id}", loot.texture);

        Debug.Log($"{loot.UIShape} {loot.UIShape.center}");
        SlotPosition(loot.Coordinates, loot.UIShape, rt);
        return rt;
    }

    private void FreeSlotState(Lootable loot, Vector3Int previousOrigin)
    {
        var coordinates = loot.InventoryShape
            .Select(coords => new Vector2Int(coords.x + previousOrigin.x, coords.y + previousOrigin.y))
            .ToArray();
        for (int i = 0; i < coordinates.Length; i++)
        {
            var slot = Slots[coordinates[i]];
            slot.State = InventorySlotHUDState.Free;
            slot.LootId = null;
        }
    }


    private void ApplyNewSlotState(Lootable loot, InventorySlotHUDState state)
    {
        var coordinates = loot.InventorySlots.ToArray();
        for (int i = 0; i < coordinates.Length; i++)
        {
            var slot = Slots[coordinates[i]];
            slot.State = state;
            slot.LootId = loot.Id;
        }

    }

    private void PutInInventory(Lootable loot)
    {
        Loots.Add(loot.Id, loot);
        var rack = loot.GetComponent<InventoryRack>();
        if (rack != null)
        {
            Transforms.Add(loot.Id, CreateRackUI(loot, rack));
        } else
        {
            Transforms.Add(loot.Id, CreateLootUI(loot));
            ApplyNewSlotState(loot, InventorySlotHUDState.Occupied);
        }
    }

    private void MoveLoot(Lootable loot, Vector3Int previousOrigin)
    {
        var rectTransform = Transforms[loot.Id];
        if (loot.GetComponent<InventoryRack>())
        {
            PositionRack(loot.Coordinates.y, rectTransform);
        } else
        {

            FreeSlotState(loot, previousOrigin);
            SlotPosition(loot.Coordinates, loot.UIShape, rectTransform);
            ApplyNewSlotState(loot, InventorySlotHUDState.Occupied);
        }
    }

    private void DropLoot(Lootable loot)
    {
        var lootId = loot.Id;
        Loots.Remove(loot.Id);
        var lootTransform = Transforms[lootId];
        Destroy(lootTransform.gameObject);
        Transforms.Remove(lootId);
        ApplyNewSlotState(loot, InventorySlotHUDState.Free);
    }

    private void Inventory_OnInventoryChange(Lootable loot, InventoryEvent inventoryEvent, Vector3Int previousOrigin)
    {
        switch (inventoryEvent)
        {
            case InventoryEvent.PickUp:
                PutInInventory(loot);
                break;
            case InventoryEvent.Drop:
                DropLoot(loot);
                break;
            case InventoryEvent.Move:
                MoveLoot(loot, previousOrigin);
                break;

        }
    }
}
