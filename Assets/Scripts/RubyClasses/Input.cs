using MRuby.Library.Mapper;

namespace RGSSUnity.RubyClasses
{
    using System;
    using System.Collections.Generic;
    using MRuby.Library.Language;

    [RbModule("Input", "Unity")]
    public static class Input
    {
        private static readonly Dictionary<ulong, InputStateRecorder.InputKey> KeyMap_ = new();
        private static InputStateRecorder InputStateRecorder_;

        [RbInitEntryPoint]
        public static void Init(RbClass cls)
        {
            KeyMap_.Clear();
            var state = cls.State;
            InputStateRecorder.InputKey[] keys = (InputStateRecorder.InputKey[])System.Enum.GetValues(typeof(InputStateRecorder.InputKey));
            foreach (var key in keys)
            {
                KeyMap_[state.GetInternSymbol(key.ToString())] = key;
            }

            InputStateRecorder_ = InputStateRecorder.Instance;
        }

        [RbClassMethod("trigger?")]
        public static RbValue Trigger(RbState state, RbValue self, RbValue key)
        {
            return InputStateRecorder_.IsTriggered(KeyMap_[state.UnboxSymbol(key)]).ToValue(state);
        }

        [RbClassMethod("press?")]
        public static RbValue Press(RbState state, RbValue self, RbValue key)
        {
            return InputStateRecorder_.IsPressed(KeyMap_[state.UnboxSymbol(key)]).ToValue(state);
        }

        [RbClassMethod("repeat?")]
        public static RbValue Repeat(RbState state, RbValue self, RbValue key)
        {
            return InputStateRecorder_.IsRepeated(KeyMap_[state.UnboxSymbol(key)]).ToValue(state);
        }

        [RbClassMethod("dir4")]
        public static RbValue Dir4(RbState state, RbValue self)
        {
            var dir = InputStateRecorder_.GetDir4();

            var dirVal = dir switch
            {
                InputStateRecorder.Direction.None => 0.ToValue(state),
                InputStateRecorder.Direction.L => 4.ToValue(state),
                InputStateRecorder.Direction.R => 6.ToValue(state),
                InputStateRecorder.Direction.U => 8.ToValue(state),
                InputStateRecorder.Direction.D => 2.ToValue(state),
                _ => throw new ArgumentOutOfRangeException()
            };

            return dirVal;
        }

        [RbClassMethod("dir8")]
        public static RbValue Dir8(RbState state, RbValue self)
        {
            var dir = InputStateRecorder_.GetDir8();

            var dirVal = dir switch
            {
                InputStateRecorder.Direction.None => 0.ToValue(state),
                InputStateRecorder.Direction.L | InputStateRecorder.Direction.U => 7.ToValue(state),
                InputStateRecorder.Direction.L | InputStateRecorder.Direction.D => 1.ToValue(state),
                InputStateRecorder.Direction.R | InputStateRecorder.Direction.U => 9.ToValue(state),
                InputStateRecorder.Direction.R | InputStateRecorder.Direction.D => 3.ToValue(state),
                InputStateRecorder.Direction.L => 4.ToValue(state),
                InputStateRecorder.Direction.R => 6.ToValue(state),
                InputStateRecorder.Direction.U => 8.ToValue(state),
                InputStateRecorder.Direction.D => 2.ToValue(state),
                _ => throw new ArgumentOutOfRangeException()
            };

            return dirVal;
        }

        [RbClassMethod("update")]
        public static RbValue Update(RbState state, RbValue self)
        {
            InputStateRecorder_.Update();
            return state.RbNil;
        }
    }
}