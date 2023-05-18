using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DeCrawl.Primitives;

namespace FP
{
    public class FacePlayer : MonoBehaviour
    {
        [SerializeField, Range(0, 90)]
        float degreesStep = 30f;

        [SerializeField]
        float ignoreDistancesSquared = 20;

        private void OnEnable()
        {
            PlayerController.OnPlayerMove += PlayerController_OnPlayerMove;
        }

        private void PlayerController_OnPlayerMove(Vector3Int position, CardinalDirection lookDirection)
        {
            Vector3 playerPosition = NodeLevel.instance.AsWorldPosition(position);
            var offset = (playerPosition - transform.position);
            offset.y = 0;
            var offsetMag2 = offset.sqrMagnitude;
            if (offsetMag2 > ignoreDistancesSquared || offsetMag2 < 1) return;

            var targetY = Quaternion.LookRotation(offset, Vector3.up).eulerAngles.y;
            targetY /= degreesStep;            
            transform.rotation = Quaternion.Euler(0, Mathf.Round(targetY) * degreesStep, 0);
        }
    }
}
