using MRuby.Library.Language;
using MRuby.Library.Mapper;

namespace RGSSUnity.RubyClasses
{
    public class ColorData : RubyData
    {
        public UnityEngine.Color Color;

        public ColorData(RbState state) : base(state)
        {
        }
    }

    [RbClass("Color", "Object", "Unity")]
    public static class Color
    {
        public static RbValue CreateColor(RbState state, long rVal, long gVal, long bVal, long aVal)
        {
            var color = new UnityEngine.Color(rVal / 255.0f, gVal / 255.0f, bVal / 255.0f, aVal / 255.0f);
            var stub = new ColorData(state) { Color = color };
            
            var colorCls = RubyScriptManager.Instance.GetClassUnderUnityModule("Color");
            var res = colorCls.NewObjectWithRData(stub);
            return res;
        }
        
        [RbClassMethod("new_rgba")]
        public static RbValue NewColor(RbState state, RbValue self, RbValue r, RbValue g, RbValue b, RbValue a)
        {
            var rVal = r.ToIntUnchecked();
            var gVal = g.ToIntUnchecked();
            var bVal = b.ToIntUnchecked();
            var aVal = a.ToIntUnchecked();

            var res = CreateColor(state, rVal, gVal, bVal, aVal);
            return res;
        }
        
        [RbInstanceMethod("set_rgba")]
        public static RbValue Set(RbState state, RbValue self, RbValue r, RbValue g, RbValue b, RbValue a)
        {
            var rVal = r.ToIntUnchecked();
            var gVal = g.ToIntUnchecked();
            var bVal = b.ToIntUnchecked();
            var aVal = a.ToIntUnchecked();

            var colorData = self.GetRDataObject<ColorData>();
            colorData.Color = new UnityEngine.Color(rVal, gVal, bVal, aVal);
            return state.RbNil;
        }

        [RbInstanceMethod("r")]
        public static RbValue GetR(RbState state, RbValue self)
        {
            var colorData = self.GetRDataObject<ColorData>();
            return ((int)(colorData.Color.r * 255.0f)).ToValue(state);
        }

        [RbInstanceMethod("r=")]
        public static RbValue SetR(RbState state, RbValue self, RbValue r)
        {
            var rVal = r.ToIntUnchecked();
            var colorData = self.GetRDataObject<ColorData>();
            colorData.Color.r = rVal / 255.0f;
            return state.RbNil;
        }

        [RbInstanceMethod("g")]
        public static RbValue GetG(RbState state, RbValue self)
        {
            var colorData = self.GetRDataObject<ColorData>();
            return ((int)(colorData.Color.g * 255.0f)).ToValue(state);
        }

        [RbInstanceMethod("g=")]
        public static RbValue SetG(RbState state, RbValue self, RbValue g)
        {
            var gVal = g.ToIntUnchecked();
            var colorData = self.GetRDataObject<ColorData>();
            colorData.Color.g = gVal / 255.0f;
            return state.RbNil;
        }

        [RbInstanceMethod("b")]
        public static RbValue GetB(RbState state, RbValue self)
        {
            var colorData = self.GetRDataObject<ColorData>();
            return ((int)(colorData.Color.b * 255.0f)).ToValue(state);
        }

        [RbInstanceMethod("b=")]
        public static RbValue SetB(RbState state, RbValue self, RbValue b)
        {
            var bVal = b.ToIntUnchecked();
            var colorData = self.GetRDataObject<ColorData>();
            colorData.Color.b = bVal * 255.0f;
            return state.RbNil;
        }

        [RbInstanceMethod("a")]
        public static RbValue GetA(RbState state, RbValue self)
        {
            var colorData = self.GetRDataObject<ColorData>();
            return ((int)(colorData.Color.a * 255.0f)).ToValue(state);
        }

        [RbInstanceMethod("a=")]
        public static RbValue SetA(RbState state, RbValue self, RbValue a)
        {
            var aVal = a.ToIntUnchecked();
            var colorData = self.GetRDataObject<ColorData>();
            colorData.Color.a = aVal * 255.0f;
            return state.RbNil;
        }
    }
}