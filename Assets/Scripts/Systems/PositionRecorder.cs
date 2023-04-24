using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using CompressString;

public class PositionRecorder : MonoBehaviour, StateSaver
{
    [System.Serializable]
    private struct StateDto
    {
        public PositionRecordDto[] records;

        public StateDto(PositionRecordDto[] records)
        {
            this.records = records;
        }
    }


    [System.Serializable]
    private struct PositionRecordDto
    {
        public string id;
        public Vector3Int position;
        public FaceDirection lookDirection;

        public PositionRecordDto(string id, Vector3Int position, FaceDirection lookDirection)
        {
            this.id = id;
            this.position = position;
            this.lookDirection = lookDirection;
        }
    }

    Dictionary<string, Vector3Int> positions = new Dictionary<string, Vector3Int>();
    Dictionary<string, FaceDirection> lookDirections = new Dictionary<string, FaceDirection>();
    bool listening = true;

    public static PositionRecorder instance { get; private set; }

    private void Awake()
    {        
        if (instance != null && instance != this)
        {
            Destroy(this);
            return;
        }

        instance = this;

        var entities = FindObjectsOfType<MovingEntity>();
        for (int i=0; i<entities.Length; i++)
        {
            entities[i].OnMove += PositionRecorder_OnMove;
        }
    }

    public string[] GetEntities(Vector3Int position) => positions
        .Where(kvp => kvp.Value == position)
        .Select(kvp => kvp.Key)
        .ToArray();
    

    private void OnDestroy()
    {
        var entities = FindObjectsOfType<MovingEntity>();
        for (int i = 0; i < entities.Length; i++)
        {
            entities[i].OnMove -= PositionRecorder_OnMove;
        }
    }

    private void PositionRecorder_OnMove(string id, Vector3Int position, FaceDirection lookDirection)
    {
        if (!listening) return;

        positions[id] = position;
        lookDirections[id] = lookDirection;
    }

    public void ResetStored()
    {
        positions.Clear();
        lookDirections.Clear();
    }

    public string SerializeState()
    {
        var records = positions.Keys
            .Select(id => new PositionRecordDto(id, positions[id], lookDirections[id]))
            .ToArray();

        return JsonUtility.ToJson(new StateDto(records));
    }

    public void DeserializeState(string json)
    {
        var records = JsonUtility.FromJson<StateDto>(json).records;
        for (int i=0; i<records.Length; i++)
        {
            var state = records[i];
            positions[state.id] = state.position;
            lookDirections[state.id] = state.lookDirection;
        }
    }

    public void RestorePositions()
    {
        listening = false;

        var entities = FindObjectsOfType<MovingEntity>();
        for (int i = 0; i < entities.Length; i++)
        {
            var entity = entities[i];
            var id = entity.Id;
            if (positions.ContainsKey(id))
            {
                entity.SetNewGridPosition(positions[id], lookDirections[id]);
            }
        }

        listening = true;
    }

    private string debugPlayerPrefsKey = "save.debug";

    [SerializeField]
    bool hideDebugUI = false;

    private void OnGUI()
    {
        if (hideDebugUI) return;

        if (GUILayout.Button("Debug Save"))
        {
            PlayerPrefs.SetString(debugPlayerPrefsKey, StringCompressor.CompressString(SerializeState()));                
        }
        if (GUILayout.Button("Debug Load"))
        {
            var data = PlayerPrefs.GetString(debugPlayerPrefsKey);
            if (string.IsNullOrEmpty(data))
            {
                Debug.LogWarning($"Nothing stored at {debugPlayerPrefsKey} yet");
            } else
            {
                DeserializeState(StringCompressor.DecompressString(data));                
                RestorePositions();
            }
                
        }
    }
}
