using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DeCrawl.Systems;

namespace FP
{
    public class UIMenuMain : MonoBehaviour, IUIMenuView
    {
        public UIMenuSystem.State State => UIMenuSystem.State.Main;

        public List<UIButton> DisabledOptions = new List<UIButton>();

        public bool Active { set => gameObject.SetActive(value); }

        UIMenuSystem menuSystem;

        private void Start()
        {
            menuSystem = GetComponentInParent<UIMenuSystem>();
        }

        UIButton selectedOption;
        UIButton hoverOption;       

        UIButton CurrentOption
        {
            get => hoverOption ?? selectedOption;
        }

        private void OnEnable()
        {
            DungeonInput.OnInput += DungeonInput_OnInput;

            bool autoSelectFirstButton = selectedOption == null;

            foreach (var button in GetComponentsInChildren<UIButton>(true))
            {
                if (autoSelectFirstButton)
                {
                    selectedOption = button;
                    button.Hovered = true;
                    autoSelectFirstButton = false;
                }
                button.Disabled = DisabledOptions.Contains(button);
                button.OnHover += Button_OnHover;
                button.OnClick += Button_OnClick;
            }
        }

        private void OnDisable()
        {
            DungeonInput.OnInput -= DungeonInput_OnInput;
            foreach (var button in GetComponentsInChildren<UIButton>(true))
            {
                button.OnHover -= Button_OnHover;
                button.OnClick -= Button_OnClick;
            }
        }

        private void Button_OnClick(UIButton button)
        {
            selectedOption = button;
        }

        private void Button_OnHover(UIButton button)
        {
            // Debug.Log($"Btn {button} {button.Hovered} / Sel {selectedOption} / Hov {hoverOption}");

            // Set hover option if needed
            if (button.Hovered)
            {
                hoverOption = button;
            }

            // Refuse deselecting the selected if no hover
            if (selectedOption == button && hoverOption == button && !button.Hovered)
            {
                // Debug.Log("Force hover selected");
                button.Hovered = true;
            }

            // Ignore if it's just cleaning up other button
            if (hoverOption != button && !button.Hovered)
            {
                // Debug.Log("Ignored");
                return;
            }

            if (!button.Hovered)
            {
                hoverOption = null;
            }

            // Clean up hover state of 
            if (hoverOption && selectedOption != button && selectedOption?.Hovered == true)
            {
                // Debug.Log("Cleanup selected hovered");
                selectedOption.Hovered = false;
                return;
            } 
            
            // Fallback to previous selected without pointer            
            if (selectedOption?.Hovered != true && selectedOption != button)
            {
                // Debug.Log("Fallback hover selected");
                selectedOption.Hovered = true;
            }

            // Debug.Log("No action");
        }

        private void DungeonInput_OnInput(DungeonInput.InputEvent input, DungeonInput.InputType type)
        {
            if (menuSystem.state != UIMenuSystem.State.Main || !DungeonInput.OverlappingTypes(type, DungeonInput.InputType.Down)) return;

            if (
                (input == DungeonInput.InputEvent.Abort || input == DungeonInput.InputEvent.Pause)
            )
            {
                menuSystem.state = UIMenuSystem.State.Hidden;
            }
            else if (input == DungeonInput.InputEvent.Select)
            {
                CurrentOption?.HandleClick();
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
