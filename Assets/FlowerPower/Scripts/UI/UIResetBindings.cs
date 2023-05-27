using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FP
{
    public class UIResetBindings : MonoBehaviour
    {
        UIButton _Button;

        UIButton Button
        {
            get
            {
                if (_Button == null)
                {
                    _Button = GetComponent<UIButton>();
                }
                return _Button;
            }
        }

        private void OnEnable()
        {
            Button.OnClick += Button_OnClick;
        }

        private void OnDisable()
        {
            Button.OnClick -= Button_OnClick;
        }

        private void Button_OnClick(UIButton button)
        {
            GetComponentInParent<UISettings>().HandleResetBindings();
        }
    }
}
