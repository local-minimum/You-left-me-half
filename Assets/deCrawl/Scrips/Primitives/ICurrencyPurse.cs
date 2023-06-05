using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DeCrawl.Systems;

namespace DeCrawl.Primitives
{
    public interface ICurrencyPurse
    {
        public int Money { get; }
        public int XP { get;}
        public int Health { get; }

        /// <summary>
        /// Implementation that registers a currency change with CurrencyTracker
        /// </summary>
        /// <param name="type"></param>
        public void InvokeCurrencyChange(CurrencyType type);

        /// <summary>
        /// Receives amount of currency.
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="type"></param>
        /// <returns>Amount that could not be received</returns>
        public int Receive(int amount, CurrencyType type);

        /// <summary>
        /// Withdraws amount
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="type"></param>
        /// <returns>Amount that could not be withdrawn</returns>
        public int Withdraw(int amount, CurrencyType type);

        /// <summary>
        /// Set purse so that it contains exact amount.
        /// 
        /// NOTE: Should not update CurrencyTracker!
        /// </summary>        
        public void SetCurrencyHeld(CurrencyType type, int amount, int capacity);
    }
}
