using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Button : WorldClickable
{
    [SerializeField]
    Transform GridSwapPosition;

    [SerializeField]
    GameObject door;

    [SerializeField]
    GridEntity openState = GridEntity.InBound;
    
    private void Start()
    {
        UpdateDoor();
    }

    protected override bool PreClickCheckRefusal() => false;

    // TODO: Require player inventory keys, where needed
    protected override bool RefuseClick() => false;

    protected override void OnClick()
    {
        UpdateDoor(door.activeSelf);
    }

    Vector3Int GridPosition
    {
        get => Level.AsGridPosition(GridSwapPosition.position);
    }

    void UpdateDoor() => UpdateDoor(GridPosition);
    

    void UpdateDoor(bool toOpen)
    {
        var gridPosition = GridPosition;

        if (toOpen)
        {
            if (!Level.instance.ClaimPosition(openState, gridPosition, true, true))
            {
                Debug.Log($"Could not open door {name}");
                return;
            }
        } else
        {
            if (!Level.instance.ReleasePosition(openState, gridPosition))
            {
                Debug.Log($"Could not close door {name}");
                return;
            }
        }

        UpdateDoor(gridPosition);
    }

    void UpdateDoor(Vector3Int gridPosition)
    {
        var status = Level.instance.GridStatus(gridPosition.x, gridPosition.z);
        door.SetActive(status == GridEntity.OutBound || status == GridEntity.VirtualOutBound);
    }
}
