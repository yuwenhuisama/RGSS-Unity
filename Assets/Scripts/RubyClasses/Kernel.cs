using System.Collections.Generic;
using MRuby.Library;
using MRuby.Library.Language;

namespace RGSSUnity.RubyClasses
{
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
    }
}