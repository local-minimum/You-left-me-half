using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace YLHalf
{
    public class UIBar : MonoBehaviour
    {
        [SerializeField]
        Image fillImage;

        [SerializeField]
        int FullWidthValue = 400;

        [SerializeField]
        CanisterType type;

        [SerializeField]
        RectTransform barTransform;

        int Capacity;

        private void Update()
        {
            barTransform.anchorMax = new Vector2((float)Capacity / FullWidthValue, 1);
            barTransform.offsetMax = Vector2.zero;
        }

        private void OnEnable()
        {
            Inventory.OnCanisterChange += Inventory_OnCanisterChange;
        }

        private void OnDisable()
        {
            Inventory.OnCanisterChange -= Inventory_OnCanisterChange;
        }

        private void Inventory_OnCanisterChange(CanisterType type, int stored, int capacity)
        {
            if (type != this.type) return;
            Capacity = capacity;
            fillImage.fillAmount = capacity == 0 ? 0 : (float)stored / capacity;
        }
    }
}