using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DeCrawl.Utils;

public enum InventorySlotHUDState { Corrupted, Free, Occupied };

public delegate void BeginHoverSlot(InventorySlotHUD slot);
public delegate void EndHoverSlot(InventorySlotHUD slot);
public delegate void ClickSlot(InventorySlotHUD slot);
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
    public static event ClickSlot OnClickSlot;

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

    [SerializeField]
    Color pulseColor;

    [SerializeField]
    AnimationCurve pulse;

    private void Start()
    {
        image.preserveAspect = true;

        var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
        sprite.name = "Slot Background";        

        image.sprite = sprite;
    }

    private void OnEnable()
    {
        MasterOfEndings.OnEnding += MasterOfEndings_OnEnding;
    }

    private void OnDisable()
    {
        MasterOfEndings.OnEnding -= MasterOfEndings_OnEnding;
    }

    private void MasterOfEndings_OnEnding(EndingType type, Ending ending)
    {
        enabled = false;
    }

    private InventorySlotHUDState state = InventorySlotHUDState.Free;

    [SerializeField]
    TMPro.TextMeshProUGUI text;

    public int CorruptionCount
    {
        set
        {
            text.text = value.ToRomanNumerals();
        }
    }


    public InventorySlotHUDState State
    {
        get
        {
            return state;
        }

        set
        {
            if (state == InventorySlotHUDState.Corrupted && value != InventorySlotHUDState.Corrupted && pulsing)
            {
                StopPulsing();
            }
            state = value;
            Hover = false;
            
            SetBaseColor();
        }
    }

    void SetBaseColor()
    {
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

    public bool Hover
    {
        set
        {
            if (value)
            {
                image.color = hoverColor;
            } else {
                SetBaseColor();
            }
        }
    }

    bool pulsing = false;

    public void PulseCorruption()
    {
        StartCoroutine(AnimatePulse(corruptionColor));
    }

    IEnumerator<WaitForSeconds> AnimatePulse(Color baseColor)
    {
        pulsing = true;
        float t0 = Time.timeSinceLevelLoad;
        while (pulsing)
        {
            float t = (Time.timeSinceLevelLoad - t0) % pulse.keys[pulse.keys.Length - 1].time;
            image.color = Color.Lerp(baseColor, pulseColor, t);
            yield return new WaitForSeconds(0.02f);
        }
        SetBaseColor();
    }

    public void StopPulsing()
    {
        pulsing = false;
    }

    public bool Pulsing { get => pulsing; }

    public void OnPointerEnter()
    {
        OnBeginHoverSlot?.Invoke(this);
    }

    public void OnPointerExit()
    {
        OnEndHoverSlot?.Invoke(this);
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

    public void OnClick()
    {
        if (DraggedLoot == null) OnClickSlot?.Invoke(this);
    }

    public void OnDragEnd()
    {
        if (DraggedLoot == LootId && LootId != null)
        {
            OnEndDragLoot?.Invoke(DraggedLoot);
            DraggedLoot = null;
        }
    }
}
