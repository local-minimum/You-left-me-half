using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FP
{
    public class LetterLane : MonoBehaviour
    {
        [SerializeField]
        float startOutside = 0.2f;

        [SerializeField]
        float killOutside = 0.1f;

        [SerializeField, Range(0, 1)]
        float speed = 0.2f;

        [SerializeField, Range(0, 1)]
        float correctLetterProbability = 0.3f;

        char[] alternativeLetters;
        char letter = 'G';

        [SerializeField]
        TMPro.TextMeshProUGUI TargetLetter;

        [SerializeField]
        TMPro.TextMeshProUGUI SlidingLetter;

        [SerializeField]
        float upSlideProbability = 0.5f;

        [SerializeField]
        float BetweenMaxTime = 0.5f;

        [SerializeField]
        Color goodZoneColor;

        [SerializeField]
        Color badZoneColor;

        private void OnEnable()
        {
            TargetLetter.text = letter.ToString();
            SlidingLetter.text = "";
        }

        bool sliding = false;
        float spawnOffset;
        float direction;
        float startTime = -1;
        float killPosition;

        public bool ShowingCorrectLetter { get; private set; }

        public void Configure(char letter, char[] alternativeLetters)
        {
            this.letter = letter;
            this.alternativeLetters = alternativeLetters;
            TargetLetter.text = letter.ToString();
            SlidingLetter.text = "";
        }

        char RandomLetter
        {
            get
            {
                if (Random.value < correctLetterProbability)
                {
                    return letter;
                }
                return alternativeLetters[Random.Range(0, alternativeLetters.Length)];
            }
        }

        char currentLetter;

        void SpawnLetter()
        {
            var letter = RandomLetter;
            ShowingCorrectLetter = letter == this.letter;

            spawnOffset = Random.value < upSlideProbability ? -startOutside : 1 + startOutside;
            direction = spawnOffset < 0.5f ? 1f : -1f;
            killPosition = spawnOffset < 0.5f ? 1f + killOutside : -killOutside;

            startTime = Time.timeSinceLevelLoad;

            SetSlider();

            currentLetter = letter;
            SlidingLetter.text = letter.ToString();
            sliding = true;
        }

        float yPosition;

        bool InLetterZone => yPosition >= 0.4 && yPosition <= 0.6;
  

        void SetSlider()
        {
            yPosition = spawnOffset + direction * (Time.timeSinceLevelLoad - startTime) * speed;
            var offset = new Vector2(0, yPosition);
            SlidingLetter.rectTransform.anchorMin = offset;
            offset.x = 1;
            SlidingLetter.rectTransform.anchorMax = offset;

            SlidingLetter.color = InLetterZone == ShowingCorrectLetter ? goodZoneColor : badZoneColor;

            if (direction > 0)
            {
                sliding = yPosition < killPosition;
            }
            else
            {
                sliding = yPosition > killPosition;
            }

            if (!sliding)
            {
                startTime = -1;
            }
        }

        private void Update()
        {
            if (sliding)
            {
                SetSlider();
            }
            else if (startTime < 0)
            {
                startTime = Time.timeSinceLevelLoad + Random.value * BetweenMaxTime;
            }
            else if (Time.timeSinceLevelLoad > startTime)
            {
                SpawnLetter();
            }
        }

        public bool Handle(char ch, bool asGood)
        {
            if (ch != currentLetter || !sliding) return false;

            var letterZone = InLetterZone;

            if (asGood)
            {
                if (letterZone == ShowingCorrectLetter)
                {
                    startTime = -1;
                    Debug.Log($"Good clear {currentLetter}");
                    return true;
                }
            } else
            {
                if (letterZone != ShowingCorrectLetter)
                {
                    startTime = -1;
                    Debug.Log($"Bad clear {currentLetter}");
                    return true;
                }
            }

            return false;
        }
    }
}
