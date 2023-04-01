using System.Collections;
using System.Collections.Generic;
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
    VirtualOutBount,
}

abstract public class Level : MonoBehaviour
{
    static int gridScale = 3;

    abstract public int lvl { get; }
    abstract protected string[] grid { get; }
        
    public FaceDirection PlayerSpawnDirection;

    protected char getPosition(int x, int z) => grid[grid.Length - z - 1][x];
        
    protected void ApplyOverGrid(System.Action<int, int> xzCallback)
    {
        if (grid == null)
        {
            Debug.LogWarning($"{name} lacks initiated grid");
            return;
        }


        for (int z = 0; z < grid.Length; z++)
        {
            for (int x = 0; x < grid[grid.Length - z - 1].Length; x++)
            {
                xzCallback(x, z);
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
            Gizmos.DrawWireCube(new Vector3(x, 0.5f, z)*gridScale, Vector3.one * gridScale * gizmoScale);
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


    Dictionary<KeyValuePair<int, int>, char> levelRestore = new Dictionary<KeyValuePair<int, int>, char>();

    public bool ClaimPosition(GridEntity entity, Vector3Int position)
    {
        var z = grid.Length - position.z - 1;
        if (z < 0 || z >= grid.Length || position.x < 0) return false;

        var line = grid[z];
        if (position.x >= line.Length) return false;

        var current = line[position.x];
        if (GridEntity.InBound.GetStringValue().Contains(current) || GridEntity.PlayerSpawn.GetStringValue().Contains(current))
        {
            var chars = line.ToCharArray();
            var restorePosition = new KeyValuePair<int, int>(position.x, position.z);
            if (!levelRestore.ContainsKey(restorePosition)) levelRestore[restorePosition] = current;
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
            var restorePosition = new KeyValuePair<int, int>(position.x, position.z);
            var original = levelRestore.ContainsKey(restorePosition) ? levelRestore[restorePosition] : GridEntity.InBound.GetStringValue()[0];
            var chars = line.ToCharArray();
            chars[position.x] = original;
            grid[z] = new string(chars);

            return true;
        }
        return false;
    }

    public static Vector3Int AsWorldPosition(Vector3Int gridPosition) => gridPosition * gridScale;

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
}
