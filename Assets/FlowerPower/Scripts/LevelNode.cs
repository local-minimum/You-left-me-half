using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DeCrawl.Primitives;
using DeCrawl.Utils;

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

        Color StatusColor
        {
            get
            {
                if (AccessibleDirections.Count == 0) return Color.clear;
                if (playerSpawn) return Color.cyan;

                switch (Occupant)
                {
                    case FPEntity.Nothing:
                        return Color.white;
                    case FPEntity.Player:
                        return Color.magenta;
                    case FPEntity.Enemy:
                        return Color.red;
                }
                return Color.black;
            }
        }

        float gizmoScale = 0.99f;
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = StatusColor;
            var size = gizmoScale * LevelGridSize.Size;
            Gizmos.DrawWireCube(transform.position + Vector3.up * 0.5f * LevelGridSize.Size, Vector3.one * size);
        }        
    }
}
