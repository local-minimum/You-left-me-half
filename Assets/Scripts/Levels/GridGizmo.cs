using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DeCrawl.Systems.Development;

namespace YLHalf
{
    public class GridGizmo : MonoBehaviour
    {
#if UNITY_EDITOR
        private bool GridCharacterToColor(GridEntity entity, out Color color)
        {
            switch (entity)
            {
                case GridEntity.Other:
                    Debug.Log("other");
                    color = GridVisualizer.instance.StatusColor(GridVisualizer.GizmoColor.Enemy);
                    return true;
                case GridEntity.PlayerSpawn:
                    color = GridVisualizer.instance.StatusColor(GridVisualizer.GizmoColor.Stairs);
                    return true;
                case GridEntity.Player:
                    color = GridVisualizer.instance.StatusColor(GridVisualizer.GizmoColor.Player);
                    return true;
                case GridEntity.InBound:
                    color = GridVisualizer.instance.StatusColor(GridVisualizer.GizmoColor.Empty);
                    return true;
                case GridEntity.VirtualSpace:
                    color = GridVisualizer.instance.StatusColor(GridVisualizer.GizmoColor.Restricted);
                    return true;
                default:
                    color = Color.clear;
                    return false;
            }
        }

        private void DrawPositionGizmo((int, int) coord, GridEntity entity)
        {
            if (GridCharacterToColor(entity, out Color color))
            {
                GridVisualizer.instance.DrawTileGizmo(new Vector3(coord.Item1 * Level.GridScale, 0, coord.Item2 * Level.GridScale), Level.GridScale, color);
            }
        }

        private void OnDrawGizmos()
        {
            GetComponent<Level>().ApplyOverGrid(DrawPositionGizmo);
        }
#endif
    }
}