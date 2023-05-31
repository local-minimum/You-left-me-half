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

            public StateDto(string positions, string loot)
            {
                SerializedPositions = positions;
                SerializedLoot = loot;
            }
        }

        private string debugPlayerPrefsKey = "save.debug";

        [SerializeField]
        bool hideDebugUI = false;

        void Save()
        {
            var state = new StateDto(
                PositionRecorder.instance.SerializeState(),
                LootTable.instance.SerializeState()
            );
            var compressedJSON = StringCompressor.CompressString(JsonUtility.ToJson(state));
            PlayerPrefs.SetString(debugPlayerPrefsKey, compressedJSON);
        }


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

                PositionRecorder.instance.DeserializeState(state.SerializedPositions);
                PositionRecorder.instance.RestorePositions();

                LootTable.instance.DeserializeState(state.SerializedLoot);
            }
            Game.RevertStatus();
        }

        private void OnGUI()
        {
            if (hideDebugUI) return;

            if (GUILayout.Button("Debug Save"))
            {
                Save();
            }
            if (GUILayout.Button("Debug Load"))
            {
                Load();
            }
        }
    }
}
