using UnityEngine;
using DeCrawl.Primitives;

namespace DeCrawl.World
{
    public struct NavInstructions
    {
        public bool enabled;
        public bool failed;
        public System.Action<float> Interpolate;
        public System.Action OnDone;
        public float duration;
        public Vector3Int targetPosition;
        public CardinalDirection targetLookDirection;

        public NavInstructions(
            System.Action<float> interpolate,
            System.Action onDone,
            float duration,
            Vector3Int targetPosition,
            CardinalDirection targetLookDirection
        )
        {
            enabled = true;
            failed = false;
            Interpolate = interpolate;
            OnDone = onDone;
            this.duration = duration;
            this.targetPosition = targetPosition;
            this.targetLookDirection = targetLookDirection;
        }

        public static NavInstructions NoNavigation
        {
            get
            {
                var intructions = new NavInstructions();
                intructions.enabled = false;
                return intructions;
            }
        }

        public static NavInstructions FailedMove
        {
            get
            {
                var intructions = new NavInstructions();
                intructions.enabled = false;
                intructions.failed = true;
                return intructions;
            }
        }

    }
}