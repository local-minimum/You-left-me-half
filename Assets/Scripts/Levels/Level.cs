using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum GridEntity
{        
    [StringValue("Pp")]
    Player,
    [StringValue("Ss")]
    PlayerSpawn,
    [StringValue("Oo")]
    Other,
    [StringValue("Xx")]
    InBound,
    [StringValue("-")]
    OutBound,
    [StringValue("Vv")]
    VirtualOutBound,
}

public static class GridEntityExtensions
{
    public static GridEntity ToGridEntity(this char ch) {
        switch (ch) {
            case 'p':
            case 'P':
                return GridEntity.Player;
            case 's':
            case 'S':
                return GridEntity.PlayerSpawn;
            case 'O':
            case 'o':
                return GridEntity.Other;
            case 'X':
            case 'x':
                return GridEntity.InBound;
            case 'V':
            case 'v':
                return GridEntity.VirtualOutBound;
            default:
                return GridEntity.OutBound;
        }
    }

    public static bool IsClaimable(this GridEntity entity, bool allowVirtual = false) => 
        entity == GridEntity.InBound || entity == GridEntity.PlayerSpawn || (allowVirtual && entity == GridEntity.VirtualOutBound);

    public static bool IsInbound(this GridEntity entity, bool allowVirtual = false) =>
        entity != GridEntity.OutBound && (allowVirtual || entity != GridEntity.VirtualOutBound);

    public static bool IsOccupied(this GridEntity entity) => 
        entity == GridEntity.Player || entity == GridEntity.Other;

    public static bool IsBaseType(this GridEntity entity) =>
        entity == GridEntity.OutBound || entity == GridEntity.InBound || entity == GridEntity.VirtualOutBound;
}

abstract public class Level : MonoBehaviour
{
    public static int GridScale = 3;

    abstract public int lvl { get; }
    abstract protected string[] grid { get; }
        
    public FaceDirection PlayerSpawnDirection;
    
    protected char getPosition(int x, int z) => grid[grid.Length - z - 1][x];
    protected char getPosition((int, int) coords) => grid[grid.Length - coords.Item2 - 1][coords.Item1];

    protected int getRowLength(int z) => grid[grid.Length - z - 1].Length;

    protected void ApplyOverGrid(System.Action<int, int> xzCallback)
    {
        if (grid == null)
        {
            Debug.LogWarning($"{name} lacks initiated grid");
            return;
        }


        for (int z = 0; z < grid.Length; z++)
        {
            for (int x = 0, l = getRowLength(z); x < l; x++)
            {
                xzCallback(x, z);
            }
        }
    }

    protected void ApplyOverGrid(System.Action<(int, int)> xzCallback)
    {
        if (grid == null)
        {
            Debug.LogWarning($"{name} lacks initiated grid");
            return;
        }


        for (int z = 0; z < grid.Length; z++)
        {
            for (int x = 0, l = getRowLength(z); x < l; x++)
            {
                xzCallback((x, z));
            }
        }
    }


    protected Vector3Int FirstGridPosition(System.Func<char, bool> predicate)
    {
        for (int z = 0; z < grid.Length; z++)
        {
            for (int x = 0; x < grid[grid.Length - z - 1].Length; x++)
            {
                if (predicate(getPosition(x, z)))
                {
                    return new Vector3Int(x, 0, z);
                }
            }
        }

        throw new System.ArgumentException("No position matches predicate");
    }

#if UNITY_EDITOR
    [SerializeField, Range(0, 1)] float gizmoScale = 0.97f;

    private bool GridCharacterToColor(char c, out Color color)
    {
        switch (c)
        {
            // Other moving entity
            case 'O':
            case 'o':
                color = Color.red;
                return true;
            // PlayerFirstSpawn;
            case 'S':
            case 's':
                color = Color.yellow;
                return true;
            // Player
            case 'P':
            case 'p':
                color = Color.magenta;
                return true;
            // Passable
            case 'X':
            case 'x':
                color = Color.cyan;
                return true;
            // Virtual Block
            case 'V':
            case 'v':
                color = Color.gray;
                return true;
            default:
                color = Color.black;
                return false;
        }
    }

    private void DrawPositionGizmo(int x, int z)
    {
        if (GridCharacterToColor(getPosition(x, z), out Color color))
        {
            Gizmos.color = color;
            Gizmos.DrawWireCube(new Vector3(x, 0.5f, z)*GridScale, Vector3.one * GridScale * gizmoScale);
        }
    }

    private void OnDrawGizmos()
    {            
        ApplyOverGrid(DrawPositionGizmo);
    }
#endif

