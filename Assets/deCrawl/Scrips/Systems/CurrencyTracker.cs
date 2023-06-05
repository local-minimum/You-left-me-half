using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DeCrawl.Utils;

namespace DeCrawl.Systems
{
    public enum CurrencyType { Money, Health, XP, BossHealth }
    public static class CurrencyTracker
    {
        public delegate void ChangeEvent(CurrencyType type, int available, int capacity);

        public static event ChangeEvent OnChange;

        private static Dictionary<CurrencyType, int> Capacity = new Dictionary<CurrencyType, int>();

        /// <summary>
        /// Updates capacity and uses stored availability 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="capacity"></param>
        public static void UpdateCapacity(CurrencyType type, int capacity)
        {
            Capacity[type] = capacity;
            OnChange?.Invoke(type, Mathf.Min(Available.GetValueOrDefault(type, 0), capacity), capacity);
        }

        private static Dictionary<CurrencyType, int> Available = new Dictionary<CurrencyType, int>();

        public static void AddAvailable(CurrencyType type, int amount) => SubtractAvailable(type, -amount);
        public static void SubtractAvailable(CurrencyType type, int amount)
        {
            var available = Mathf.Max(0, Available.GetValueOrDefault(type, 0) - amount);
            // Debug.Log($"{type}: {Available[type]} => {available}");
            Available[type] = available;
            
            OnChange?.Invoke(type, available, Capacity.GetValueOrDefault(type, available));
        }

        /// <summary>
        /// Sets available amount, if capacity hasn't been regisered then it's assumed to be same as available
        /// </summary>
        /// <param name="type"></param>
        /// <param name="available"></param>
        public static void UpdateAvailable(CurrencyType type, int available)
        {
            Available[type] = available;
            OnChange?.Invoke(type, available, Capacity.GetValueOrDefault(type, available));            
        }

        /// <summary>
        /// Update both parameters for type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="available"></param>
        /// <param name="capacity"></param>
        public static void Update(CurrencyType type, int available, int capacity)
        {
            Available[type] = available;
            Capacity[type] = capacity;
            OnChange?.Invoke(type, Mathf.Min(available, capacity), capacity);
        }

        public static void ReEmit(CurrencyType type)
        {
            OnChange?.Invoke(type, Available.GetValueOrDefault(type, 0), Capacity.GetValueOrDefault(type, 0));
        }

        
        [System.Serializable]
        private struct Purse
        {
            public CurrencyType Currency;
            public int Available;
            public int Capacity;

            public Purse(CurrencyType currency, int available, int capacity)
            {
                Currency = currency;
                Available = available;
                Capacity = capacity;
            }
        }

        [System.Serializable]
        private struct CurrenciesDto
        {
            public Purse[] Purses;

            public CurrenciesDto(Dictionary<CurrencyType, int> available, Dictionary<CurrencyType, int> capacity)
            {
                var currencies = System.Enum.GetValues(typeof(CurrencyType)) as CurrencyType[];

                Purses = currencies
                    .Select(currency => new Purse(
                        currency,
                        available.GetValueOrDefault(currency, 0),
                        capacity.GetValueOrDefault(currency, available.GetValueOrDefault(currency, 0))
                    ))
                    .ToArray();
            }
        }

        public static string SerializeState() => JsonUtility.ToJson(new CurrenciesDto(Available, Capacity));

        public static void DeserializeState(string json)
        {
            if (json == null)
            {
                Debug.LogError("No currency information stored, game state corrupt");
                return;
            }

            var purse = InterfaceFinder.FindMonoBehaviourWithICurrencyPurse();
            if (purse == null)
            {
                Debug.LogWarning("Could not restore currencies because nothing implements a purse");
                return;
            }

            var state = JsonUtility.FromJson<CurrenciesDto>(json);

            foreach (var currency in state.Purses)
            {
                if (currency.Available == 0 && currency.Capacity == 0)
                {
                    Debug.Log($"Skipping Currency Restore: {currency.Currency} since available and capacity is zero");
                    continue;
                }
                Debug.Log($"Currency Restore: {currency.Currency} set to {currency.Available} / {currency.Capacity}");
                purse?.SetCurrencyHeld(currency.Currency, currency.Available, currency.Capacity);
                Update(currency.Currency, currency.Available, currency.Capacity);
            }
           
        }
        
    }
}
