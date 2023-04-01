using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Math
{
    public static float ManhattanMagnitude(this Vector3 v) => Mathf.Abs(v.x) + Mathf.Abs(v.y) + Mathf.Abs(v.z);
}
