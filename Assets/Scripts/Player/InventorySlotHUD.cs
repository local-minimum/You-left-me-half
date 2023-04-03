using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum InventorySlotHUDState { Corrupted, Free, Occupied };

public delegate void BeginHoverSlot(InventorySlotHUD slot);
public delegate void EndHoverSlot(InventorySlotHUD slot);
public delegate void BeginDragLoot(string lootId);
public delegate void DragLoot(string lootId);
public delegate void EndDragLoot(string lootId);

public class InventorySlotHUD : MonoBehaviour
{
    public static event BeginHoverSlot OnBeginHoverSlot;
    public static event EndHoverSlot OnEndHoverSlot;
    public static event BeginDragLoot OnBeginDragLoot;
    public static event DragLoot OnDragLoot;
    public static event EndDragLoot OnEndDragLoot;

    private static string DraggedLoot { get; set; }
    public Vector2Int Coordinates { get; set; }
    public string LootId { get; set; }

    [SerializeField]
    Texture2D texture;

    Image _image;

    Image image
    {
        get
        {
            if (_image == null)
            {
                _image = GetComponent<Image>();
            }
            return _image;
        }
    }

    [SerializeField]
    Color corruptionColor;

    [SerializeField]
    Color freeColor;

    [SerializeField]
    Color occupiedColor;

    [SerializeField]
    Color hoverColor;

    private void Start()
    {
        image.preserveAspect = true;

        var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
        sprite.name = "Slot Background";        

        image.sprite = sprite;
    }

    private InventorySlotHUDState state = InventorySlotHUDState.Free;

    public InventorySlotHUDState State
    {
        get
        {
            return state;
        }

        set
        {
            state = value;
            Hover = false;
        }
    }

    public bool Hover
    {
        set
        {
            if (value)
            {
                image.color = hoverColor;
            } else {
                switch (state)
                {
                    case InventorySlotHUDState.Free:
                        image.color = freeColor;
                        break;
                    case InventorySlotHUDState.Occupied:
                        image.color = occupiedColor;
                        break;
                    case InventorySlotHUDState.Corrupted:
                        image.color = corruptionColor;
                        break;
                }
            }
        }
    }


    public void OnPointerEnter()
    {
        if (State == InventorySlotHUDState.Occupied)
        {
            OnBeginHoverSlot?.Invoke(this);
        }
    }

    public void OnPointerExit()
    {
        if (State == InventorySlotHUDState.Occupied)
        {
            OnEndHoverSlot?.Invoke(this);
        }
    }

    public void OnDragStart()
    {
        if (LootId != null)
        {
            if (DraggedLoot != null) OnEndDragLoot?.Invoke(DraggedLoot);

            DraggedLoot = LootId;
            OnBeginDragLoot?.Invoke(DraggedLoot);
        }
    }

    public void OnDrag()
    {
        if (DraggedLoot == LootId && LootId != null) OnDragLoot?.Invoke(DraggedLoot);
    }

    public void OnDragEnd()
    {
        if (DraggedLoot == LootId && LootId != null) OnEndDragLoot?.Invoke(DraggedLoot);
    }
}
