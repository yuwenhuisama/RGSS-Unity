using MRuby.Library.Mapper;

namespace RGSSUnity.RubyClasses
{
    using MRuby.Library.Language;

    [RbModule("Input", "Unity")]
    public static class Input
    {
        [RbClassMethod("trigger?")]
        public static RbValue Trigger(RbState state, RbValue self, RbValue key)
        {
            state.RaiseNotImplementError();
            return state.RbFalse;
        }

        [RbClassMethod("press?")]
        public static RbValue Press(RbState state, RbValue self, RbValue key)
        {
            state.RaiseNotImplementError();
            return state.RbFalse;
        }

        [RbClassMethod("repeat?")]
        public static RbValue Repeat(RbState state, RbValue self, RbValue key)
        {
            state.RaiseNotImplementError();
            return state.RbFalse;
        }

        [RbClassMethod("dir4")]
        public static RbValue Dir4(RbState state, RbValue self)
        {
            state.RaiseNotImplementError();
            return state.RbNil;
        }

        [RbClassMethod("dir8")]
        public static RbValue Dir8(RbState state, RbValue self)
        {
            state.RaiseNotImplementError();
            return state.RbNil;
        }

        [RbClassMethod("update")]
        public static RbValue Update(RbState state, RbValue self)
        {
            state.RaiseNotImplementError();
            return state.RbNil;
        }
        
    }
}