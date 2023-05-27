using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DeCrawl.Systems;


namespace FP {
    public class UISettings : MonoBehaviour, IUIMenuView
    {
        public UIMenuSystem.State State => UIMenuSystem.State.Settings;

        [SerializeField]
        UIButton ResetButton;

        [SerializeField]
        TMPro.TextMeshProUGUI BindingWarnings;

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

        bool _KeyBinding = false;
        public bool KeyBinding { 
            get => _KeyBinding; 
            set {
                _KeyBinding = value;
                SyncBindingWarnings();
            } 
        }
        public bool Active { set { gameObject.SetActive(value); } }

        UIMenuSystem menuSystem;

        private void OnEnable()
        {
            DungeonInput.OnInput += DungeonInput_OnInput;
            HasCustomBindings = DungeonInput.instance.HasCustomBindings;
            SyncBindingWarnings();
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
            SyncBindingWarnings();
        }

        void SyncBindingWarnings()
        {
            if (BindingWarnings == null) return;

            var dupes = DungeonInput.instance.DuplicateBindings.ToArray();
            if (dupes.Length == 0)
            {
                BindingWarnings.text = "";
            } else if (dupes.Length == 1) {
                BindingWarnings.text = dupes[0].ToString();
            } else
            {
                BindingWarnings.text = $"There are {dupes.Length} binding conflicts, first: {dupes[0]}";
            }
        }
    }
}