using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DeCrawl.Systems;

namespace FP
{
    public class UIMenuSpawner : MonoBehaviour
    {
        [SerializeField]
        UIMenuSystem Prefab;

        UIMenuSystem _system;

        UIMenuSystem System
        {
            get
            {
                if (_system == null)
                {
                    _system = Instantiate(Prefab);
                }
                return _system;
            }
        }

        private void OnEnable()
        {
            DungeonInput.OnInput += DungeonInput_OnInput;
        }

        private void OnDisable()
        {
            DungeonInput.OnInput -= DungeonInput_OnInput;
        }

        void ShowInventory(DungeonInput.InputType type)
        {
            System.state = UIMenuSystem.State.Inventory;
            System.GetComponentInChildren<UIInventory>().Configure(
                PlayerController.instance.GetComponentInChildren<Inventory>()
            );
        }

        private void DungeonInput_OnInput(DungeonInput.InputEvent input, DungeonInput.InputType type)
        {
            if (Game.Status != GameStatus.Playing || !DungeonInput.OverlappingTypes(type, DungeonInput.InputType.Down)) return;

            if (input == DungeonInput.InputEvent.Inventory)
            {
                ShowInventory(type);
                return;
            }

            if (input == DungeonInput.InputEvent.Pause || input == DungeonInput.InputEvent.Abort)
            {
                System.state = UIMenuSystem.State.Main;
            }
        }
    }
}
