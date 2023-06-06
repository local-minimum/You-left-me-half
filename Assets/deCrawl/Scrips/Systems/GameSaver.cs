using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DeCrawl.Primitives;
using DeCrawl.Systems.Storage;

namespace DeCrawl.Systems
{
    public class GameSaver : FindingSingleton<GameSaver>
    {
        [System.Serializable]
        private struct StateDto
        {
            public string SerializedMetadata;
            public string SerializedPositions;
            public string SerializedLoot;
            public string SerializedPhases;
            public string SerializedCurrencies;

            public StateDto(string metadata, string positions, string loot, string phases, string currencies)
            {
                SerializedMetadata = metadata;
                SerializedPositions = positions;
                SerializedLoot = loot;
                SerializedPhases = phases;
                SerializedCurrencies = currencies;
            }
        }

        [SerializeField]
        private string debugStorageKey = "debug";

        IEnumerable<IStorage> storages => GetComponents<IStorage>().Where(s => s.Enabled);

        [SerializeField]
        bool hideDebugUI = false;

        public void Save(string saveSlot)
        {
            var state = new StateDto(
                MetadataRecorder.instance?.SerializeState(),
                PositionRecorder.instance?.SerializeState(),
                LootTable.instance?.SerializeState(),
                PhaseRecorder.instance?.SerializeState(),
                CurrencyTracker.SerializeState()
            );

            var data = JsonUtility.ToJson(state);
            foreach (var storage in storages)
            {
                if (!storage.Save(saveSlot, data))
                {
                    Debug.Log($"Could not save state to {storage}");
                }
            }
        }

        public bool HasSave(string saveSlot) => storages.Any(s => s.Has(saveSlot));

        void Load(StateDto state)
        {
            PositionRecorder.instance?.DeserializeState(state.SerializedPositions);

            PhaseRecorder.instance?.DeserializeState(state.SerializedPhases);

            // Loot must be fixed after phases in case a phase affects what can be looted where
            LootTable.instance?.DeserializeState(state.SerializedLoot);

            CurrencyTracker.DeserializeState(state.SerializedCurrencies);

            MetadataRecorder.instance?.DeserializeState(state.SerializedMetadata);
        }

        public bool PeakMetadata(string saveSlot, out MetadataRecorder.GameMetadata metadata)
        {
            if (MetadataRecorder.instance == null)
            {
                metadata = null;
                return false;
            }

            foreach (var storage in storages)
            {
                if (storage.Read(saveSlot, out string data))
                {
                    Debug.Log($"+++ Loading metadata {saveSlot} from {storage} +++");
                    var state = JsonUtility.FromJson<StateDto>(data);
                    metadata = MetadataRecorder.instance.Peak(state.SerializedMetadata);
                    if (metadata != null)
                    {
                        return true;
                    }                    
                }
                else
                {
                    Debug.LogWarning($"Loading metadata {saveSlot} from {storage} failed");
                }
            }

            metadata = null;
            return false;
        }

        public void Load(string saveSlot)
        {
            Game.Status = GameStatus.Loading;
            foreach (var storage in storages)
            {
                if (storage.Read(saveSlot, out string data))
                {
                    Debug.Log($"+++ Loading {saveSlot} from {storage} +++");
                    var state = JsonUtility.FromJson<StateDto>(data);
                    Load(state);
                    break;
                } else
                {
                    Debug.LogWarning($"Loading {saveSlot} from {storage} failed");
                }
            }
            Game.RevertStatus();
        }

        [SerializeField]
        Rect DebugUIRect = new Rect(0, 0, 100, 48);

        private void OnGUI()
        {
            if (hideDebugUI) return;

            GUILayout.BeginArea(DebugUIRect);

            if (GUILayout.Button("Debug Save"))
            {
                Save(debugStorageKey);
            }
            if (HasSave(debugStorageKey) && GUILayout.Button("Debug Load"))
            {
                Load(debugStorageKey);
            }

            GUILayout.EndArea();
        }
    }
}
