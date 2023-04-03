using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Math
{
    public static float ManhattanMagnitude(this Vector3 v) => Mathf.Abs(v.x) + Mathf.Abs(v.y) + Mathf.Abs(v.z);

    public static Vector2Int XY(this Vector3Int v) => new Vector2Int(v.x, v.y);

    public static Vector2Int XZ(this Vector3Int v) => new Vector2Int(v.z, v.z);
}
