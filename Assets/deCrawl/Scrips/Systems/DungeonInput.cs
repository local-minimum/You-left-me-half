using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DeCrawl.Primitives;

namespace DeCrawl.Systems
{
    public class DungeonInput : FindingSingleton<DungeonInput>
    {
        public enum InputEvent { 
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

        public static bool OverlappingTypes(InputType type, InputType kind) => (type & kind) != InputType.None;
        

        public delegate void DungeonInputEvent(InputEvent input, InputType type);

        public static event DungeonInputEvent OnInput;

        [SerializeField]
        string PreferenceStorage = "Settings.Input";

        [SerializeField]
        Dictionary<(InputEvent, int), KeyCode> MappingsRestore = new Dictionary<(InputEvent, int), KeyCode>();

        [Header("Move Forward")]        
        private KeyCode[] ForwardKeys = new KeyCode[] {  KeyCode.W, KeyCode.UpArrow };

        [Header("Move Backward")]
        private KeyCode[] BackwardKeys = new KeyCode[] { KeyCode.S, KeyCode.DownArrow } ;

        [Header("Strafe Left")]
        private KeyCode[] LeftKeys = new KeyCode[] { KeyCode.A, KeyCode.LeftArrow };

        [Header("Strafe Right")]
        private KeyCode[] RightKeys = new KeyCode[] { KeyCode.D, KeyCode.RightArrow };

        [Header("Turn Clock-Wise")]
        private KeyCode[] TurnClockWiseKeys = new KeyCode[] { KeyCode.E, KeyCode.End };

        [Header("Turn Counter Clock-Wise")]
        private KeyCode[] TurnCounterClockWiseKeys = new KeyCode[] { KeyCode.Q, KeyCode.Home };

        [Header("Inventory")]
        private KeyCode[] InventoryKeys = new KeyCode[] { KeyCode.I, KeyCode.Tab };

        [Header("Pause")]
        private KeyCode[] PauseKeys = new KeyCode[] { KeyCode.P, KeyCode.Pause };

        [Header("Abort")]
        private KeyCode[] AbortKeys = new KeyCode[] { KeyCode.Escape, KeyCode.Backspace };

        [Header("Select")]
        private KeyCode[] SelectKeys = new KeyCode[] { KeyCode.Return, KeyCode.None };

        public KeyCode GetKey(InputEvent input, int index)
        {
            switch (input)
            {
                case InputEvent.MoveForward:
                    return ForwardKeys[index];
                case InputEvent.MoveBackwards:
                    return BackwardKeys[index];
                case InputEvent.StrafeLeft:
                    return LeftKeys[index];
                case InputEvent.StrafeRight:
                    return RightKeys[index];
                case InputEvent.TurnClockWise:
                    return TurnClockWiseKeys[index];
                case InputEvent.TurnCounterClockWise:
                    return TurnCounterClockWiseKeys[index];
                case InputEvent.Pause:
                    return PauseKeys[index];
                case InputEvent.Select:
                    return SelectKeys[index];
                case InputEvent.Abort:
                    return AbortKeys[index];
                case InputEvent.Inventory:
                    return InventoryKeys[index];
                default:
                    return KeyCode.None;
            }
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

        void EnsureNoDupes(InputEvent input, KeyCode[] alternatives)
        {
            for (int i = 0; i<alternatives.Length; i++)
            {
                var primary = alternatives[i];
                for (int j = i + 1; j<alternatives.Length; j++)
                {
                    if (alternatives[j] == primary)
                    {
                        TrackResetBindings((input, j), alternatives[j]);
                        alternatives[j] = KeyCode.None;
                    }
                }
            }
        }

        void UpdateKey(KeyCode[] keys, InputEvent input, int index, KeyCode code, bool isOverride)
        {
            if (isOverride)
            {
                TrackResetBindings((input, index), keys[index]);
            }
            keys[index] = code;
            if (isOverride)
            {
                EnsureNoDupes(input, keys);
            }
        }

        public void SetKey(InputEvent input, int index, KeyCode keyCode) => SetKey(input, index, keyCode, true);
        public void SetKey(InputEvent input, int index, KeyCode keyCode, bool isOverride)
        {
            switch (input)
            {
                case InputEvent.MoveForward:
                    UpdateKey(ForwardKeys, input, index, keyCode, isOverride);
                    break;
                case InputEvent.MoveBackwards:
                    UpdateKey(BackwardKeys, input, index, keyCode, isOverride);
                    break;
                case InputEvent.StrafeLeft:
                    UpdateKey(LeftKeys, input, index, keyCode, isOverride);
                    break;
                case InputEvent.StrafeRight:
                    UpdateKey(RightKeys, input, index, keyCode, isOverride);
                    break;
                case InputEvent.TurnClockWise:
                    UpdateKey(TurnClockWiseKeys, input, index, keyCode, isOverride);
                    break;
                case InputEvent.TurnCounterClockWise:
                    UpdateKey(TurnCounterClockWiseKeys, input, index, keyCode, isOverride);
                    break;
                case InputEvent.Pause:
                    UpdateKey(PauseKeys, input, index, keyCode, isOverride);
                    break;
                case InputEvent.Select:
                    UpdateKey(SelectKeys, input, index, keyCode, isOverride);
                    break;
                case InputEvent.Abort:
                    UpdateKey(AbortKeys, input, index, keyCode, isOverride);
                    break;
                case InputEvent.Inventory:
                    UpdateKey(InventoryKeys, input, index, keyCode, isOverride);
                    break;
                default:
                    Debug.LogWarning($"Don't know how to set {input}[{index}] = {keyCode}");
                    break;
            }
        }

        void EmitFor(KeyCode[] codes, InputEvent input)
        {
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
    }
}
