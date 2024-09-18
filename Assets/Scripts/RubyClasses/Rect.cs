using MRuby.Library.Mapper;

namespace RGSSUnity.RubyClasses
{
    using MRuby.Library.Language;

    public class RectData : RubyData
    {
        public UnityEngine.Rect Rect;

        public RectData(RbState state) : base(state)
        {
        }
    }

    [RbClass("Rect", "Object", "Unity")]
    public static class Rect
    {
        public static RbValue CreateRect(RbState state, long xVal, long yVal, long wVal, long hVal)
        {
            var rect = new UnityEngine.Rect(xVal, yVal, wVal, hVal);
            var stub = new RectData(state) { Rect = rect };
            var cls = RubyScriptManager.Instance.GetClassUnderUnityModule("Rect");
            var res = cls.NewObjectWithRData(stub);
            return res;
        }

        [RbClassMethod("new_xywh")]
        public static RbValue NewRect(RbState state, RbValue self, RbValue x, RbValue y, RbValue w, RbValue h)
        {
            var xVal = x.ToIntUnchecked();
            var yVal = y.ToIntUnchecked();
            var wVal = w.ToIntUnchecked();
            var hVal = h.ToIntUnchecked();

            var res = CreateRect(state, xVal, yVal, wVal, hVal);
            return res;
        }

        [RbInstanceMethod("set_xywh")]
        public static RbValue Set(RbState state, RbValue self, RbValue x, RbValue y, RbValue w, RbValue h)
        {
            var xVal = x.ToIntUnchecked();
            var yVal = y.ToIntUnchecked();
            var wVal = w.ToIntUnchecked();
            var hVal = h.ToIntUnchecked();

            var rectData = self.GetRDataObject<RectData>();
            rectData.Rect.Set(xVal, yVal, wVal, hVal);
            return state.RbNil;
        }

        [RbInstanceMethod("x")]
        public static RbValue GetX(RbState state, RbValue self)
        {
            var rectData = self.GetRDataObject<RectData>();
            return rectData.Rect.x.ToValue(state);
        }

        [RbInstanceMethod("x=")]
        public static RbValue SetX(RbState state, RbValue self, RbValue x)
        {
            var xVal = x.ToIntUnchecked();
            var rectData = self.GetRDataObject<RectData>();
            rectData.Rect.x = xVal;
            return state.RbNil;
        }

        // generate GetY SetY GetW SetW GetH SetH
        [RbInstanceMethod("y")]
        public static RbValue GetY(RbState state, RbValue self)
        {
            var rectData = self.GetRDataObject<RectData>();
            return rectData.Rect.y.ToValue(state);
        }

        [RbInstanceMethod("y=")]
        public static RbValue SetY(RbState state, RbValue self, RbValue y)
        {
            var yVal = y.ToIntUnchecked();
            var rectData = self.GetRDataObject<RectData>();
            rectData.Rect.y = yVal;
            return state.RbNil;
        }

        [RbInstanceMethod("w")]
        public static RbValue GetW(RbState state, RbValue self)
        {
            var rectData = self.GetRDataObject<RectData>();
            return rectData.Rect.width.ToValue(state);
        }

        [RbInstanceMethod("w=")]
        public static RbValue SetW(RbState state, RbValue self, RbValue w)
        {
            var wVal = w.ToIntUnchecked();
            var rectData = self.GetRDataObject<RectData>();
            rectData.Rect.width = wVal;
            return state.RbNil;
        }

        [RbInstanceMethod("h")]
        public static RbValue GetH(RbState state, RbValue self)
        {
            var rectData = self.GetRDataObject<RectData>();
            return rectData.Rect.height.ToValue(state);
        }

        [RbInstanceMethod("h=")]
        public static RbValue SetH(RbState state, RbValue self, RbValue h)
        {
            var hVal = h.ToIntUnchecked();
            var rectData = self.GetRDataObject<RectData>();
            rectData.Rect.height = hVal;
            return state.RbNil;
        }
    }
}