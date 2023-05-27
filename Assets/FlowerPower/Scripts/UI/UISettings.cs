using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DeCrawl.Systems;

namespace FP {
    public class UISettings : MonoBehaviour, IUIMenuView
    {
        public UIMenuSystem.State State => UIMenuSystem.State.Settings;

        [SerializeField]
        UIButton ResetButton;

        public bool HasCustomBindings
        {
            set
            {
                if (ResetButton)
                {
                    ResetButton.Disabled = !value;
                }
            }
        }

        public bool KeyBinding { get; set; }
        public bool Active { set { gameObject.SetActive(value); } }

        UIMenuSystem menuSystem;

        private void OnEnable()
        {
            DungeonInput.OnInput += DungeonInput_OnInput;
            HasCustomBindings = DungeonInput.instance.HasCustomBindings;
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

        public void HandleResetBindings() {
            DungeonInput.instance.ResetKeyBindings();
            foreach (var keybinding in GetComponentsInChildren<UIKeyBinding>())
            {
                keybinding.SyncKey();
            }
            HasCustomBindings = false;
        }
    }
}