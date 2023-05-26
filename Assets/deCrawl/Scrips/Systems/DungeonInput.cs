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
        public KeyCode[] ForwardKeys = new KeyCode[] {  KeyCode.W, KeyCode.UpArrow };

        [Header("Move Backward")]
        public KeyCode[] BackwardKeys = new KeyCode[] { KeyCode.S, KeyCode.DownArrow } ;

        [Header("Strafe Left")]
        public KeyCode[] LeftKeys = new KeyCode[] { KeyCode.A, KeyCode.LeftArrow };

        [Header("Strafe Right")]
        public KeyCode[] RightKeys = new KeyCode[] { KeyCode.D, KeyCode.RightArrow };

        [Header("Turn Clock-Wise")]
        public KeyCode[] TurnClockWiseKeys = new KeyCode[] { KeyCode.E, KeyCode.End };

        [Header("Turn Counter Clock-Wise")]
        public KeyCode[] TurnCounterClockWiseKeys = new KeyCode[] { KeyCode.Q, KeyCode.Home };

        [Header("Inventory")]
        public KeyCode[] InventoryKeys = new KeyCode[] { KeyCode.I, KeyCode.Tab };

        [Header("Pause")]
        public KeyCode[] PauseKeys = new KeyCode[] { KeyCode.P, KeyCode.Pause };

        [Header("Abort")]
        public KeyCode[] AbortKeys = new KeyCode[] { KeyCode.Escape, KeyCode.Backspace };

        [Header("Select")]
        public KeyCode[] SelectKeys = new KeyCode[] { KeyCode.Return };

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
