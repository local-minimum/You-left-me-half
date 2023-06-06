using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DeCrawl.Systems;

namespace FP
{
    public class UISaves : MonoBehaviour, IUIMenuView
    {
        public UIMenuSystem.State State => UIMenuSystem.State.Save;

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
            if (!DungeonInput.OverlappingTypes(type, DungeonInput.InputType.Down)) return;

            if (input == DungeonInput.InputEvent.Abort)
            {
                menuSystem.state = UIMenuSystem.State.Main;
            }
        }
    }
}