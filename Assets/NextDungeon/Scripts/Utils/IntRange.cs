using UnityEngine;

namespace ND
{
    public interface IRange<T>
    {
        public T lowerBound { get; set; }
        public T upperBound { get; set; }
    }

    [System.Serializable]
    public struct IntRange: IRange<int>
    {
        [SerializeField]
        private int _lowerBound;

        [SerializeField]
        private int _upperBound;

        public int lowerBound { get => _lowerBound; set { _lowerBound = value; } }
        public int upperBound { get => _upperBound; set { _upperBound = value; } }
    }
}