using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DeCrawl.Primitives;

namespace FP
{
    public class LevelGridSize : FindingSingleton<LevelGridSize>
    {
        [SerializeField]
        int size = 3;

        public static int Size => instance.size;        
    }
}
