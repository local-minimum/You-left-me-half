using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DeCrawl.Systems;

namespace DeCrawl.UI
{
    public class CurrencyBar : MonoBehaviour
    {
        [SerializeField]
        Image fillImage;

        [SerializeField]
        bool CapacityIsFullWidthValue = false;

        [SerializeField]
        int FullWidthValue = 400;

        [SerializeField]
        CurrencyType type;

        [SerializeField]
        RectTransform barTransform;

        [SerializeField]
        TMPro.TextMeshProUGUI changeText;

        int Capacity;

        private void Update()
        {
            UpdateBar();
        }

        private void OnEnable()
        {
            CurrencyTracker.OnChange += CurrencyTracker_OnChange;
        }


        private void OnDisable()
        {
            CurrencyTracker.OnChange -= CurrencyTracker_OnChange;
        }
        
        void UpdateBar()
        {
            barTransform.anchorMax = new Vector2((float)Capacity / FullWidthValue, 1);
            barTransform.offsetMax = Vector2.zero;
        }

        int trackedAvailable = -1;

        [SerializeField]
        float showChangeTime = 1f;

        [SerializeField]
        Color gainColor;

        [SerializeField]
        Color lossColor;

        int lastChange = 0;
        IEnumerator<WaitForSeconds> ShowChange(int change)
        {
            if (change == 0) yield break;

            lastChange = change;
            changeText.color = change > 0 ? gainColor : lossColor;
            changeText.text = change > 0 ? $"+{change}" : change.ToString();

            yield return new WaitForSeconds(showChangeTime);

            if (lastChange == change)
            {
                changeText.text = "";
            }
        }

        private void CurrencyTracker_OnChange(CurrencyType type, int available, int capacity)
        {
            if (type != this.type) return;

            if (CapacityIsFullWidthValue)
            {
                Capacity = capacity;
                FullWidthValue = capacity;
            } else
            {
                Capacity = Mathf.Min(capacity, FullWidthValue);
            }

            fillImage.fillAmount = capacity == 0 ? 0 : Mathf.Min(1, (float)available / capacity);

            if (changeText != null && gameObject.activeSelf)
            {
                if (trackedAvailable > 0 && available > 0)
                {
                    StartCoroutine(ShowChange(available - trackedAvailable));
                }
                trackedAvailable = available;
            }
        }
    }
}