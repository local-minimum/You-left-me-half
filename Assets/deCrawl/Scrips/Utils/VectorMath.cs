using System.Collections.Generic;
using UnityEngine;

namespace DeCrawl.Utils
{
    using TupleIntXZ = System.ValueTuple<int, int>;
    using TupleInt2D = System.ValueTuple<int, int>;

    public static class VectorMath
    {       
        #region Magnitudes
        public static float ManhattanMagnitude(this Vector3 v) =>
            Mathf.Abs(v.x) + Mathf.Abs(v.y) + Mathf.Abs(v.z);
        public static int ManhattanMagnitude(this Vector3Int v) =>
            Mathf.Abs(v.x) + Mathf.Abs(v.y) + Mathf.Abs(v.z);

        public static int CheckerMagnitude(this Vector3Int v) =>
            Mathf.Max(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
        #endregion

        #region Distances
        public static int ManhattanDistance(this TupleInt2D a, TupleInt2D b) =>
            Mathf.Abs(a.Item1 - b.Item1) + Mathf.Abs(a.Item2 - b.Item2);
        #endregion

        #region Coordinates Convertions
        public static Vector2Int XYVector2Int(this Vector3Int v) => new Vector2Int(v.x, v.y);

        public static Vector2Int XZVector2Int(this Vector3Int v) => new Vector2Int(v.z, v.z);

        public static Vector3Int XYVector3Int(this Vector2Int v) => new Vector3Int(v.x, v.y);

        public static Vector3Int XZVector3Int(this Vector2Int v) => new Vector3Int(v.x, 0, v.y);

        public static Vector3Int XZVector3Int(this (int, int) t) => new Vector3Int(t.Item1, 0, t.Item2);

        public static TupleIntXZ XZTuple(this Vector3Int v) => (v.x, v.z);
        #endregion

        #region Cardinal Translations in XZ Plane
        public static TupleIntXZ StepNorth(this TupleIntXZ c) => (c.Item1, c.Item2 - 1);
        public static TupleIntXZ StepSouth(this TupleIntXZ c) => (c.Item1, c.Item2 + 1);
        public static TupleIntXZ StepWest(this TupleIntXZ c) => (c.Item1 - 1, c.Item2);
        public static TupleIntXZ StepEast(this TupleIntXZ c) => (c.Item1 + 1, c.Item2);
        #endregion

        #region Neighbours in XZ Plane

        public static IEnumerable<TupleIntXZ> Neighbours(this TupleIntXZ c)
        {
            yield return c.StepNorth();
            yield return c.StepWest();
            yield return c.StepSouth();
            yield return c.StepEast();
        }
        #endregion

        #region Arithmatic
        public static TupleInt2D Add(this TupleInt2D left, TupleInt2D right) => (left.Item1 + right.Item1, left.Item2 + right.Item2);
        public static TupleInt2D Add(this TupleInt2D left, Vector3Int right) => (left.Item1 + right.x, left.Item2 + right.z);
        public static TupleInt2D Subtract(this TupleInt2D left, TupleInt2D right) => (left.Item1 - right.Item1, left.Item2 - right.Item2);
        public static TupleInt2D Subtract(this TupleInt2D left, Vector3Int right) => (left.Item1 - right.x, left.Item2 - right.z);
        #endregion

        #region Floor
        public static TupleInt2D Floor(this (float, float) coords) => (Mathf.FloorToInt(coords.Item1), Mathf.FloorToInt(coords.Item2));

        public static TupleInt2D TupleInt2D(float first, float second) => (Mathf.FloorToInt(first), Mathf.FloorToInt(second));

        #endregion
    }
}