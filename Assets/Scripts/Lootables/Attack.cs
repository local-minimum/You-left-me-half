using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DeCrawl.Primitives;

public class Attack : Lootable
{
    public Texture2D textureProgress;
    public Texture2D textureOverlay;
    public Image.FillMethod fillMethod = Image.FillMethod.Radial360;
    public AttackStats attackStats;

}
