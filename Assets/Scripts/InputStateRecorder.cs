namespace RGSSUnity
{
    using System;

    internal class InputStateRecorder
    {
        internal enum InputKey
        {
            DOWN = 0,
            LEFT,
            RIGHT,
            UP,

            A,
            B,
            C,
            X,
            Y,
            Z,
            L,
            R,

            SHIFT,
            CTRL,
            ALT,

            F5,
            F6,
            F7,
            F8,
            F9,
        }

        [Flags]
        internal enum Direction
        {
            None = 0,
            L = 1,
            R = 2,
            U = 4,
            D = 8,
        }

        private struct InputState
        {
            public bool Triggered { get; set; }
            public bool Pressed { get; set; }
            public bool Repeated { get; set; }
            public int RepeatCount { get; set; }
        }

        public static readonly InputStateRecorder Instance = new InputStateRecorder();

        private InputState[] previousKeyState;
        private InputState[] keyState;

        internal void Init()
        {
            var keys = (InputKey[])System.Enum.GetValues(typeof(InputKey));
            this.keyState = new InputState[keys.Length];
            this.previousKeyState = new InputState[keys.Length];
        }

        internal void SetPress(InputKey key)
        {
            this.keyState[(int)key].Pressed = true;
            this.keyState[(int)key].Repeated = true;
            if (!this.previousKeyState[(int)key].Pressed)
            {
                this.keyState[(int)key].Triggered = true;
            }
        }

        internal void SetRelease(InputKey key)
        {
            this.keyState[(int)key].Pressed = false;
            this.keyState[(int)key].Repeated = false;
            this.keyState[(int)key].RepeatCount = 0;
            this.keyState[(int)key].Pressed = false;
        }

        internal Direction GetDir4()
        {
            if (InputStateRecorder.Instance.IsPressed(InputStateRecorder.InputKey.UP))
            {
                return Direction.U;
            }
            
            if (InputStateRecorder.Instance.IsPressed(InputStateRecorder.InputKey.DOWN))
            {
                return Direction.D;
            }
            
            if (InputStateRecorder.Instance.IsPressed(InputStateRecorder.InputKey.LEFT))
            {
                return Direction.L;
            }
            
            if (InputStateRecorder.Instance.IsPressed(InputStateRecorder.InputKey.RIGHT))
            {
                return Direction.R;
            }
            
            return Direction.None;
        }
        
        internal Direction GetDir8()
        {
            if (InputStateRecorder.Instance.IsPressed(InputStateRecorder.InputKey.UP))
            {
                if (InputStateRecorder.Instance.IsPressed(InputStateRecorder.InputKey.LEFT))
                {
                    return Direction.U | Direction.L;
                }
                
                if (InputStateRecorder.Instance.IsPressed(InputStateRecorder.InputKey.RIGHT))
                {
                    return Direction.U | Direction.R;
                }
                
                return Direction.U;
            }
            
            if (InputStateRecorder.Instance.IsPressed(InputStateRecorder.InputKey.DOWN))
            {
                if (InputStateRecorder.Instance.IsPressed(InputStateRecorder.InputKey.LEFT))
                {
                    return Direction.D | Direction.L;
                }
                
                if (InputStateRecorder.Instance.IsPressed(InputStateRecorder.InputKey.RIGHT))
                {
                    return Direction.D | Direction.R;
                }
                
                return Direction.D;
            }
            
            if (InputStateRecorder.Instance.IsPressed(InputStateRecorder.InputKey.LEFT))
            {
                return Direction.L;
            }
            
            if (InputStateRecorder.Instance.IsPressed(InputStateRecorder.InputKey.RIGHT))
            {
                return Direction.R;
            }
            
            return Direction.None;
        }
        
        private void Refresh()
        {
            for (int i = 0; i < this.keyState.Length; i++)
            {
                ref var state = ref this.keyState[i];
                state.Triggered = false;

                if (state.Pressed)
                {
                    ++state.RepeatCount;
                    state.Repeated = (state.RepeatCount >= 23) && ((state.RepeatCount + 1) % 6 == 0);
                }
            }
        }

        internal void Update()
        {
            Array.Copy(this.keyState, this.previousKeyState, this.keyState.Length);
            this.Refresh();
        }

        internal bool IsTriggered(InputKey key)
        {
            return this.previousKeyState[(int)key].Triggered;
        }

        internal bool IsPressed(InputKey key)
        {
            return this.previousKeyState[(int)key].Pressed;
        }

        internal bool IsRepeated(InputKey key)
        {
            return this.previousKeyState[(int)key].Repeated;
        }
    }
}