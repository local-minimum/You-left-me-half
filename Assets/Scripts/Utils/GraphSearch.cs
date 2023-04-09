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
        bool[,] Map;

        public (int, int) Shape => (Map.GetLength(0), Map.GetLength(1));
        public bool InBound(int x, int z) => Map[z, x];
        public bool InBound((int, int) coords) => Map[coords.Item2, coords.Item1];
        public bool WithinShape((int, int) coords) =>
            coords.Item1 >= 0 && coords.Item2 >= 0 && coords.Item1 < Map.GetLength(1) && coords.Item2 < Map.GetLength(0);

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
            if (searchParameters.WithinShape(coords) && searchParameters.InBound(coords))
            {
                if (!cache.ContainsKey(coords))
                {
                    node = new SearchNode(parent, coords, searchParameters.Target);
                    cache.Add(coords, node);
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

    static void DebugLog(
        SearchParameters searchParameters, 
        Dictionary<(int, int), SearchNode> cache, 
        (int, int) coordinates
    )
    {
        var (rows, cols) = searchParameters.Shape;
        var outString = $"{searchParameters.Origin} -> {searchParameters.Target} / Cache size{cache.Count}:\n";
        for (int z = rows - 1; z>0; z--)
        {
            for (int x = 0; x< cols; x ++)
            {
                var state = searchParameters.InBound(x, z);
                var coords = (x, z);
                var isCoords = coordinates == coords;
                if (state)
                {
                    if (isCoords)
                    {
                        outString += '@';
                    } else if (coords == searchParameters.Origin)
                    {
                        outString += cache.ContainsKey(coords) ? 'F' : 'f';
                    } else if (coords == searchParameters.Target)
                    {
                        outString += cache.ContainsKey(coords) ? 'X' : 'x';
                    } else if (!cache.ContainsKey(coords))
                    {
                        outString += '.';
                    } else if (cache[coords].State == SearchNodeState.Open)
                    {
                        outString += 'o';
                    } else
                    {
                        outString += 'c';
                    }
                    
                } else if (isCoords)
                {
                    outString += '?';
                } else if (cache.ContainsKey(coords))
                {
                    outString += '!';
                } else
                {
                    outString += ' ';
                }
            }
            outString += '\n';
        }

        Debug.Log(outString);
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
                .Where(n => n.State != SearchNodeState.Closed && n.Score <= searchParameters.MaxDepth)
                .OrderBy(n => n.Score)
                .FirstOrDefault();

            if (node == null)
            {
                
                Debug.Log($"Found no path after investigating {cache.Count()} nodes, max depth {searchParameters.MaxDepth}");
                node = cache.Values
                    .Where(n => n.State != SearchNodeState.Closed)
                    .OrderBy(n => n.Score)
                    .FirstOrDefault();

                DebugLog(searchParameters, cache, node?.Coordinates ?? (-1, -1));                
                
                path = null;
                return false;
            }

            node.State = SearchNodeState.Closed;

            var potentialTarget = GetAdjacentNodes(node, cache, searchParameters).FirstOrDefault();
            if (potentialTarget != null && potentialTarget.Coordinates == searchParameters.Target)
            {
                path = potentialTarget.Path;
                return searchParameters.InBound(potentialTarget.Coordinates);
            }
        }
    }
}
