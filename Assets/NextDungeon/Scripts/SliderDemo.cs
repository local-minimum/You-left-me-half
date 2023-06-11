using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ND {
    public class SliderDemo : MonoBehaviour
    {
        [MinMaxRange(0, 999)]
        public IntRange selection;
    }
}