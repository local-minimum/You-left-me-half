using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VectorMath
{
    public static float ManhattanMagnitude(this Vector3 v) => 
        Mathf.Abs(v.x) + Mathf.Abs(v.y) + Mathf.Abs(v.z);
    public static int ManhattanMagnitude(this Vector3Int v) => 
        Mathf.Abs(v.x) + Mathf.Abs(v.y) + Mathf.Abs(v.z);

    public static int CheckerDitsance(this Vector3Int v) => 
        Mathf.Max(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));

    public static Vector2Int XY(this Vector3Int v) => new Vector2Int(v.x, v.y);

    public static Vector2Int XZ(this Vector3Int v) => new Vector2Int(v.z, v.z);

    public static Vector3Int XY(this Vector2Int v) => new Vector3Int(v.x, v.y);

    public static Vector3Int XZ(this Vector2Int v) => new Vector3Int(v.x, 0, v.y);

    public static (int, int) XZTuple(this Vector3Int v) => (v.x, v.z);

    public static int ManhattanDistance(this (int, int) a, (int, int) b) => 
        Mathf.Abs(a.Item1 - b.Item1) + Mathf.Abs(a.Item2 - b.Item2);

    public static (int, int) StepNorth(this (int, int) c) => (c.Item1, c.Item2 - 1);
    public static (int, int) StepSouth(this (int, int) c) => (c.Item1, c.Item2 + 1);
    public static (int, int) StepWest(this (int, int) c) => (c.Item1 - 1, c.Item2);
    public static (int, int) StepEast(this (int, int) c) => (c.Item1 + 1, c.Item2);

    public static IEnumerable<(int, int)> Neighbours(this (int, int) c)
    {
        yield return c.StepNorth();
        yield return c.StepWest();
        yield return c.StepSouth();
        yield return c.StepEast();
    }

    public static bool InGrid<T>(this (int, int) c, T[,] grid) =>
        c.Item1 >= 0 && c.Item2 >= 0 && c.Item1 < grid.GetLength(0) && c.Item2 < grid.GetLength(1);
}
