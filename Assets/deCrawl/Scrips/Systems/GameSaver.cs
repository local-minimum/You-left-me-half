using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DeCrawl.Primitives;
using DeCrawl.Utils;

namespace DeCrawl.Systems
{
    public class GameSaver : FindingSingleton<GameSaver>
    {
        [System.Serializable]
        private struct StateDto
        {
            public string SerializedPositions;
            public string SerializedLoot;
            public string SerializedPhases;
            public string SerializedCurrencies;

            public StateDto(string positions, string loot, string phases, string currencies)
            {
                SerializedPositions = positions;
                SerializedLoot = loot;
                SerializedPhases = phases;
                SerializedCurrencies = currencies;
            }
        }

        [SerializeField]
        private string debugPlayerPrefsKey = "save.debug";

        [SerializeField]
        bool hideDebugUI = false;

        void Save()
        {
            var state = new StateDto(
                PositionRecorder.instance?.SerializeState(),
                LootTable.instance?.SerializeState(),
                PhaseRecorder.instance?.SerializeState(),
                CurrencyTracker.SerializeState()
            );
            
            var compressedJSON = StringCompressor.CompressString(JsonUtility.ToJson(state));
            PlayerPrefs.SetString(debugPlayerPrefsKey, compressedJSON);
        }

        public bool HasSave => PlayerPrefs.HasKey(debugPlayerPrefsKey);

        void Load()
        {
            Game.Status = GameStatus.Loading;
            var data = PlayerPrefs.GetString(debugPlayerPrefsKey);
            if (string.IsNullOrEmpty(data))
            {
                Debug.LogWarning($"Nothing stored at {debugPlayerPrefsKey} yet");
            }
            else
            {                
                var state = JsonUtility.FromJson<StateDto>(StringCompressor.DecompressString(data));

                PositionRecorder.instance?.DeserializeState(state.SerializedPositions);

                PhaseRecorder.instance?.DeserializeState(state.SerializedPhases);

                // Loot must be fixed after phases in case a phase affects what can be looted where
                LootTable.instance?.DeserializeState(state.SerializedLoot);

                CurrencyTracker.DeserializeState(state.SerializedCurrencies);

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
                Save();
            }
            if (HasSave && GUILayout.Button("Debug Load"))
            {
                Load();
            }

            GUILayout.EndArea();
        }
    }
}
