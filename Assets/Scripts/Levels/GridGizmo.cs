using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridGizmo : MonoBehaviour
{
#if UNITY_EDITOR
    [SerializeField, Range(0, 1)] float gizmoScale = 0.97f;

    private bool GridCharacterToColor(GridEntity entity, out Color color)
    {
        switch (entity)
        {
            case GridEntity.Other:
                color = Color.red;
                return true;
            case GridEntity.PlayerSpawn:
                color = Color.yellow;
                return true;
            case GridEntity.Player:
                color = Color.magenta;
                return true;
            case GridEntity.InBound:
                color = Color.cyan;
                return true;
            case GridEntity.VirtualSpace:
                color = Color.gray;
                return true;
            default:
                color = Color.black;
                return false;
        }
    }

    private void DrawPositionGizmo((int, int) coord, GridEntity entity)
    {
        if (GridCharacterToColor(entity, out Color color))
        {
            Gizmos.color = color;
            Gizmos.DrawWireCube(new Vector3(coord.Item1, 0.5f, coord.Item2) * Level.GridScale, Vector3.one * Level.GridScale * gizmoScale);
        }
    }

    private void OnDrawGizmos()
    {
        GetComponent<Level>().ApplyOverGrid(DrawPositionGizmo);
    }
#endif
}
