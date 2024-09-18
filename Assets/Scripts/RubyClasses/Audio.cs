using MRuby.Library.Mapper;

namespace RGSSUnity.RubyClasses
{
    using MRuby.Library.Language;

    [RbModule("Audio", "Unity")]
    public static class Audio
    {
        [RbModuleMethod("setup_midi")]
        public static RbValue SetupMidi(RbState state, RbValue self)
        {
            state.RaiseNotImplementError();
            return state.RbNil;
        }
        
        [RbModuleMethod("bgm_play")]
        public static RbValue BgmPlay(RbState state, RbValue self, RbValue volume, RbValue pos)
        {
            state.RaiseNotImplementError();
            return state.RbNil;
        }
        
        [RbModuleMethod("bgm_stop")]
        public static RbValue BgmStop(RbState state, RbValue self)
        {
            state.RaiseNotImplementError();
            return state.RbNil;
        }
        
        [RbModuleMethod("bgm_pos")]
        public static RbValue BgmPos(RbState state, RbValue self)
        {
            state.RaiseNotImplementError();
            return state.RbNil;
        }
        
        [RbModuleMethod("bgm_fade")]
        public static RbValue BgmFade(RbState state, RbValue self, RbValue time)
        {
            state.RaiseNotImplementError();
            return state.RbNil;
        }
        
        [RbModuleMethod("bgs_play")]
        public static RbValue BgsPlay(RbState state, RbValue self, RbValue volume, RbValue pos)
        {
            state.RaiseNotImplementError();
            return state.RbNil;
        }
        
        [RbModuleMethod("bgs_stop")]
        public static RbValue BgsStop(RbState state, RbValue self)
        {
            state.RaiseNotImplementError();
            return state.RbNil;
        }
        
        [RbModuleMethod("bgs_pos")]
        public static RbValue BgsPos(RbState state, RbValue self)
        {
            state.RaiseNotImplementError();
            return state.RbNil;
        }
        
        [RbModuleMethod("bgs_fade")]
        public static RbValue BgsFade(RbState state, RbValue self, RbValue time)
        {
            state.RaiseNotImplementError();
            return state.RbNil;
        }
        
        [RbModuleMethod("me_play")]
        public static RbValue MePlay(RbState state, RbValue self, RbValue volume, RbValue pos)
        {
            state.RaiseNotImplementError();
            return state.RbNil;
        }
        
        [RbModuleMethod("me_stop")]
        public static RbValue MeStop(RbState state, RbValue self)
        {
            state.RaiseNotImplementError();
            return state.RbNil;
        }
        
        [RbModuleMethod("me_fade")]
        public static RbValue MeFade(RbState state, RbValue self, RbValue time)
        {
            state.RaiseNotImplementError();
            return state.RbNil;
        }

        [RbModuleMethod("se_play")]
        public static RbValue SePlay(RbState state, RbValue self, RbValue volume, RbValue pos)
        {
            state.RaiseNotImplementError();
            return state.RbNil;
        }
        
        [RbModuleMethod("se_stop")]
        public static RbValue SeStop(RbState state, RbValue self)
        {
            state.RaiseNotImplementError();
            return state.RbNil;
        }
    }
}