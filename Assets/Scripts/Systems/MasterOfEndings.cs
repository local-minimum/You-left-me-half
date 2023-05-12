using System.Collections.Generic;
using UnityEngine;
using DeCrawl.Primitives;
using DeCrawl.Systems;

namespace YLHalf
{
    public enum EndingType { Death };
    public enum Ending { NoHealth, NoHealthCanister, LostConnection }

    public delegate void EndingEvent(EndingType type, Ending ending);

    public class MasterOfEndings : FindingSingleton<MasterOfEndings>
    {

        public static event EndingEvent OnEnding;

        private void OnEnable()
        {
            Inventory.OnCanisterChange += Inventory_OnCanisterChange;
        }

        private void OnDisable()
        {
            Inventory.OnCanisterChange -= Inventory_OnCanisterChange;
        }

        private void Inventory_OnCanisterChange(CanisterType type, int stored, int capacity)
        {
            if (type != CanisterType.Health) return;
            if (capacity == 0)
            {
                OnEnding?.Invoke(EndingType.Death, Ending.NoHealthCanister);
                Game.Status = GameStatus.GameOver;

            }
            else if (stored == 0)
            {
                OnEnding?.Invoke(EndingType.Death, Ending.NoHealth);
                Game.Status = GameStatus.GameOver;
            }
        }

        public void TriggerDisconnect()
        {
            OnEnding?.Invoke(EndingType.Death, Ending.LostConnection);
            Game.Status = GameStatus.GameOver;
        }
    }
}