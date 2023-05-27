using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DeCrawl.Primitives;
using DeCrawl.Utils;

namespace DeCrawl.Systems
{
    public class DungeonInput : FindingSingleton<DungeonInput>
    {
        public enum InputEvent { 
            None,
            MoveForward,
            MoveBackwards, 
            StrafeLeft,
            StrafeRight,
            TurnClockWise,
            TurnCounterClockWise,
            Inventory,
            Pause,
            Abort,
            Select
        }

        [System.Flags]
        public enum InputType {
            None = 0b_0000_0000,
            Down = 0b_0000_0001,
            Held = 0b_0000_0010,
            Up = 0b_0000_0100,
            Click = Down | Up
        }

        [System.Serializable]
        public class InputMapping
        {
            public KeyCode Primary = KeyCode.None;
            public KeyCode Secondary = KeyCode.None;

            public InputMapping(KeyCode primary, KeyCode secondary)
            {
                Primary = primary;
                Secondary = secondary;
            }

            public int Length => 2;

            public KeyCode this[int index]
            {
                get
                {
                    if (index == 0) return Primary;
                    if (index == 1) return Secondary;
                    throw new System.IndexOutOfRangeException("Index must be either 0 or 1");
                }

                set
                {
                    if (index == 0)
                    {
                        Primary = value;
                    } else if (index == 1)
                    {
                        Secondary = value;
                    } else
                    {
                        throw new System.IndexOutOfRangeException("Index must be either 0 or 1");
                    }
                }
            }

            public string ToJSON() => JsonUtility.ToJson(this);

            public static InputMapping FromJSON(string json) => JsonUtility.FromJson<InputMapping>(json);
        }


        [System.Serializable]
        public class StoredMappings
        {
            public List<string> Inputs;
            public List<string> SerializedMappings;
            
            public StoredMappings()
            {
                Inputs = new List<string>();
                SerializedMappings = new List<string>();
            }

            public (string, string) this[int index]
            {
                get => (Inputs[index], SerializedMappings[index]);
            }


            public int Count
            {
                get => Mathf.Min(Inputs.Count, SerializedMappings.Count);
            }

            public void Add(InputEvent input, InputMapping mapping)
            {
                if (SerializedMappings == null)
                {
                    SerializedMappings = new List<string>();
                }
                if (Inputs == null)
                {
                    Inputs = new List<string>();
                }
                Inputs.Add(input.ToString());
                SerializedMappings.Add(mapping.ToJSON());
            }
        }

        public static bool OverlappingTypes(InputType type, InputType kind) => (type & kind) != InputType.None;
        

        public delegate void DungeonInputEvent(InputEvent input, InputType type);

        public static event DungeonInputEvent OnInput;

        [SerializeField]
        string PreferenceStorage = "Settings.Input";

        [SerializeField]
        Dictionary<(InputEvent, int), KeyCode> MappingsRestore = new Dictionary<(InputEvent, int), KeyCode>();

        [SerializeField]        
        private InputMapping ForwardKeys = new InputMapping(KeyCode.W, KeyCode.UpArrow);

        [SerializeField]
        private InputMapping BackwardKeys = new InputMapping(KeyCode.S, KeyCode.DownArrow);

        [SerializeField]
        private InputMapping LeftKeys = new InputMapping(KeyCode.A, KeyCode.LeftArrow);

        [SerializeField]
        private InputMapping RightKeys = new InputMapping(KeyCode.D, KeyCode.RightArrow);

        [SerializeField]
        private InputMapping TurnClockWiseKeys = new InputMapping(KeyCode.E, KeyCode.End);

        [SerializeField]
        private InputMapping TurnCounterClockWiseKeys = new InputMapping(KeyCode.Q, KeyCode.Home);

        [SerializeField]
        private InputMapping InventoryKeys = new InputMapping(KeyCode.I, KeyCode.Tab);

        [SerializeField]
        private InputMapping PauseKeys = new InputMapping(KeyCode.P, KeyCode.Pause);

        [SerializeField]
        private InputMapping AbortKeys = new InputMapping(KeyCode.Escape, KeyCode.Backspace);

        [SerializeField]
        private InputMapping SelectKeys = new InputMapping(KeyCode.Return, KeyCode.None);

        private InputMapping GetMapping(InputEvent input)
        {
            switch (input)
            {
                case InputEvent.MoveForward:
                    return ForwardKeys;
                case InputEvent.MoveBackwards:
                    return BackwardKeys;
                case InputEvent.StrafeLeft:
                    return LeftKeys;
                case InputEvent.StrafeRight:
                    return RightKeys;
                case InputEvent.TurnClockWise:
                    return TurnClockWiseKeys;
                case InputEvent.TurnCounterClockWise:
                    return TurnCounterClockWiseKeys;
                case InputEvent.Pause:
                    return PauseKeys;
                case InputEvent.Select:
                    return SelectKeys;
                case InputEvent.Abort:
                    return AbortKeys;
                case InputEvent.Inventory:
                    return InventoryKeys;
                default:
                    return null;
            }
        }

        public KeyCode GetKey(InputEvent input, int index)
        {
            var mapping = GetMapping(input);
            if (mapping == null) return KeyCode.None;
            return mapping[index];
        }

