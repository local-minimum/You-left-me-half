using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ND
{
    public class MinMaxRangeAttribute : PropertyAttribute
    {
        public int minValue { get; protected set; }
        public int maxValue { get; protected set; }

        public MinMaxRangeAttribute(int minValue, int maxValue)
        {
            this.minValue = minValue;
            this.maxValue = maxValue;
        }
    }
}
