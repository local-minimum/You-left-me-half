using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using DeCrawl.Primitives;

namespace DeCrawl.Systems
{
    public class LootTable : FindingSingleton<LootTable>, StateSaver
    {
        private new void Awake()
        {
            if (instance == this)
            {
                DontDestroyOnLoad(this);
            }
        }

        private void Start()
        {
            AvailableLootables.ToList().ForEach(l => l.Loot(LootOwner.LootTable));
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
                .OrderBy(l => l.SerializationPriority)
                .Select(l => {
                    Debug.Log($"Storing {l.Id} owned by {l.Owner} at {l.Coordinates}");
                    return new LootDto(l.Id, l.Coordinates, l.Owner);
                 })
                .ToArray();

            return JsonUtility.ToJson(new StateDto(lootables));
        }

        public virtual Lootable CreateLootById(string id) => throw new System.NotImplementedException($"Loot table has no custom loot creator, cannot load {id}");

        public void DeserializeState(string json)
        {            
            var allLoot = AllLootables
                .OrderBy(l => l.SerializationPriority)
                .ToArray();

            var records = JsonUtility
                .FromJson<StateDto>(json)
                .records
                .ToDictionary(loot => loot.id, loot => loot);

            for (int i = 0; i < allLoot.Length; i++)
            {
                var loot = allLoot[i];

                if (records.ContainsKey(loot.Id))
                {
                    var record = records[loot.Id];

                    // First return item then place it were it needs to go
                    if (record.owner != LootOwner.LootTable)
                    {
                        loot.Loot(LootOwner.LootTable);
                    }

                    Debug.Log($"Restore loot {loot.Id} to {record.owner} at {record.coordinates}");
                    loot.Loot(record.owner, record.coordinates);
                    records.Remove(loot.Id);
                }
                else
                {
                    loot.gameObject.SetActive(false);
                }
            }

            foreach (var record in records)
            {

                var loot = CreateLootById(record.Key);
                loot.Loot(record.Value.owner, record.Value.coordinates);
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
            .Where(l => l is T)
            .FirstOrDefault() as T;


        public int Count<T>() where T : Lootable => AvailableLootables
            .Where(l => l is T)
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
            if (args.Owner == LootOwner.LootTable)
            {
                args.Consumed = true;

                loot.transform.SetParent(transform);
                loot.transform.localPosition = Vector3.zero;

                args.Coordinates = Vector3Int.zero;
                args.DefinedPosition = true;

            }
        }
    }
}