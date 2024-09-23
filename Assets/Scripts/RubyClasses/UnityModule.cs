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
    using JetBrains.Annotations;
    using Unity.VisualScripting;
    using UnityEngine.Networking;

    [System.Serializable]
    internal class RMConfig
    {
        public string rtp_path;
        public string project_path;
    }

    [RbModule("Unity", "")]
    public static class UnityModule
    {
        [CanBeNull]
        private static RbValue OnUpdateProc = null;
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
                    State.FiberResume(UpdateFiber);
                }
            }
            else
            {
                OnUpdateProc?.CallMethod("call");
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

            RmvaScripts.Add((id, name, scriptString));

            return state.RbNil;
        }

        [RbClassMethod("run_rmva_scripts")]
        private static RbValue RunRmvaScripts(RbState state, RbValue self)
        {
            // load RPG scripts
            var inst = RubyScriptManager.Instance;
            var res = inst.LoadAllScriptInResources("rpg", out var error);
            if (error)
            {
                state.Raise(res);
                return res;
            }

            foreach (var (scriptId, scriptName, scriptContent) in RmvaScripts)
            {
                Debug.Log($"run rmva script: {scriptName}");
                res = inst.LoadScriptContentWithFileName(scriptName, scriptContent);

                if (res.IsException)
                {
                    state.Raise(res);
                    return res;
                }
            }

            // run patch script
            res = inst.LoadScriptInResources("patch_rmva");
            return res;
        }

        [RbClassMethod("rtp_path")]
        private static RbValue GetRtpPath(RbState state, RbValue self)
        {
            // read text via UnityWebRequest
            var path = Path.Join(Application.streamingAssetsPath, "rm_conf.json");
            using UnityWebRequest www = UnityWebRequest.Get(path);
            www.SendWebRequest();
            while (!www.isDone)
            {
            }

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Failed to read rm_conf.json: {www.error}");
                var errorCls = state.GetExceptionClass("RGSSError");
                var exc = state.GenerateExceptionWithNewStr(errorCls, "Failed to load RM config");
                state.Raise(exc);
                return state.RbNil;
            }

            var res = www.downloadHandler.text;
            var json = JsonUtility.FromJson<RMConfig>(res);
            return string.IsNullOrEmpty(json.rtp_path) ? string.Empty.ToValue(state) : json.rtp_path.ToValue(state);
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