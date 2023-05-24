using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DeCrawl.Primitives;

namespace FP {
    public class LetterLoot : Lootable
    {
        public static string AsId(char letter) => $"Letter-{letter}";
    }
}
