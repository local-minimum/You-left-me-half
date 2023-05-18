using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DeCrawl.Primitives;

namespace FP
{
    public class Teleporter : MonoBehaviour
    {
        [SerializeField]
        LevelNode teleportTarget;

        [SerializeField]
        CardinalDirection teleportDirection;

        LevelNode node;

        private void Start()
        {
            node = GetComponent<LevelNode>();
        }

        private void OnEnable()
        {
            PlayerController.OnPlayerMove += PlayerController_OnPlayerMove;
        }

        private void PlayerController_OnPlayerMove(Vector3Int position, DeCrawl.Primitives.CardinalDirection lookDirection)
        {
            if (position != node.Coordinates) return;

            PlayerController.instance.Teleport(teleportTarget.Coordinates, teleportDirection);
        }
    }
}
