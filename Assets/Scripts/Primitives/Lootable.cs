using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class LootEventArgs: System.EventArgs
{
    public LootOwner Owner { get; private set; }
    public Vector3Int Coordinates { get; private set; }
    public bool DefinedPosition { get; private set; }
    public bool Consumed { get; set; }

    public LootEventArgs(LootOwner owner)
    {
        Owner = owner;
        DefinedPosition = false;
    }

    public LootEventArgs(LootOwner owner, Vector3Int coordinates)
    {
        Owner = owner;
        DefinedPosition = true;
        Coordinates = coordinates;
    }
}

public delegate void  LootEvent(Lootable loot, LootEventArgs args);

public enum LootOwner { None, Player, Level };

public class Lootable : MonoBehaviour
{
    public static event LootEvent OnLoot;

    [SerializeField, Tooltip("Leave empty to use game object name")]
    string id;

    public string Id
    {
        get
        {
            return string.IsNullOrEmpty(id) ? name : id;
        }
    }
    
    public Vector3Int Coordinates { get; set; }
    public LootOwner Owner { get; set; }

    [SerializeField]
    public Vector2Int[] InventoryShape;

    public bool Loot(LootOwner owner)
    {
        var args = new LootEventArgs(owner);        
        OnLoot?.Invoke(this, args);
        return args.Consumed;
    }

    public bool Loot(LootOwner owner, Vector3Int coordnates)
    {
        var args = new LootEventArgs(owner, coordnates);
        OnLoot?.Invoke(this, args);
        return args.Consumed;
    }

    private void Awake()
    {
        if (InventoryShape.Any(coords => coords.y < 0))
        {
            Debug.LogError($"{Id} ({name}) has negative y-values in inventory shape");
        }
    }
}
