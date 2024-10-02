using System.Collections.Generic;
using MRuby.Library;
using MRuby.Library.Language;

namespace RGSSUnity.RubyClasses
{
    using System.Text;
    using UnityEngine;

    class KernelKeeperCategory
    {
    }

    public static class Kernel
    {
        private static readonly HashSet<string> RequiredPath = new();

        public static void Init()
        {
            var stat = RubyScriptManager.Instance.State;
            var kernel = stat.GetModule("Kernel");

            var keeper = RbNativeObjectLiveKeeper<KernelKeeperCategory, NativeMethodFunc>.GetOrCreateKeeper(stat);

            kernel.DefineModuleMethod("require", Require, RbHelper.MRB_ARGS_REQ(1), out var func);
            keeper.Keep(func);

            kernel.DefineModuleMethod("msgbox", MsgBox, RbHelper.MRB_ARGS_ANY(), out func);
            keeper.Keep(func);

            kernel.DefineModuleMethod("p", Print, RbHelper.MRB_ARGS_ANY(), out func);
            keeper.Keep(func);

            kernel.DefineModuleMethod("print", Print, RbHelper.MRB_ARGS_ANY(), out func);
            keeper.Keep(func);
        }

        public static RbValue Utf8ToGbk(RbState state, RbValue self, params RbValue[] args)
        {
            if (GlobalConfig.CnVerRmva)
            {
                var gbkBytes = RbHelper.GetRawBytesFromRbStringObject(args[0]);
                var utf8Bytes = Encoding.Convert(Encoding.UTF8, Encoding.GetEncoding("GBK"), gbkBytes);
                return RbHelper.BuildRbStringObjectFromRawBytes(state, utf8Bytes);
            }
            return args[0];
        }

        public static RbValue GbkToUtf8(RbState state, RbValue self, params RbValue[] args)
        {
            if (GlobalConfig.CnVerRmva)
            {
                var utf8Bytes = RbHelper.GetRawBytesFromRbStringObject(args[0]);
                var gbkBytes = Encoding.Convert(Encoding.GetEncoding("GBK"), Encoding.UTF8, utf8Bytes);
                return RbHelper.BuildRbStringObjectFromRawBytes(state, gbkBytes);
            }
            return args[0];
        }
        
        public static void InitForUnityEditor(RbState state)
        {
            var kernel = state.GetModule("Kernel");

            var keeper = RbNativeObjectLiveKeeper<KernelKeeperCategory, NativeMethodFunc>.GetOrCreateKeeper(state);

            kernel.DefineModuleMethod("p", PrintForUnityEditor, RbHelper.MRB_ARGS_ANY(), out var func);
            keeper.Keep(func);

            kernel.DefineModuleMethod("print", PrintForUnityEditor, RbHelper.MRB_ARGS_ANY(), out func);
            keeper.Keep(func);

            kernel.DefineModuleMethod("utf82gbk", Utf8ToGbk, RbHelper.MRB_ARGS_REQ(1), out func);
            keeper.Keep(func);
            
            kernel.DefineModuleMethod("gbk2utf8", GbkToUtf8, RbHelper.MRB_ARGS_REQ(1), out func);
            keeper.Keep(func);
        }

        public static bool IsScriptLoaded(string path)
        {
            return RequiredPath.Contains(path);
        }

        public static void AddPath(string path)
        {
            RequiredPath.Add(path);
        }

        private static RbValue Require(RbState state, RbValue self, params RbValue[] args)
        {
            var pathStr = args[0].ToStringUnchecked();
            return Require(state, pathStr);
        }

        private static RbValue Require(RbState state, string pathStr)
        {
            if (!RequiredPath.Add(pathStr))
            {
                return state.RbNil;
            }

            var res = RubyScriptManager.Instance.LoadScriptInResources(pathStr, out var error);

            if (error)
            {
                state.Raise(res);
            }

            return res;
        }

        private static RbValue MsgBox(RbState state, RbValue self, params RbValue[] args)
        {
            foreach (var arg in args)
            {
                var str = arg.CallMethod("to_s");
                var info = str.ToStringUnchecked();
                RGSSLogger.Log(info);
            }
            return state.RbNil;
        }

        private static RbValue Print(RbState state, RbValue self, params RbValue[] args)
        {
            return MsgBox(state, self, args);
        }

        private static RbValue PrintForUnityEditor(RbState state, RbValue self, params RbValue[] args)
        {
            foreach (var arg in args)
            {
                var str = arg.CallMethod("to_s");

                // For RMVA cn ver, we use GBK encoding as the script name
                if (GlobalConfig.CnVerRmva)
                {
                    var bytes = RbHelper.GetRawBytesFromRbStringObject(str);
                    var content = Encoding.GetEncoding("GBK").GetString(bytes);
                    Debug.Log(content);
                }
                else
                {
                    var content = str.ToString();
                    Debug.LogError(content);
                }
            }
            return state.RbNil;
        }
    }
}