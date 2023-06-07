using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DeCrawl.Primitives;
using DeCrawl.Utils;
using DeCrawl.Systems.Development;

namespace FP
{
    public class LevelNode : MonoBehaviour
    {
        public bool playerSpawn;
        public CardinalDirection PlayerSpawnDirection;

        [SerializeField]
        public List<CardinalDirection> AccessibleDirections = new List<CardinalDirection>() { 
            CardinalDirection.Up,
            CardinalDirection.North,
            CardinalDirection.West,
            CardinalDirection.South,
            CardinalDirection.East,
        };

        public bool CanBeReachedFrom(Vector3Int coordinates)
        {
            var offset = coordinates - Coordinates;
            if (offset.ManhattanMagnitude() != 1) return false;

            return AccessibleDirections.Contains(offset.AsDirection());
        }

        public Vector3Int Coordinates {
            get
            {                
                return NodeLevel.instance.AsGridPosition(transform.position);
            }
        }

        private List<FPEntity> occupants = new List<FPEntity>();
        public FPEntity Occupant
        {
            get
            {
                if (occupants.Count == 0) return FPEntity.Nothing;
                return occupants[0];
            }
        }

        public void ClearOccupants()
        {
            occupants.Clear();
        }

        public bool AddOccupant(FPEntity entity)
        {
            if (occupants.Count == 0 || occupants.Contains(entity))
            {
                occupants.Add(entity);
                return true;
            }
            return false;
        }

        public bool RemoveOccupant(FPEntity entity) => occupants.Remove(entity);

        #if UNITY_EDITOR
        Color StatusColor
        {
            get
            {
                if (AccessibleDirections.Count == 0) return GridVisualizer.instance.StatusColor(GridVisualizer.GizmoColor.None);

                switch (Occupant)
                {
                    case FPEntity.Player:
                        return GridVisualizer.instance.StatusColor(GridVisualizer.GizmoColor.Player);
                    case FPEntity.Enemy:
                        return GridVisualizer.instance.StatusColor(GridVisualizer.GizmoColor.Enemy);
                    case FPEntity.Nothing:
                        if (GetComponent<FightTrigger>()?.WillTrigger ?? false) return GridVisualizer.instance.StatusColor(GridVisualizer.GizmoColor.Trigger);
                        if (playerSpawn) return GridVisualizer.instance.StatusColor(GridVisualizer.GizmoColor.Stairs);
                        return GridVisualizer.instance.StatusColor(GridVisualizer.GizmoColor.Empty);
                }
                
                return GridVisualizer.instance.StatusColor(GridVisualizer.GizmoColor.Empty);
            }
        }

        private void OnDrawGizmos()
        {
            GridVisualizer.instance.DrawTileGizmo(transform.position, LevelGridSize.Size, StatusColor);
        }
        #endif
    }
}
