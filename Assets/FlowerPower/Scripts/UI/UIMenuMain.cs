using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DeCrawl.Systems;

namespace FP
{
    public class UIMenuMain : MonoBehaviour, IUIMenuView
    {
        public UIMenuSystem.State State => UIMenuSystem.State.Main;

        public bool Active { set => gameObject.SetActive(value); }

        UIMenuSystem menuSystem;

        private void Start()
        {
            menuSystem = GetComponentInParent<UIMenuSystem>();
        }

        private void OnEnable()
        {
            DungeonInput.OnInput += DungeonInput_OnInput;
        }

        private void OnDisable()
        {
            DungeonInput.OnInput -= DungeonInput_OnInput;
        }

        private void DungeonInput_OnInput(DungeonInput.InputEvent input, DungeonInput.InputType type)
        {
            if (menuSystem.state != UIMenuSystem.State.Main) return;

            if (
                (input == DungeonInput.InputEvent.Abort || input == DungeonInput.InputEvent.Pause) &&
                DungeonInput.OverlappingTypes(type, DungeonInput.InputType.Down)
            )
            {
                menuSystem.state = UIMenuSystem.State.Hidden;
            }
        }

        public void HandleClickResume()
        {
            menuSystem.state = UIMenuSystem.State.Hidden;
        }

        public void HandleClickSettings()
        {
            menuSystem.state = UIMenuSystem.State.Settings;
        }

        public void HandleClickAbout()
        {
            menuSystem.state = UIMenuSystem.State.About;
        }
    }
}
