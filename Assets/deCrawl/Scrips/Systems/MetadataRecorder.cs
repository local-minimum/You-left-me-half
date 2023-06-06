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
            /// <summary>
            /// The title of the same, i.e. if user is allowed to input it
            /// </summary>
            public string Title;
            /// <summary>
            /// The active quest or event
            /// </summary>
            public string Quest;
            /// <summary>
            /// The name of the player character(s) / group
            /// </summary>
            public string Character;
            /// <summary>
            /// The region or level in the game
            /// </summary>
            public string Region;
            /// <summary>
            /// Any extra information the game wants to track
            /// </summary>
            public string AuxInfo;
            /// <summary>
            /// Total time played (not counting menus and paused game)
            /// </summary>
            public double PlayTimeSeconds;
            /// <summary>
            /// Time for the most recent version of metadata
            /// </summary>
            public System.DateTimeOffset Time;
            /// <summary>
            /// An image of the game when metadata was recorded
            /// </summary>
            public Texture2D Screenshot;

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
                if (!string.IsNullOrEmpty(dto.Screenshot))
                {
                    var bytes = System.Convert.FromBase64String(dto.Screenshot);
                    Texture2D tex = new Texture2D(2, 2);
                    if (ImageConversion.LoadImage(tex, bytes))
                    {
                        Screenshot = tex;
                    } else
                    {
                        Debug.Log("Failed to load screenshot");
                    }
                }
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
            public string Screenshot;

            public MetadataDto(GameMetadata metadata)
            {
                Title = metadata.Title;
                Quest = metadata.Quest;
                Character = metadata.Character;
                Region = metadata.Region;
                AuxInfo = metadata.AuxInfo;
                PlayTimeSeconds = metadata.PlayTimeSeconds;
                EpochMillies = metadata.Time.ToUniversalTime().ToUnixTimeMilliseconds();
                Screenshot = System.Convert.ToBase64String(ImageConversion.EncodeToPNG(metadata.Screenshot));                
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

        /// <summary>
        /// The title of the same, i.e. if user is allowed to input it
        /// </summary>
        public string Title
        {
            get => _Metadata.Title;
        }

        /// <summary>
        /// The active quest or event
        /// </summary>
        public string Quest
        {
            get => _Metadata.Quest;
            set
            {
                _Metadata.Quest = value;
            }
        }

        /// <summary>
        /// The name of the player character(s) / group
        /// </summary>
        public string Character
        {
            get => _Metadata.Character;
            set
            {
                _Metadata.Character = value;
            }
        }

        /// <summary>
        /// The region or level in the game
        /// </summary>
        public string Region
        {
            get => _Metadata.Region;
            set
            {
                _Metadata.Region = value;
            }
        }

        /// <summary>
        /// Any extra information the game wants to track
        /// </summary>
        public string AuxInfo
        {
            get => _Metadata.AuxInfo;
            set
            {
                _Metadata.AuxInfo = value;
            }
        }

        /// <summary>
        /// An image of the game when metadata was recorded
        /// </summary>
        public Texture2D Screenshot
        {
            get => _Metadata.Screenshot;
            set
            {
                _Metadata.Screenshot = value;
            }
        }
    }
}
