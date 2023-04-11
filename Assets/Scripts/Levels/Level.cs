using System.Collections.Generic;
using System.Linq;
using UnityEngine;

abstract public class Level : MonoBehaviour
{
    public static int GridScale = 3;

    abstract public int lvl { get; }
    abstract protected string[] charGrid { get; }

    private GridEntity[,] _grid;
    protected GridEntity[,] grid
    {
        get
        {
            if (_grid == null)
            {
                var (cols, rows) = CharGridShape;
                _grid = new GridEntity[cols, rows];
                System.Action<int, int> callback = (x, z) =>
                {
                    _grid[x, z] = getChar(x, z).ToGridEntity();
                };
                ApplyOverCharGrid(callback);
            };


            return _grid;
        }
    }

    (int, int) CharGridShape
    {
        get
        {
            var maxX = 0;
            System.Action<int, int> callback = (x, _) =>
            {
                maxX = Mathf.Max(maxX, x);
            };
            ApplyOverCharGrid(callback);
            return (maxX + 1, charGrid.Length);
        }
    }

    (int, int) GridShape => (grid.GetLength(0), grid.GetLength(1));

    public FaceDirection PlayerSpawnDirection;

    private char getChar(int x, int z) => charGrid[charGrid.Length - z - 1][x];

    protected int getRowLength(int z) => charGrid[charGrid.Length - z - 1].Length;

    private void ApplyOverCharGrid(System.Action<int, int> xzCallback)
    {
        if (charGrid == null)
        {
            Debug.LogWarning($"{name} lacks initiated grid");
            return;
        }


        for (int z = 0; z < charGrid.Length; z++)
        {
            for (int x = 0, l = getRowLength(z); x < l; x++)
            {
                xzCallback(x, z);
            }
        }
    }

    public void ApplyOverGrid(System.Action<(int, int), GridEntity> xzCallback)
    {
        if (charGrid == null)
        {
            Debug.LogWarning($"{name} lacks initiated grid");
            return;
        }

        var (maxX, maxZ) = GridShape;

        for (int z = 0; z < maxZ; z++)
        {
            for (int x = 0; x < maxX; x++)
            {
                xzCallback((x, z), GridStatus(x, z));
            }
        }
    }


    protected Vector3Int FirstGridPosition(System.Func<GridEntity, bool> predicate)
    {
        var (maxX, maxZ) = GridShape;

        for (int z = 0; z < maxZ; z++)
        {
            for (int x = 0; x < maxX; x++)
            {
                if (predicate(grid[x, z]))
                {
                    return new Vector3Int(x, 0, z);
                }
            }
        }

        throw new System.ArgumentException("No position matches predicate");
    }


    public Vector3Int PlayerFirstSpawnPosition
    {
        get { return FirstGridPosition(e => e == GridEntity.PlayerSpawn); }
    }


    public Vector3Int PlayerPosition
    {
        get { return FirstGridPosition(e => e == GridEntity.Player); }
    }

    Dictionary<(int, int), GridEntity[]> gridRestore = new Dictionary<(int, int), GridEntity[]>();

    public GridEntity GridBaseStatus(int x, int z, bool allowVirtual = false) => GridBaseStatus((x, z), allowVirtual);

    public GridEntity GridBaseStatus((int, int) coords, bool allowVirtual = false)
    {
        var (x, z) = coords;
        if (x < 0 || z < 0) return GridEntity.None;

        var current = grid[x, z];
        if (current.ConvertableToBaseType(allowVirtual))
        {
            return current.BaseType();
        }

        if (gridRestore.ContainsKey(coords))
        {
            var restores = gridRestore[coords];
            for (int i = 1; i>=0; i--)
            {
                if (restores[i].ConvertableToBaseType(allowVirtual))
                {
                    return restores[i].BaseType();
                }
            }
        }

        return GridEntity.None;        
    }

    public GridEntity GridStatus((int, int) coords) => grid[coords.Item1, coords.Item2];
    public GridEntity GridStatus(int x, int z) => grid[x, z];

    private GridEntity GetPositionRestore(int x, int z, out int i)
    {
        var restorePosition = (x, z);
        if (!gridRestore.ContainsKey(restorePosition))
        {
            i = -1;
            return GridEntity.None;
        }

        var restoreArr = gridRestore[restorePosition];
        for (i = 1; i >= 0; i--)
        {
            if (restoreArr[i] != GridEntity.None)
            {
                return restoreArr[i];
            }
        }

        i = -1;
        return GridEntity.None;
    }

    public bool IsValidPosition(Vector3Int position) {
        var (maxX, maxZ) = GridShape;
        return !(position.z < 0 || position.z >= maxZ || position.x < 0 || position.x >= maxX);
    }

