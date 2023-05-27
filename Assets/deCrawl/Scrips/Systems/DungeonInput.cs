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

        void EnsureNoDupes(KeyCode[] alternatives)
        {
            for (int i = 0; i<alternatives.Length; i++)
            {
                var primary = alternatives[i];
                for (int j = i + 1; j<alternatives.Length; j++)
                {
                    if (alternatives[j] == primary)
                    {
                        alternatives[j] = KeyCode.None;
                    }
                }
            }
        }

        public void SetKey(InputEvent input, int index, KeyCode keyCode)
        {
            switch (input)
            {
                case InputEvent.MoveForward:
                    ForwardKeys[index] = keyCode;
                    EnsureNoDupes(ForwardKeys);
                    break;
                case InputEvent.MoveBackwards:
                    BackwardKeys[index] = keyCode;
                    EnsureNoDupes(BackwardKeys);
                    break;
                case InputEvent.StrafeLeft:
                    LeftKeys[index] = keyCode;
                    EnsureNoDupes(LeftKeys);
                    break;
                case InputEvent.StrafeRight:
                    RightKeys[index] = keyCode;
                    EnsureNoDupes(RightKeys);
                    break;
                case InputEvent.TurnClockWise:
                    TurnClockWiseKeys[index] = keyCode;
                    EnsureNoDupes(TurnClockWiseKeys);
                    break;
                case InputEvent.TurnCounterClockWise:
                    TurnCounterClockWiseKeys[index] = keyCode;
                    EnsureNoDupes(TurnCounterClockWiseKeys);
                    break;
                case InputEvent.Pause:
                    PauseKeys[index] = keyCode;
                    EnsureNoDupes(PauseKeys);
                    break;
                case InputEvent.Select:
                    SelectKeys[index] = keyCode;
                    EnsureNoDupes(SelectKeys);
                    break;
                case InputEvent.Abort:
                    AbortKeys[index] = keyCode;
                    EnsureNoDupes(AbortKeys);
                    break;
                case InputEvent.Inventory:
                    InventoryKeys[index] = keyCode;
                    EnsureNoDupes(InventoryKeys);
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
