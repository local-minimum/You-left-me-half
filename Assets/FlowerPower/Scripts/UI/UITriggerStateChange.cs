using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FP
{
    public class UITriggerStateChange : MonoBehaviour
    {
        [SerializeField]
        UIButton button;

        [SerializeField]
        UIMenuSystem.State TriggerState;

        [SerializeField]
        UIMenuSystem _menuSystem;

        UIMenuSystem MenuSystem
        {
            get
            {
                if (_menuSystem == null)
                {
                    _menuSystem = GetComponentInParent<UIMenuSystem>();
                }
                return _menuSystem;
            }
        }

        private void OnEnable()
        {
            if (button == null)
            {
                button = GetComponent<UIButton>();
            }
            button.OnClick += Button_OnClick;
        }

        private void OnDisable()
        {
            if (button)
            {
                button.OnClick -= Button_OnClick;
            }
        }


        private void Button_OnClick(UIButton _)
        {
            MenuSystem.state = TriggerState;
        }
    }
}
