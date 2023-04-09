using System.Collections.Generic;
using System.Linq;
using UnityEngine;

static public class GraphSearch
{
    public class SearchParameters
    {
        public (int, int) Origin;
        public (int, int) Target;
        public int MaxDepth;
        public bool[,] Map;

        public SearchParameters((int, int) origin, Vector3Int target, bool[,] map, int maxDepth)
        {
            Origin = origin;
            Target = target.XZTuple();
            Map = map;
            MaxDepth = maxDepth < 0 ? int.MaxValue : maxDepth;
        }
    }

    enum SearchNodeState { Open, Closed };

    class SearchNode
    {
        public SearchNodeState State { get; set; }
        public (int, int) Coordinates { get; private set; }
        public int Depth { get; private set; }
        public int TargetDistance { get; private set; }
        public int Score => Depth + TargetDistance;

        SearchNode Parent { get; set; }

        public List<(int, int)> Path
        {
            get
            {
                var path = new List<(int, int)>();
                var n = this;
                while (n != null)
                {
                    path.Add(n.Coordinates);
                    n = n.Parent;
                }
                path.Reverse();
                return path;
            }
        }

        public SearchNode(SearchNode parent, (int, int) coordinates, (int, int) target)
        {
            Parent = parent;
            Coordinates = coordinates;
            TargetDistance = coordinates.ManhattanDistance(target);
            Depth = parent == null ? 0 : parent.Depth + 1;
            State = SearchNodeState.Open;
        }
    }
    static IEnumerable<SearchNode> GetAdjacentNodes(SearchNode parent, Dictionary<(int, int), SearchNode> cache, SearchParameters searchParameters)
    {
        SearchNode node = null;
        var adjacent = new List<SearchNode>();
        foreach (var coords in parent.Coordinates.Neighbours())
        {
            if (coords.InGrid(searchParameters.Map) && searchParameters.Map[coords.Item2, coords.Item1])
            {
                if (!cache.ContainsKey(coords))
                {
                    node = new SearchNode(parent, coords, searchParameters.Target);
                }
                else if (cache[coords].State != SearchNodeState.Closed)
                {
                    node = cache[coords];
                }
                if (node != null)
                {
                    adjacent.Add(node);
                }
            }
        }

        return adjacent.OrderBy(n => n.Score);
    }

    public static bool AStarSearch(
        SearchParameters searchParameters,
        out List<(int, int)> path
    )
    {
        var cache = new Dictionary<(int, int), SearchNode>();
        cache.Add(searchParameters.Origin, new SearchNode(null, searchParameters.Origin, searchParameters.Target));
        while (true)
        {
            var node = cache.Values
                .Where(n => n.State != SearchNodeState.Closed && n.Score < searchParameters.MaxDepth)
                .OrderBy(n => n.Score)
                .FirstOrDefault();

            if (node == null)
            {
                path = null;
                return false;
            }

            node.State = SearchNodeState.Closed;

            var potentialTarget = GetAdjacentNodes(node, cache, searchParameters).FirstOrDefault();
            if (potentialTarget != null && potentialTarget.Coordinates == searchParameters.Target)
            {
                path = potentialTarget.Path;
                return true;
            }
        }
    }
}
