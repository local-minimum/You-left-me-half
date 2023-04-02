using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum InventorySlotHUDState { Corrupted, Free, Occupied };

public class InventorySlotHUD : MonoBehaviour
{
    public Vector2Int Coordinates { get; set; }

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
            switch (value)
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
            state = value;
        }
    }
}
