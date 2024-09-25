using MRuby.Library;
using MRuby.Library.Language;

namespace RGSSUnity.RubyClasses
{
    public static class RbClassExtension
    {
        public static RbValue NewObjectWithRData<T>(this RbClass cls, T data, params RbValue[] args)
            where T : RubyData
        {
            var obj = cls.NewObjectWithCSharpDataObject(RubyData.RDataName, data, Release, args);
            return obj;
        }

        private static void Release(RbState state, object obj)
        {
            var rdata = (obj as RubyData)!;
            RbNativeObjectLiveKeeper<RubyData, RubyData>
                .GetOrCreateKeeper(state)
                .Release(rdata);
        }
    }

    public static class RbValueExtension
    {
        public static T GetRDataObject<T>(this RbValue value)
            where T : RubyData
        {
            var data = value.GetDataObject<T>(RubyData.RDataName)!;
            return data;
        }
    }

    public abstract class RubyData
    {
        public const string RDataName = "RData";

        protected RubyData(RbState state)
        {
            RbNativeObjectLiveKeeper<RubyData, RubyData>
                .GetOrCreateKeeper(state)
                .Keep(this);
        }
    }
    
    public static class RbStateExtension
    {
        public static void RaiseRGSSError(this RbState state, string message)
        {
            var exceptionClass = state.GetExceptionClass("RGSSError");
            var exc = state.GenerateExceptionWithNewStr(exceptionClass, message);
            state.Raise(exc);
        }
    }
}