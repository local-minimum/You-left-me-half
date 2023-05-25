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
        private void DungeonInput_OnInput(DungeonInput.InputEvent input, DungeonInput.InputType type)
        {
            if (input != DungeonInput.InputEvent.Inventory) return;
            if (DungeonInput.OverlappingTypes(type, DungeonInput.InputType.Down) && Game.Status == GameStatus.Playing)
            {
                System.state = UIMenuSystem.State.Inventory;
                System.GetComponentInChildren<UIInventory>().Configure(
                    PlayerController.instance.GetComponentInChildren<Inventory>()
                );
            }
        }
    }
}
