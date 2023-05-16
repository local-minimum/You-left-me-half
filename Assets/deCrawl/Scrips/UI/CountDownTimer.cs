using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DeCrawl.UI
{
    public class CountDownTimer : MonoBehaviour
    {
        public delegate void TimesUpEvent();

        public event TimesUpEvent OnTimesUp;

        [SerializeField]
        TMPro.TextMeshProUGUI TextField;

        [SerializeField]
        Color startColor;

        [SerializeField]
        Color endColor;

        [SerializeField]
        float blinkLastSeconds = 5;

        [SerializeField]
        float blinkDuration = 0.5f;

        [SerializeField]
        int blinkShowRatio = 3;

        bool running = false;
        float duration;
        float timerStart;

        public void StartCountDown(float duration)
        {
            this.duration = duration;
            running = true;
            timerStart = Time.timeSinceLevelLoad;

            TextField.gameObject.SetActive(true);
            TextField.enabled = true;
        }

        public void StopCountDown(bool hideTextFieldGO = false)
        {
            running = false;
            if (hideTextFieldGO)
            {
                TextField.gameObject.SetActive(false);
            }
        }

        private void Update()
        {
            if (!running) return;

            var remaining = Mathf.Max(0, duration - (Time.timeSinceLevelLoad - timerStart));

            TextField.text = remaining.ToString("00.0");
            TextField.color = Color.Lerp(startColor, endColor, 1 - remaining / duration);

            if (remaining < blinkLastSeconds)
            {
                TextField.enabled = (Mathf.FloorToInt(blinkShowRatio * remaining / blinkDuration) % blinkShowRatio) != 0;
            }

            if (remaining == 0)
            {
                running = false;
            }
        }
    }
}
