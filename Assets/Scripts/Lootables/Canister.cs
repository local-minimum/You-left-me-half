using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DeCrawl.Primitives;
using DeCrawl.Systems;

namespace YLHalf
{
    public class Canister : Lootable
    {
        public Texture2D textureProgress;

        public Texture2D textureOverlay;

        public CurrencyType CanisterType;

        [SerializeField]
        private int capacity = 128;

        [SerializeField]
        private int initialStored = 0;

        public Image.FillMethod fillMethod = Image.FillMethod.Horizontal;

        public int Capacity { get { return capacity; } }

        private int stored = 0;
        bool ready = false;

        public int Stored { get { return ready ? stored : initialStored; } }

        public float Fill
        {
            get
            {
                return Mathf.Clamp01((float)stored / capacity);
            }
        }

        public bool Receive(int amount, out int remaining)
        {
            remaining = Mathf.Max(0, stored + amount - capacity);
            stored = Mathf.Min(Capacity, stored + amount);

            return remaining > 0;
        }

        public bool Withdraw(int amount, out int remaining)
        {
            remaining = Mathf.Max(0, amount - stored);
            stored = Mathf.Max(0, stored - amount);

            return remaining > 0;
        }

        private void Start()
        {
            stored = Mathf.Min(capacity, initialStored);
            ready = true;
        }
    }
}