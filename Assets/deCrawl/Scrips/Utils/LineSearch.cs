using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DeCrawl.Utils
{
    using Tuple2D = System.ValueTuple<float, float>;
    using TupleInt2D = System.ValueTuple<int, int>;

    public static class LineSearch
    {
        public class SearchParameters
        {
            public Tuple2D Origin;
            public Tuple2D Target;
            public int MaxDepth;
            public System.Func<TupleInt2D, bool> Predicate;

            public SearchParameters(TupleInt2D origin, Vector3Int target, System.Func<TupleInt2D, bool> predicate, int maxDepth = -1)
            {
                Origin = origin;
                Target = target.XZTuple();
                Predicate = predicate;
                MaxDepth = maxDepth < 0 ? int.MaxValue : maxDepth;
            }
        }

        public static bool Search(SearchParameters searchParameters, out List<TupleInt2D> path)
        {
            var lastCoords = searchParameters.Origin.Floor();
            path = new List<TupleInt2D>() { lastCoords };

            // Differential
            var dx = searchParameters.Target.Item1 - searchParameters.Origin.Item1;
            var dz = searchParameters.Target.Item2 - searchParameters.Origin.Item2;

            // Increments
            var sx = Mathf.Sign(dx);
            var sz = Mathf.Sign(dz);

            // Segment Length
            dx = Mathf.Abs(dx);
            dz = Mathf.Abs(dz);
            var d = Mathf.Max(dx, dz);

            var r = (float)d / 2;
            var (x, z) = searchParameters.Origin;

            if (dx > dz)
            {
                for (var i = 0; i < d; i++)
                {
                    x += sx;
                    r += dz;
                    if (r >= dx)
                    {
                        z += sz;
                        r -= dx;
                    }

                    var current = VectorMath.TupleInt2D(x, z);
                    if (current != lastCoords)
                    {
                        path.Add(current);
                        lastCoords = current;
                    }
                }
            }
            else
            {
                for (var i = 0; i < d; i++)
                {
                    z += sz;
                    r += dx;
                    if (r >= dz)
                    {
                        z += sx;
                        r -= dz;
                    }
                }

                var current = VectorMath.TupleInt2D(x, z);
                if (current != lastCoords)
                {
                    path.Add(current);
                    lastCoords = current;
                }

            }

            return lastCoords == searchParameters.Target && path.All(coords => searchParameters.Predicate(coords));
        }
    }
}
