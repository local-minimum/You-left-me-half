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
        }
    }
}