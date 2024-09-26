using System;
using MRuby.Library.Language;
using MRuby.Library.Mapper;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using JetBrains.Annotations;
using UnityEngine.Networking;

namespace RGSSUnity.RubyClasses
{
    [RbModule("Unity", "")]
    public static class UnityModule
    {
        [CanBeNull]
        private static RbValue UpdateFiber = null;

        private static List<(long ScriptId, string ScriptName, string ScriptContent)> RmvaScripts = new();
        private static RbState State;

        [RbInitEntryPoint]
        public static void Init(RbClass cls)
        {
            State = RubyScriptManager.Instance.State;
        }

        public static void Update()
        {
            // when waiting, do not update logic only update rendering
            if (Graphics.WaitCount > 0)
            {
                Graphics.Render();
                return;
            }

            if (UpdateFiber != null)
            {
                if (State.CheckFiberAlive(UpdateFiber).IsTrue)
                {
                    bool error = false;
                    var res = State.Protect((_, _, _) =>
                    {
                        return State.FiberResume(UpdateFiber);
                    }, ref error, out var func);

                    if (error)
                    {
                        RGSSLogger.LogError("Failed to resume update fiber");
                        var msg = res.CallMethod("msg").ToString();
                        RGSSLogger.LogError(msg);
                    }
                    GC.KeepAlive(func);
                }
            }
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
            var traceStr = exceptionString.CallMethod("to_s");
            var message = traceStr.ToString();
            RGSSLogger.LogError($"Unhandled Ruby exception happened in script:\n {message}");

            return state.RbNil;
        }

        [RbClassMethod("msgbox")]
        private static RbValue MessageBox(RbState state, RbValue self, RbValue[] args)
        {
            Array.ForEach(args, v =>
            {
                var str = v.CallMethod("to_s");
                var message = str.ToString();
                RGSSLogger.Log(message);
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

            RmvaScripts.Add((id, name, scriptString));

            return state.RbNil;
        }

        [RbClassMethod("run_rmva_scripts")]
        private static RbValue RunRmvaScripts(RbState state, RbValue self)
        {
            var inst = RubyScriptManager.Instance;

            // load ext scripts
            var res = inst.LoadAllScriptInResources("ext", out var error);
            if (error)
            {
                state.Raise(res);
                return res;
            }

            // load RPG scripts
            res = inst.LoadAllScriptInResources("rpg", out error);
            if (error)
            {
                state.Raise(res);
                return res;
            }

            foreach (var (scriptId, scriptName, scriptContent) in RmvaScripts)
            {
                RGSSLogger.Log($"run rmva script: {scriptName}");
                res = inst.LoadScriptContentWithFileName(scriptName, scriptContent, out error);

                if (error)
                {
                    state.Raise(res);
                    return res;
                }
            }

            return state.RbNil;
        }

        [RbClassMethod("rtp_path")]
        private static RbValue GetRtpPath(RbState state, RbValue self)
        {
            return string.IsNullOrEmpty(GlobalConfig.RtpPath) ? state.RbNil : GlobalConfig.RtpPath.ToValue(state);
        }

        [RbClassMethod("exit_game")]
        private static RbValue ExitGame(RbState state, RbValue self)
        {
            Application.Quit();
            return state.RbNil;
        }

        [RbClassMethod("register_update_fiber")]
        private static RbValue RegisterUpdateFiber(RbState state, RbValue self, RbValue fiber)
        {
            if (fiber.IsFiber)
            {
                if (UpdateFiber != null)
                {
                    state.GcUnregister(UpdateFiber);
                }
                UpdateFiber = fiber;
                state.GcRegister(fiber);
            }
            return state.RbNil;
        }

        [RbClassMethod("unregister_update_fiber")]
        private static RbValue UnregisterUpdateFiber(RbState state, RbValue self)
        {
            if (UpdateFiber != null)
            {
                state.GcUnregister(UpdateFiber);
                UpdateFiber = null;
            }
            return state.RbNil;
        }
    }
}