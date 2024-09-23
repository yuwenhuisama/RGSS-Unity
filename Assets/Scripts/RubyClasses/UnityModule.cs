using System;
using MRuby.Library.Language;
using MRuby.Library.Mapper;
using System.Collections.Generic;
using UnityEngine;
using ICSharpCode.SharpZipLib;

namespace RGSSUnity.RubyClasses
{
    using System.IO;
    using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

    [RbModule("Unity", "")]
    public static class UnityModule
    {
        private static RbValue OnUpdateProc = null!;
        private static List<(long ScriptId, string ScriptName, string ScriptContent)> RmvaScripts = new();

        [RbInitEntryPoint]
        public static void Init(RbClass cls)
        {
            var state = RubyScriptManager.Instance.State;
        }

        public static void Update()
        {
            // when waiting, do not update logic only update rendering
            if (Graphics.WaitCount > 0)
            {
                Graphics.Render();
                return;
            }

            OnUpdateProc?.CallMethod("call");
        }

        [RbClassMethod("rmva_project_path")]
        private static RbValue GetRmvaProjectPath(RbState state, RbValue self)
        {
            var path = Application.streamingAssetsPath;
            return path.ToValue(state);
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
            if (OnUpdateProc != null)
            {
                state.GcUnregister(OnUpdateProc);
            }
            OnUpdateProc = val;
            state.GcRegister(OnUpdateProc);

            return state.RbNil;
        }

        [RbClassMethod("on_update")]
        private static RbValue GetOnUpdate(RbState state, RbValue self)
        {
            return OnUpdateProc ?? state.RbNil;
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

        [RbClassMethod("register_rmva_script")]
        private static RbValue RegisterRmvaScript(RbState state, RbValue self, RbValue scriptId, RbValue scriptName, RbValue scriptContent)
        {
            var id = scriptId.ToIntUnchecked();
            var name = scriptName.ToStringUnchecked();
            var bytes = RbHelper.GetRawBytesFromRbStringObject(scriptContent);

            // use ICSharpCode.SharpZipLib to inflate bytes
            using var inputStream = new MemoryStream(bytes);
            using var inflaterStream = new InflaterInputStream(inputStream);
            using var outputStream = new MemoryStream();
            inflaterStream.CopyTo(outputStream);
            var scriptString = System.Text.Encoding.UTF8.GetString(outputStream.ToArray());

            return state.RbNil;
        }
    }
}