using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DeCrawl.Primitives;

namespace DeCrawl.Systems.Development
{
    public class GridVisualizer : SpawningSingleton<GridVisualizer>
    {
        public enum GizmoColor { None, Empty, Restricted, Player, Enemy, Stairs, Trigger };

        [Header("Colors")]

        [SerializeField]
        Color EmptyColor = Color.white;

        [SerializeField]
        Color RestrictedColor = Color.gray;

        [SerializeField]
        Color PlayerColor = Color.magenta;

        [SerializeField]
        Color EnemyColor = Color.red;

        [SerializeField]
        Color StairsColor = Color.cyan;

        [SerializeField]
        Color TriggerColor = Color.blue;

        public Color StatusColor(GizmoColor gizmoColor)
        {
            switch (gizmoColor) {
                case GizmoColor.Empty:
                    return EmptyColor;
                case GizmoColor.Restricted:
                    return RestrictedColor;
                case GizmoColor.Player:
                    return PlayerColor;
                case GizmoColor.Enemy:
                    return EnemyColor;
                case GizmoColor.Stairs:
                    return StairsColor;
                case GizmoColor.Trigger:
                    return TriggerColor;
            }
            
            return Color.clear;
        }

        [Header("Tiles")]
        [SerializeField, Range(0, 1)]
        float gizmoTileScale = 0.99f;
        [SerializeField, Range(0, 1)]
        float gizmoTileFloorAlpha = 0.75f;
        [SerializeField, Range(0, 1)]
        float gizmoTileFloorThickness = 0.1f;
        [SerializeField, Range(0, 1)]
        float gizmoTileCubeAlpha = 0.4f;

        public void DrawTileGizmo(Vector3 anchor, float gridSize, Color color)
        {
            var size = gizmoTileScale * gridSize;
            color.a = gizmoTileFloorAlpha;
            Gizmos.color = color;

            Gizmos.DrawCube(anchor, new Vector3(size, gizmoTileFloorThickness, size));

            color.a = gizmoTileCubeAlpha;
            Gizmos.color = color;

            Gizmos.DrawWireCube(anchor + Vector3.up * 0.5f * gridSize, Vector3.one * size);
        }
    }
}
