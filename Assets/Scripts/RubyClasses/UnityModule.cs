using System;
using MRuby.Library.Language;
using MRuby.Library.Mapper;

namespace RGSSUnity.RubyClasses
{
    using MRuby.Library;
    using UnityEngine;

    class UnityModuleCategory
    {
    }

    [RbModule("Unity", "")]
    public static class UnityModule
    {
        private static RbValue onUpdateProc = null!;

        [RbInitEntryPoint]
        public static void Init(RbClass cls)
        {
            var state = RubyScriptManager.Instance.State;

            onUpdateProc = state.NewProc((stat, self, args) => state.RbNil, out var callFn).ToValue();

            RbNativeObjectLiveKeeper<UnityModuleCategory, NativeMethodFunc>.GetOrCreateKeeper(state).Keep("OnUpdate", callFn);
            state.GcRegister(onUpdateProc);
        }

        public static void Update()
        {
            // when waiting, do not update logic only update rendering
            if (Graphics.WaitCount > 0)
            {
                Graphics.Render();
                return;
            }

            onUpdateProc.CallMethod("call");
        }

        [RbClassMethod("on_top_exception")]
        private static RbValue OnTopExceptionHappened(RbState state, RbValue self, RbValue exceptionString)
        {
            var traceStr = state.UnboxString(exceptionString);
            Debug.LogError($"Unhandled Ruby exception happened in script:\n {traceStr}");
            return state.RbNil;
        }

        [RbClassMethod("on_update=")]
        private static RbValue SetOnUpdate(RbState state, RbValue self, RbValue val)
        {
            var keeper = RbNativeObjectLiveKeeper<UnityModuleCategory, NativeMethodFunc>.GetOrCreateKeeper(state);
            if (keeper.Contains("OnUpdate"))
            {
                keeper.Release("OnUpdate");
            }

            state.GcUnregister(onUpdateProc);
            onUpdateProc = val;
            state.GcRegister(onUpdateProc);

            return state.RbNil;
        }

        [RbClassMethod("on_update")]
        private static RbValue GetOnUpdate(RbState state, RbValue self)
        {
            return onUpdateProc;
        }

        [RbClassMethod("msgbox")]
        private static RbValue MessageBox(RbState state, RbValue self, RbValue[] args)
        {
            Array.ForEach(args, v =>
            {
                var str = v.CallMethod("to_s");
                var message = state.UnboxString(str);
                Debug.Log(message);
            });
            return state.RbNil;
        }
    }
}