    public Vector3Int PlayerFirstSpawnPosition
    {
        get { return FirstGridPosition(ch => GridEntity.PlayerSpawn.GetStringValue().Contains(ch)); }
    }


    public Vector3Int PlayerPosition
    {
        get { return FirstGridPosition(ch => GridEntity.Player.GetStringValue().Contains(ch)); }
    }

    Dictionary<(int, int), char[]> levelRestore = new Dictionary<(int, int), char[]>();

    public GridEntity GridBaseStatus(int x, int z, bool allowVirtual = false)
    {
        var entity = getPosition(x, z).ToGridEntity();
        if (entity.IsBaseType())
        {
            return entity;
        }

        var original = GetPositionRestore(x, z, out int i).ToGridEntity();
        if (i >= 0)
        {            
            if (original.IsBaseType())
            {
                return original;
            }            
        }

        return entity.IsInbound(allowVirtual) ? GridEntity.InBound : GridEntity.OutBound;
    }

    public GridEntity GridStatus((int, int) coords) => getPosition(coords).ToGridEntity();
    public GridEntity GridStatus(int x, int z) => getPosition(x, z).ToGridEntity();    

    private char GetPositionRestore(int x, int z, out int i)
    {
        var restorePosition = (x, z);
        if (!levelRestore.ContainsKey(restorePosition))
        {
            i = -1;
            return char.MinValue;
        }

        var restoreArr = levelRestore[restorePosition];            
        for (i = 1; i >= 0; i--)
        {
            if (restoreArr[i] != char.MinValue)
            {
                return restoreArr[i];
            }
        }

        i = -1;
        return char.MinValue;
    }

    public bool ClaimPosition(GridEntity entity, Vector3Int position, bool allowVirtualPositions, bool allowOutOfBounds = false)
    {
        var z = grid.Length - position.z - 1;
        if (z < 0 || z >= grid.Length || position.x < 0) return false;

        var line = grid[z];
        if (position.x >= line.Length) return false;

        var current = line[position.x];
        
        if (
            GridEntity.InBound.GetStringValue().Contains(current) || 
            GridEntity.PlayerSpawn.GetStringValue().Contains(current) || 
            allowVirtualPositions && GridEntity.VirtualOutBound.GetStringValue().Contains(current) ||
            allowOutOfBounds && GridEntity.OutBound.GetStringValue().Contains(current)
            )
        {
            var chars = line.ToCharArray();
            var restorePosition = (position.x, position.z);
            if (!levelRestore.ContainsKey(restorePosition))
            {
                levelRestore[restorePosition] = new char[2] { current, char.MinValue };                
            } else
            {
                var restoreArr = levelRestore[restorePosition];
                bool hadEmptyRestoreSlot = false;
                for (int i = 0; i < 2; i++)
                {
                    if (restoreArr[i] == char.MinValue)
                    {
                        restoreArr[i] = current;
                        hadEmptyRestoreSlot = true;
                        break;
                    }
                }
                if (!hadEmptyRestoreSlot) return false;
            }
            chars[position.x] = entity.GetStringValue()[0];
            grid[z] = new string(chars);
            return true;
        }
        return false;
    }


    public bool ReleasePosition(GridEntity entity, Vector3Int position)
    {
        var z = grid.Length - position.z - 1;
        if (z < 0 || z >= grid.Length || position.x < 0) return false;

        var line = grid[z];
        if (position.x >= line.Length) return false;

        var current = line[position.x];
        if (entity.GetStringValue().Contains(current))
        {
            var restorePosition = (position.x, position.z);

            var original = GetPositionRestore(position.x, position.z, out int i);
            if (i >= 0)
            {
                levelRestore[restorePosition][i] = char.MinValue;
            } else
            {
                return false;
            }
            
            var chars = line.ToCharArray();
            chars[position.x] = original;
            grid[z] = new string(chars);

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

    Vector3Int playerPosition;
    FaceDirection playerLookDirection;

    private void PlayerController_OnPlayerMove(Vector3Int position, FaceDirection lookDirection)
    {
        playerPosition = position;
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

    bool[,] CreateMapFilter(System.Func<GridEntity, bool> predicate)
    {
        var maxX = 0;
        var lookup = new HashSet<(int, int)>();
        System.Action<(int, int)> callback = (coords) =>
        {
            maxX = Mathf.Max(maxX, coords.Item1);
            if (predicate(getPosition(coords).ToGridEntity()))
            {
                lookup.Add(coords);
            }
        };
        ApplyOverGrid(callback);
        var map = new bool[grid.Length, maxX + 1];        
        foreach (var key in lookup)
        {
            map[key.Item2, key.Item1] = true;
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
            CreateMapFilter(permissablePredicate),
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
