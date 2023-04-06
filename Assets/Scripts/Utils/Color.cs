using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ColorExtensions
{
    public static Color Clone(this Color color) => new Color(color.r, color.g, color.b, color.a);
    public static Color Clone(this Color color, float alpha) => new Color(color.r, color.g, color.b, alpha);

}
