using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DeCrawl.Gadgets;

namespace Experiments {
    public class UIDiceVisualizer : MonoBehaviour
    {
        [SerializeField, Range(1, 20)]
        int dice;

        [SerializeField]
        MarkowDice Dice;

        [SerializeField]
        AnimationCurve throwDurationToVelocity;

        [SerializeField, Range(0, 1)]
        float velocityVariability = 0.1f;

        private void OnEnable()
        {
            Dice.OnDiceChange += Dice_OnDiceChange;    
        }

        private void OnDisable()
        {
            Dice.OnDiceChange -= Dice_OnDiceChange;
        }

        int[] values;
        bool[] rolling;

        private void Dice_OnDiceChange(int diceIndex, int value, int previousValue, bool rolling)
        {
            this.values[diceIndex] = value;
            this.rolling[diceIndex] = rolling;
            UpdateText();
        }

        [SerializeField]
        TMPro.TextMeshProUGUI TextUI;

        void UpdateText()
        {
            string text = "";

            for (int i = 0; i<values.Length; i++)
            {
                var v = values[i];
                text += rolling[i] ? $" {v}  " : $":{v}: ";
            }
            TextUI.text = text;
        }

        bool started = false;
        float t0;
        private void Update()
        {
            if (Input.anyKeyDown)
            {
                t0 = Time.timeSinceLevelLoad;
                started = true;
            } else if (started && !Input.anyKey)
            {
                var duration = Time.timeSinceLevelLoad - t0;
                var velocity = throwDurationToVelocity.Evaluate(duration);

                values = new int[dice];
                rolling = new bool[dice];

                Dice.Throw(dice, velocity, velocityVariability);
                started = false;
            }
        }
    }
}