        void TrackResetBindings((InputEvent, int) key, KeyCode currentValue)
        {
            if (!MappingsRestore.ContainsKey(key))
            {
                MappingsRestore.Add(key, currentValue);
            }
        }

        public void ResetKeyBindings()
        {
            foreach (var kvp in MappingsRestore)
            {
                SetKey(kvp.Key.Item1, kvp.Key.Item2, kvp.Value, false);
            }
            PlayerPrefs.DeleteKey(PreferenceStorage);
            MappingsRestore.Clear();
        }

        public bool HasCustomBindings => MappingsRestore.Count > 0;

        void EnsureNoDupes(InputEvent input, InputMapping inputMapping)
        {
            for (int i = 0; i<inputMapping.Length; i++)
            {
                var primary = inputMapping[i];
                for (int j = i + 1; j<inputMapping.Length; j++)
                {
                    if (inputMapping[j] == primary)
                    {
                        TrackResetBindings((input, j), inputMapping[j]);
                        inputMapping[j] = KeyCode.None;
                    }
                }
            }
        }

        void UpdateKey(InputMapping inputMapping, InputEvent input, int index, KeyCode code, bool isOverride)
        {
            if (isOverride)
            {
                TrackResetBindings((input, index), inputMapping[index]);
            }
            inputMapping[index] = code;
            if (isOverride)
            {
                EnsureNoDupes(input, inputMapping);
                StoreUserMappings();
            }
        }

        static InputEvent[] _allInputs;

        static InputEvent[] AllInputs
        {
            get
            {
                if (_allInputs == null)
                {
                    _allInputs = (InputEvent[])System.Enum.GetValues(typeof(InputEvent));
                }
                return _allInputs;
            }
        }

        static InputEvent InputFromString(string sInput)
        {
            foreach (var input in AllInputs)
            {
                if (input.ToString() == sInput)
                {
                    return input;
                }
            }

            return InputEvent.None;
        }

        void StoreUserMappings()
        {
            var settings = new StoredMappings();
            var events = AllInputs;
            for (int i=0; i<events.Length; i++)
            {
                if (events[i] == InputEvent.None) continue;

                var mappings = GetMapping(events[i]);
                if (mappings != null)
                {
                    settings.Add(events[i], mappings);
                }
            }

            PlayerPrefs.SetString(PreferenceStorage, JsonUtility.ToJson(settings));
        }

        void LoadUserMappings()
        {
            var settingsString = PlayerPrefs.GetString(PreferenceStorage);
            if (string.IsNullOrEmpty(settingsString)) return;

            var settings = JsonUtility.FromJson<StoredMappings>(settingsString);

            for (int i=0, l=settings.Count; i<l; i++)
            {
                var (sInput, sMapping) = settings[i];

                var input = InputFromString(sInput);
                var mapping = InputMapping.FromJSON(sMapping);

                if (input != InputEvent.None)
                {
                    Debug.Log($"Loading key-binding '{input}': {mapping.ToJSON()}");
                    for (int j = 0; j < mapping.Length; j++)
                    {
                        SetKey(input, j, mapping[j]);
                    }
                } else
                {
                    Debug.LogWarning($"Could not understand stored key-binding {input}");
                }
            }
        }

        public void SetKey(InputEvent input, int index, KeyCode keyCode) => SetKey(input, index, keyCode, true);
        public void SetKey(InputEvent input, int index, KeyCode keyCode, bool isOverride)
        {
            if (input == InputEvent.None) return;

            var mapping = GetMapping(input);
            if (mapping == null)
            {
                Debug.LogWarning($"Don't know how to set {input}[{index}] = {keyCode}");
            } else
            {
                UpdateKey(mapping, input, index, keyCode, isOverride);
            }
        }

        void EmitFor(InputMapping codes, InputEvent input)
        {
            if (input == InputEvent.None) return;

            for (int i = 0; i<codes.Length; i++)
            {
                var code = codes[i];
                var up = Input.GetKeyUp(code);
                var down = Input.GetKeyDown(code);
                if (up && down)
                {
                    OnInput?.Invoke(input, InputType.Click);
                } else if (up)
                {
                    OnInput?.Invoke(input, InputType.Up);
                } else if (down)
                {
                    OnInput?.Invoke(input, InputType.Down);
                } else if (Input.GetKey(code))
                {
                    OnInput?.Invoke(input, InputType.Held);
                }
            }
        }

        /// <summary>
        /// Emits a click event
        /// </summary>
        /// <param name="input"></param>
        public void Click(InputEvent input)
        {
            OnInput?.Invoke(input, InputType.Click);
        }

        private void Update()
        {
            EmitFor(ForwardKeys, InputEvent.MoveForward);
            EmitFor(BackwardKeys, InputEvent.MoveBackwards);
            EmitFor(LeftKeys, InputEvent.StrafeLeft);
            EmitFor(RightKeys, InputEvent.StrafeRight);
            EmitFor(TurnClockWiseKeys, InputEvent.TurnClockWise);
            EmitFor(TurnCounterClockWiseKeys, InputEvent.TurnCounterClockWise);
            EmitFor(InventoryKeys, InputEvent.Inventory);
            EmitFor(PauseKeys, InputEvent.Pause);
            EmitFor(AbortKeys, InputEvent.Abort);
            EmitFor(SelectKeys, InputEvent.Select);            
        }

        private void Start()
        {
            LoadUserMappings();
        }
    }
}
