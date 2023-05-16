using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DeCrawl.Primitives;

namespace DeCrawl.Lootables
{
    public class ActionLoot : Lootable
    {
        public Texture2D textureProgress;
        public Texture2D textureOverlay;
        public Image.FillMethod fillMethod = Image.FillMethod.Radial360;
    }
}