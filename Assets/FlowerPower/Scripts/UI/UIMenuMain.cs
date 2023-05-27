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

        UIButton selectedButton;
        UIButton hoveredButton;       

        UIButton CurrentOption
        {
            get => hoveredButton ?? selectedButton;
        }

        List<UIButton> _Buttons;

        List<UIButton> Buttons
        {
            get
            {
                if (_Buttons == null)
                {
                    _Buttons = new List<UIButton>();
                    _Buttons.AddRange(GetComponentsInChildren<UIButton>(true));
                }
                return _Buttons;
            }
        }


        private void OnEnable()
        {
            DungeonInput.OnInput += DungeonInput_OnInput;

            bool autoSelectFirstButton = selectedButton == null;

            foreach (var button in Buttons)
            {
                button.Disabled = DisabledOptions.Contains(button);
                button.OnHover += Button_OnHover;
                button.OnClick += Button_OnClick;

                if (autoSelectFirstButton)
                {
                    selectedButton = button;
                    button.Hovered = true;
                    autoSelectFirstButton = false;
                } else
                {
                    selectedButton.Hovered = true;
                }
            }
        }

        private void OnDisable()
        {
            DungeonInput.OnInput -= DungeonInput_OnInput;
            foreach (var button in Buttons)
            {
                button.OnHover -= Button_OnHover;
                button.OnClick -= Button_OnClick;
            }
        }

        private void Button_OnClick(UIButton button)
        {
            selectedButton = button;
            hoveredButton = null;
        }

        private void Button_OnHover(UIButton button)
        {
            // Debug.Log($"Btn {button} {button.Hovered} / Sel {selectedButton} / Hov {hoveredButton}");

            // Set hover option if needed
            if (button.Hovered)
            {
                hoveredButton = button;
            }

            // Refuse deselecting the selected if no hover
            if (selectedButton == button && hoveredButton == button && !button.Hovered)
            {
                // Debug.Log("Force hover selected");
                button.Hovered = true;
            }

            // Ignore if it's just cleaning up other button
            if (hoveredButton != button && !button.Hovered)
            {
                // Debug.Log("Ignored");
                return;
            }

            if (!button.Hovered)
            {
                hoveredButton = null;
            }

            // Clean up hover state of 
            if (hoveredButton && selectedButton != button && selectedButton?.Hovered == true)
            {
                // Debug.Log("Cleanup selected hovered");
                selectedButton.Hovered = false;
                return;
            } 
            
            // Fallback to previous selected without pointer            
            if (selectedButton?.Hovered != true && selectedButton != button)
            {
                // Debug.Log("Fallback hover selected");
                selectedButton.Hovered = true;
            }

            // Debug.Log("No action");
        }

        void ShiftSelection(int amount)
        {
            var buttons = Buttons;
            var selectedIndex = buttons.IndexOf(selectedButton);
            var nButtons = buttons.Count;

            for (int i=selectedIndex + amount; true; i+=amount)
            {
                var realIndex = (i + nButtons) % nButtons;
                if (realIndex == selectedIndex) return;

                var button = buttons[realIndex];
                if (!button.Disabled)
                {
                    // If we had mouse interaction ignore it
                    CurrentOption.Hovered = false;

                    // Keep track to clear it once it's no longer selected
                    var previousSelected = selectedButton;

                    // Set new state
                    selectedButton = button;
                    hoveredButton = null;

                    // Sync with state
                    previousSelected.Hovered = false;
                    button.Hovered = true;

                    break;
                }
            }
        }

        private void DungeonInput_OnInput(DungeonInput.InputEvent input, DungeonInput.InputType type)
        {
            if (!DungeonInput.OverlappingTypes(type, DungeonInput.InputType.Down)) return;

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
            else if (input == DungeonInput.InputEvent.MoveForward)
            {
                ShiftSelection(-1);
            }
            else if (input == DungeonInput.InputEvent.MoveBackwards)
            {
                ShiftSelection(1);
            }
        }
    }
}
