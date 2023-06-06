using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DeCrawl.Primitives;

namespace DeCrawl.Systems
{
    public class MetadataRecorder : FindingSingleton<MetadataRecorder>, StateSaver
    {
        GameMetadata _Metadata = new GameMetadata();
        double playStart;

        public GameMetadata Peak(string json) => FromJson(json);

        private GameMetadata FromJson(string json) => string.IsNullOrEmpty(json) ? null : new GameMetadata(JsonUtility.FromJson<MetadataDto>(json));


        public void DeserializeState(string json) => _Metadata = FromJson(json);
        

        public string SerializeState()
        {
            _Metadata.Time = System.DateTimeOffset.Now;
            return JsonUtility.ToJson(new MetadataDto(_Metadata));
        }

        public class GameMetadata {
            public string Title;
            public string Quest;
            public string Character;
            public string Region;
            public string AuxInfo;
            public double PlayTimeSeconds;
            public System.DateTimeOffset Time;

            public GameMetadata() { }

            public GameMetadata(MetadataDto dto)
            {
                Title = dto.Title;
                Quest = dto.Quest;
                Character = dto.Character;
                Region = dto.Region;
                AuxInfo = dto.AuxInfo;
                PlayTimeSeconds = dto.PlayTimeSeconds;
                Time = System.DateTimeOffset.FromUnixTimeMilliseconds(dto.EpochMillies);
            }
        }

        [System.Serializable]
        public struct MetadataDto
        {
            public string Title;
            public string Quest;
            public string Character;
            public string Region;
            public string AuxInfo;
            public double PlayTimeSeconds;
            public long EpochMillies;

            public MetadataDto(GameMetadata metadata)
            {
                Title = metadata.Title;
                Quest = metadata.Quest;
                Character = metadata.Character;
                Region = metadata.Region;
                AuxInfo = metadata.AuxInfo;
                PlayTimeSeconds = metadata.PlayTimeSeconds;
                EpochMillies = metadata.Time.ToUniversalTime().ToUnixTimeMilliseconds();
            }
        }

        private void OnEnable()
        {
            Game.OnChangeStatus += Game_OnChangeStatus;
        }

        private void OnDisable()
        {
            Game.OnChangeStatus -= Game_OnChangeStatus;
        }

        bool PlayingState(GameStatus status) => status == GameStatus.CutScene || status == GameStatus.FightScene || status == GameStatus.Playing;

        private void Game_OnChangeStatus(GameStatus status, GameStatus oldStatus)
        {
            bool isPlaying = PlayingState(status);
            bool wasPlaying = PlayingState(oldStatus);

            if (isPlaying == wasPlaying) return;

            if (isPlaying)
            {
                Debug.Log("Start recording play time");
                playStart = Time.realtimeSinceStartupAsDouble;
            } else
            {
                var playtime = Time.realtimeSinceStartupAsDouble - playStart;
                _Metadata.PlayTimeSeconds += playtime;

                Debug.Log($"Add playtime {playtime} => {_Metadata.PlayTimeSeconds}");

                _Metadata.Time = System.DateTimeOffset.Now;
            }
        }

        public string Title
        {
            get => _Metadata.Title;
        }

        public string Quest
        {
            get => _Metadata.Quest;
            set
            {
                _Metadata.Quest = value;
            }
        }

        public string Character
        {
            get => _Metadata.Character;
            set
            {
                _Metadata.Character = value;
            }
        }

        public string Region
        {
            get => _Metadata.Region;
            set
            {
                _Metadata.Region = value;
            }
        }

        public string AuxInfo
        {
            get => _Metadata.AuxInfo;
            set
            {
                _Metadata.AuxInfo = value;
            }
        }
    }
}
