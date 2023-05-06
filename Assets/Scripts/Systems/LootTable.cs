using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class LootTable : MonoBehaviour, StateSaver
{
    public static LootTable instance { get; private set; }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(this);
    }

    private void Start()
    {
        AvailableLootables.ToList().ForEach(l => l.Loot(LootOwner.None));
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    [System.Serializable]
    private struct LootDto
    {
        public string id;
        public Vector3Int coordinates;
        public LootOwner owner;

        public LootDto(string id, Vector3Int coordinates, LootOwner owner)
        {
            this.id = id;
            this.coordinates = coordinates;
            this.owner = owner;
        }
    }
    
    [System.Serializable]
    private struct StateDto
    {
        public LootDto[] records;

        public StateDto(LootDto[] records)
        {
            this.records = records;
        }
    }

    public Lootable[] AvailableLootables
    {
        get
        {
            return GetComponentsInChildren<Lootable>();
        }
    }

    private Lootable[] AllLootables
    {
        get
        {
            return FindObjectsOfType<Lootable>();
        }
    }

    public string SerializeState()
    {
        var lootables = AllLootables
            .Select(l => new LootDto(l.Id, l.Coordinates, l.Owner))
            .ToArray();

        return JsonUtility.ToJson(new StateDto(lootables));
    }

    public void DeserializeState(string json)
    {
        var allLoot = AllLootables;
        var records = JsonUtility
            .FromJson<StateDto>(json)
            .records            
            .ToDictionary(loot => loot.id, loot => loot);

        for (int i = 0; i<allLoot.Length; i++)
        {
            var loot = allLoot[i];
            
            if (records.ContainsKey(loot.Id))
            {
                var record = records[loot.Id];
                loot.Loot(record.owner, record.coordinates);
            } else
            {
                loot.gameObject.SetActive(false);
            }
        }
    }

    public bool GetLootByPartialId(string partialId, out Lootable loot)
    {
        var options = AvailableLootables
            .Where(l => l.Id.StartsWith(partialId))
            .ToArray();

        if (options.Length > 0)
        {
            loot = options[0];
            return true;
        }

        loot = null;
        return false;
    }

    public T First<T>() where T : Lootable => AvailableLootables
        .Where(l => l.GetType() == typeof(T))
        .FirstOrDefault() as T;


    public int Count<T>() where T : Lootable => AvailableLootables
        .Where(l => l.GetType() == typeof(T))
        .Count();


    private void OnEnable()
    {
        Lootable.OnLoot += Lootable_OnLoot;
    }

    private void OnDisable()
    {
        Lootable.OnLoot -= Lootable_OnLoot;
    }

    private void Lootable_OnLoot(Lootable loot, LootEventArgs args)
    {
        if (args.Owner == LootOwner.None)
        {
            args.Consumed = true;

            loot.transform.SetParent(transform);
            loot.transform.localPosition = Vector3.zero;

            args.Coordinates = Vector3Int.zero;
            args.DefinedPosition = true;

            
        }
    }
}
