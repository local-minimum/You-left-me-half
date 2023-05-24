using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DeCrawl.Primitives;

namespace FP {
    public class LetterLoot : Lootable
    {
        public static readonly char NoLetterChar = '\n';
        public static string AsId(char letter) => $"Letter-{letter}";
        public static char AsChar(string id) => id.StartsWith("Letter-") ? id[7] : NoLetterChar;

        public char Letter => AsChar(Id);
    }
}
