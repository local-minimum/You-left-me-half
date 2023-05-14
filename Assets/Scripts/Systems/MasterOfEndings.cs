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
            CurrencyTracker.OnChange += CurrencyTracker_OnChange;
        }

        private void OnDisable()
        {
            CurrencyTracker.OnChange -= CurrencyTracker_OnChange;
        }

        private void CurrencyTracker_OnChange(CurrencyType type, int available, int capacity)
        {
            if (type != CurrencyType.Health) return;
            if (capacity == 0)
            {
                OnEnding?.Invoke(EndingType.Death, Ending.NoHealthCanister);
                Game.Status = GameStatus.GameOver;

            }
            else if (available == 0)
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