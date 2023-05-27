using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DeCrawl.Systems;

namespace FP {
    public class UISettings : MonoBehaviour, IUIMenuView
    {
        public UIMenuSystem.State State => UIMenuSystem.State.Settings;

        public bool KeyBinding { get; set; }
        public bool Active { set { gameObject.SetActive(value); } }

        UIMenuSystem menuSystem;

        private void OnEnable()
        {
            DungeonInput.OnInput += DungeonInput_OnInput;
        }

        private void OnDisable()
        {
            DungeonInput.OnInput -= DungeonInput_OnInput;
        }

        private void Start()
        {
            menuSystem = GetComponentInParent<UIMenuSystem>();
        }

        private void DungeonInput_OnInput(DungeonInput.InputEvent input, DungeonInput.InputType type)
        {
            if (!DungeonInput.OverlappingTypes(type, DungeonInput.InputType.Down) || KeyBinding) return;

            if (input == DungeonInput.InputEvent.Abort)
            {
                menuSystem.state = UIMenuSystem.State.Main;
            }
        }
    }
}