using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VectorMath
{
    public static float ManhattanMagnitude(this Vector3 v) => Mathf.Abs(v.x) + Mathf.Abs(v.y) + Mathf.Abs(v.z);
    public static int ManhattanMagnitude(this Vector3Int v) => Mathf.Abs(v.x) + Mathf.Abs(v.y) + Mathf.Abs(v.z);

    public static int CheckerDitsance(this Vector3Int v) => Mathf.Max(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));

    public static Vector2Int XY(this Vector3Int v) => new Vector2Int(v.x, v.y);

    public static Vector2Int XZ(this Vector3Int v) => new Vector2Int(v.z, v.z);

    public static Vector3Int XY(this Vector2Int v) => new Vector3Int(v.x, v.y);

    public static Vector3Int XZ(this Vector2Int v) => new Vector3Int(v.x, 0, v.y);
}