    public bool ClaimPosition(GridEntity entity, Vector3Int position, bool allowVirtualPositions, bool allowOutOfBounds = false)
    {
        if (!IsValidPosition(position)) return false;

        var current = grid[position.x, position.z];
        
        if (current.IsClaimable(allowVirtualPositions) ||  allowOutOfBounds && GridEntity.OutBound == current)
        {
            var restorePosition = position.XZTuple();
            
            if (!gridRestore.ContainsKey(restorePosition))
            {
                gridRestore[restorePosition] = new GridEntity[2] { current, GridEntity.None };                
            } else
            {
                var restoreArr = gridRestore[restorePosition];
                bool hadEmptyRestoreSlot = false;
                for (int i = 0; i < 2; i++)
                {
                    if (restoreArr[i] == GridEntity.None)
                    {
                        restoreArr[i] = current;
                        hadEmptyRestoreSlot = true;
                        break;
                    }
                }
                if (!hadEmptyRestoreSlot) return false;
            }

            grid[position.x, position.z] = entity;
            return true;
        }
        return false;
    }


    public bool ReleasePosition(GridEntity entity, Vector3Int position)
    {
        if (!IsValidPosition(position)) return false;
        
        if (entity == grid[position.x, position.z])
        {
            var restorePosition = position.XZTuple();

            var original = GetPositionRestore(position.x, position.z, out int i);
            if (i >= 0)
            {
                gridRestore[restorePosition][i] = GridEntity.None;
            } else
            {
                return false;
            }


            grid[position.x, position.z] = original;
            return true;
        }
        return false;
    }

    public static Vector3Int AsWorldPosition(Vector3Int gridPosition) => gridPosition * GridScale;
    public static Vector3Int AsGridPosition(Vector3 worldPosition) => new Vector3Int(Mathf.FloorToInt(worldPosition.x / GridScale), 0, Mathf.FloorToInt(worldPosition.z / GridScale));

    private static Level _instance;
    public static Level instance { 
        get { 
            if (_instance == null)
            {
                _instance = FindObjectOfType<Level>();
            }
            return _instance;
        }
        private set { _instance = value; }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnDestroy()
    {
        if (instance == this) { instance = null; }
    }

    private void OnEnable()
    {
        Lootable.OnLoot += Lootable_OnLoot;
        PlayerController.OnPlayerMove += PlayerController_OnPlayerMove;
    }

    private void OnDisable()
    {
        Lootable.OnLoot -= Lootable_OnLoot;
        PlayerController.OnPlayerMove -= PlayerController_OnPlayerMove;
    }

    FaceDirection playerLookDirection;

    private void PlayerController_OnPlayerMove(Vector3Int position, FaceDirection lookDirection)
    {
        playerLookDirection = lookDirection;
    }

    private void Lootable_OnLoot(Lootable loot, LootEventArgs args)
    {
        if (args.Owner != LootOwner.Level) return;

        if (!args.DefinedPosition)
        {
            Debug.LogWarning($"Random placement of loot {loot.Id} attempted, not yet implemented");
            return;
        }

        loot.transform.SetParent(transform);
        loot.transform.position = GridScale * (args.Coordinates + 0.5f * playerLookDirection.AsVector());
        loot.ManifestSide = playerLookDirection;
        args.Consumed = true;
    }

    bool[,] CreateMapFilter(System.Func<(int, int), bool> predicate)
    {
        var lookup = new HashSet<(int, int)>();
        System.Action<(int, int), GridEntity> callback = (coords, _) =>
        {
            if (predicate(coords))
            {
                lookup.Add(coords);
            }
        };
        ApplyOverGrid(callback);

        var (maxX, maxZ) = GridShape;
        var map = new bool[maxX, maxZ];
        foreach (var key in lookup)
        {
            map[key.Item1, key.Item2] = true;
        }

        return map;
    }

        
    public bool FindPathToPlayerFrom(
        (int, int) origin, 
        int maxDepth, 
        System.Func<GridEntity, bool> permissablePredicate,
        out List<(int, int)> path
    )
    {
        var searchParameters = new GraphSearch.SearchParameters(
            origin, 
            PlayerPosition, 
            CreateMapFilter((coords) => permissablePredicate(GridBaseStatus(coords))),
            maxDepth
        );

        if (!searchParameters.InBound(searchParameters.Origin))
        {
            Debug.LogWarning($"Atempting to find player from out of bounds {searchParameters.Origin}");
        }

        if (!searchParameters.InBound(searchParameters.Target))
        {
            Debug.LogError($"Atempting to find player who is out of bounds {searchParameters.Origin}");
        }

        return GraphSearch.AStarSearch(searchParameters, out path);
    }
}
