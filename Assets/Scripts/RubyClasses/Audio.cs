using MRuby.Library.Mapper;

namespace RGSSUnity.RubyClasses
{
    using MRuby.Library.Language;

    [RbModule("Audio", "Unity")]
    public static class Audio
    {
        private static GameAudioManager Instance_;
        private static RbValue ExceptionHandler_;

        [RbInitEntryPoint]
        public static void Init(RbClass cls)
        {
            Instance_ = GameAudioManager.Instance;
        }

        [RbModuleMethod("setup_midi")]
        public static RbValue SetupMidi(RbState state, RbValue self)
        {
            state.RaiseNotImplementError();
            return state.RbNil;
        }

        [RbModuleMethod("bgm_play")]
        public static RbValue BgmPlay(RbState state, RbValue self, RbValue filename, RbValue volume, RbValue pitch, RbValue pos, RbValue onLoadedProc)
        {
            var volumnVal = volume.ToIntUnchecked() / 100.0f;
            var pitchVal = pitch.ToIntUnchecked() / 100.0f;
            var posVal = pos.IsInt ? pos.ToIntUnchecked() : (float)pos.ToFloatUnchecked();
            var filenameVal = filename.ToStringUnchecked();

            state.GcRegister(onLoadedProc);
            Instance_.Play(GameAudioManager.PlayType.Bgm, filenameVal, volumnVal, pitchVal, posVal, succ =>
            {
                if (!succ && !onLoadedProc.IsNil)
                {
                    onLoadedProc.CallMethod("call");
                }
                state.GcUnregister(onLoadedProc);
            });
            return state.RbNil;
        }

        [RbModuleMethod("bgm_stop")]
        public static RbValue BgmStop(RbState state, RbValue self)
        {
            Instance_.Stop(GameAudioManager.PlayType.Bgm);
            return state.RbNil;
        }

        [RbModuleMethod("bgm_pos")]
        public static RbValue BgmPos(RbState state, RbValue self)
        {
            return Instance_.Pos(GameAudioManager.PlayType.Bgm).ToValue(state);
        }

        [RbModuleMethod("bgm_fade")]
        public static RbValue BgmFade(RbState state, RbValue self, RbValue time)
        {
            var timeVal = time.ToIntUnchecked();
            Instance_.Fade(GameAudioManager.PlayType.Bgm, timeVal);
            return state.RbNil;
        }

        [RbModuleMethod("bgs_play")]
        public static RbValue BgsPlay(RbState state, RbValue self, RbValue filename, RbValue volume, RbValue pitch, RbValue pos, RbValue onLoadedProc)
        {
            var volumnVal = volume.ToIntUnchecked() / 100.0f;
            var pitchVal = pitch.ToIntUnchecked() / 100.0f;
            var posVal = pos.IsInt ? pos.ToIntUnchecked() : (float)pos.ToFloatUnchecked();
            var filenameVal = filename.ToStringUnchecked();

            state.GcRegister(onLoadedProc);
            Instance_.Play(GameAudioManager.PlayType.Bgs, filenameVal, volumnVal, pitchVal, posVal, succ =>
            {
                if (!succ && !onLoadedProc.IsNil)
                {
                    onLoadedProc.CallMethod("call");
                }
                state.GcUnregister(onLoadedProc);
            });

            return state.RbNil;
        }

        [RbModuleMethod("bgs_stop")]
        public static RbValue BgsStop(RbState state, RbValue self)
        {
            Instance_.Stop(GameAudioManager.PlayType.Bgs);
            return state.RbNil;
        }

        [RbModuleMethod("bgs_pos")]
        public static RbValue BgsPos(RbState state, RbValue self)
        {
            return Instance_.Pos(GameAudioManager.PlayType.Bgs).ToValue(state);
        }

        [RbModuleMethod("bgs_fade")]
        public static RbValue BgsFade(RbState state, RbValue self, RbValue time)
        {
            var timeVal = time.ToIntUnchecked();
            Instance_.Fade(GameAudioManager.PlayType.Bgs, timeVal);
            return state.RbNil;
        }

        [RbModuleMethod("me_play")]
        public static RbValue MePlay(RbState state, RbValue self, RbValue filename, RbValue volume, RbValue pitch, RbValue onLoadedProc)
        {
            var volumnVal = volume.ToIntUnchecked() / 100.0f;
            var pitchVal = pitch.ToIntUnchecked() / 100.0f;
            var filenameVal = filename.ToStringUnchecked();

            state.GcRegister(onLoadedProc);
            Instance_.Play(GameAudioManager.PlayType.Me, filenameVal, volumnVal, pitchVal, 0, succ =>
            {
                if (!succ && !onLoadedProc.IsNil)
                {
                    onLoadedProc.CallMethod("call");
                }
                state.GcUnregister(onLoadedProc);
            });

            return state.RbNil;
        }

        [RbModuleMethod("me_stop")]
        public static RbValue MeStop(RbState state, RbValue self)
        {
            Instance_.Stop(GameAudioManager.PlayType.Me);
            return state.RbNil;
        }

        [RbModuleMethod("me_fade")]
        public static RbValue MeFade(RbState state, RbValue self, RbValue time)
        {
            var timeVal = time.ToIntUnchecked();
            Instance_.Fade(GameAudioManager.PlayType.Me, timeVal);
            return state.RbNil;
        }

        [RbModuleMethod("se_play")]
        public static RbValue SePlay(RbState state, RbValue self, RbValue filename, RbValue volume, RbValue pitch, RbValue onLoadedProc)
        {
            var volumnVal = volume.ToIntUnchecked() / 100.0f;
            var pitchVal = pitch.ToIntUnchecked() / 100.0f;
            var filenameVal = filename.ToStringUnchecked();

            state.GcRegister(onLoadedProc);
            Instance_.Play(GameAudioManager.PlayType.Se, filenameVal, volumnVal, pitchVal, 0, succ =>
            {
                if (!succ && !onLoadedProc.IsNil)
                {
                    onLoadedProc.CallMethod("call");
                }
                state.GcUnregister(onLoadedProc);
            });
            return state.RbNil;
        }

        [RbModuleMethod("se_stop")]
        public static RbValue SeStop(RbState state, RbValue self)
        {
            Instance_.Stop(GameAudioManager.PlayType.Se);
            return state.RbNil;
        }

        [RbClassMethod("__set_exception_handler__")]
        public static RbValue SetExceptionHandler(RbState state, RbValue self, RbValue handler)
        {
            if (ExceptionHandler_ != null)
            {
                state.GcUnregister(ExceptionHandler_);
            }
            ExceptionHandler_ = handler;
            state.GcRegister(handler);
            return state.RbNil;
        }
    }
}