using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DeCrawl.Gadgets
{
    public class MarkowDice : MonoBehaviour
    {
        public delegate void DiceEvent(int diceIndex, int value, int previousValue, bool rolling);

        public event DiceEvent OnDiceChange;

        private static readonly int[] Faces = new int[] { 1, 2, 3, 4, 5, 6 };

        public static IEnumerable<int> ValueNeighbours(int face)
        {
            var opposing = GetOpposing(face);
            return Faces.Where(v => v != opposing);
        }

        public static int GetOpposing(int face) => 7 - face;        

        [SerializeField, Range(0, 1)]
        float velocityDecay = 0.3f;

        [SerializeField]
        AnimationCurve velocityToSampleTime;

        [SerializeField]
        AnimationCurve velocityToStayProbability;

        [SerializeField]
        AnimationCurve velocityToSwirveProbability;

        bool[] rolling;
        float[] velocities;
        float[] sampleTimes;
        int[] upFaces;
        int[] previousUpFaces;

        float GetNextSampleTime(int diceIndex) => Time.timeSinceLevelLoad + velocityToSampleTime.Evaluate(velocities[diceIndex]);

        void EmitEvent(int diceIndex) => OnDiceChange?.Invoke(diceIndex, upFaces[diceIndex], previousUpFaces[diceIndex] ,rolling[diceIndex]);

        public void Throw(int numberOfDice, float velocity, float velocityVariation = 0)
        {
            // Set everything rolling
            rolling = Enumerable.Repeat(true, numberOfDice).ToArray();

            // Starting velocities
            velocities = rolling
                .Select(_ => velocity * (1 + (Random.value - 0.5f) * velocityVariation))
                .ToArray();

            // Setup next sample times
            sampleTimes = velocities
                .Select((_, idx) => GetNextSampleTime(idx))
                .ToArray();

            // Starting faces
            upFaces = velocities
                .Select(_ => Random.Range(1, 7))
                .ToArray();

            // Select where we are coming from based on what was up
            previousUpFaces = upFaces
                .Select(value => {
                    var options = ValueNeighbours(value).ToArray();
                    return options[Random.Range(0, options.Length)];
                })
                .ToArray();
            
            for (int i = 0; i<rolling.Length; i++)
            {
                EmitEvent(i);
            }
        }

        int GetNewFace(int diceIndex)
        {
            var velocity = velocities[diceIndex];

            // Return stay on same face
            if (Random.value < velocityToStayProbability.Evaluate(velocity))
            {
                return upFaces[diceIndex];
            }

            // Return rolling to either side;
            if (Random.value < velocityToSwirveProbability.Evaluate(velocity))
            {
                var invalid = new int[] { upFaces[diceIndex], GetOpposing(upFaces[diceIndex]), previousUpFaces[diceIndex], GetOpposing(previousUpFaces[diceIndex]) };
                var options = Faces.Where(v => !invalid.Contains(v)).ToArray();
                return options[Random.Range(0, options.Length)];
            }

            // Return opposing side of previous face;
            return GetOpposing(previousUpFaces[diceIndex]);
        }

        private void Update()
        {
            if (rolling == null) return;

            for (int diceIndex=0; diceIndex<rolling.Length; diceIndex++)
            {
                if (!rolling[diceIndex]) continue;

                velocities[diceIndex] *= (1 - Time.deltaTime * velocityDecay);

                if (Time.timeSinceLevelLoad < sampleTimes[diceIndex]) continue;

                var newFace = GetNewFace(diceIndex);

                if (newFace == upFaces[diceIndex])
                {
                    rolling[diceIndex] = false;

                    EmitEvent(diceIndex);
                    continue;
                }

                previousUpFaces[diceIndex] = upFaces[diceIndex];
                upFaces[diceIndex] = newFace;
                sampleTimes[diceIndex] = GetNextSampleTime(diceIndex);

                EmitEvent(diceIndex);
            }
        }

#if UNITY_EDITOR
        [SerializeField]
        int DebugDice;

        private void OnGUI()
        {
            if (DebugDice == 0) return;

            if (GUILayout.Button("Throw"))
            {
                Throw(DebugDice, Random.value * 0.5f + 0.5f, 0.1f);
            }
        }
    }
#endif
}
