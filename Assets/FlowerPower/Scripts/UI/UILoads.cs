using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DeCrawl.Systems;

namespace FP
{
    public class UILoads : MonoBehaviour, IUIMenuView
    {
        public UIMenuSystem.State State => UIMenuSystem.State.Load;

        public bool Active { set => gameObject.SetActive(value); }

        UIMenuSystem menuSystem;

        private void Start()
        {
            menuSystem = GetComponentInParent<UIMenuSystem>();
        }

        private void OnEnable()
        {
            var disabled = GetComponentsInChildren<UILoadSlot>(true)
                .Where(slot => !slot.HasSave)
                .Select(slot => slot.GetComponent<UIButton>());

            GetComponent<UIButtonList>().SetDisabled(disabled);

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