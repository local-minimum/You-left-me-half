using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DeCrawl.Systems
{
    public enum CurrencyType { Money, Health, XP }
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
    }
}