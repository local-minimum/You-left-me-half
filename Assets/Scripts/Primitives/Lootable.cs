using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootEventArgs: System.EventArgs
{
    public Vector3Int Coordinates { get; private set; }
    public bool DefinedPosition { get; private set; }
    public bool Consumed { get; set; }

    public LootEventArgs()
    {
        DefinedPosition = false;
    }

    public LootEventArgs(Vector3Int coordinates)
    {
        DefinedPosition = true;
        Coordinates = coordinates;
    }
}

public delegate bool LootEvent(Lootable loot, LootEventArgs args);

public enum LootOwner { None, Player, Level };

public class Lootable : MonoBehaviour
{
    public static LootEvent OnLoot;

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
    public LootOwner Owner { get; private set; }

    public bool Loot(LootOwner owner)
    {
        var args = new LootEventArgs();
        OnLoot?.Invoke(this, args);
        return args.Consumed;
    }

    public bool Loot(LootOwner owner, Vector3Int coordnates)
    {
        var args = new LootEventArgs(coordnates);
        OnLoot?.Invoke(this, args);
        return args.Consumed;
    }
}
