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
        public static RbValue CreateColor(RbState state, float rVal, float gVal, float bVal, float aVal)
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
            var rVal = r.IsInt ? r.ToIntUnchecked() : r.ToFloatUnchecked();
            var gVal = g.IsInt ? g.ToIntUnchecked() : g.ToFloatUnchecked();
            var bVal = b.IsInt ? b.ToIntUnchecked() : b.ToFloatUnchecked();
            var aVal = a.IsInt ? a.ToIntUnchecked() : a.ToFloatUnchecked();

            var res = CreateColor(state, (float)rVal, (float)gVal, (float)bVal, (float)aVal);
            return res;
        }
        
        [RbInstanceMethod("set_rgba")]
        public static RbValue Set(RbState state, RbValue self, RbValue r, RbValue g, RbValue b, RbValue a)
        {
            var rVal = r.IsInt ? r.ToIntUnchecked() : r.ToFloatUnchecked();
            var gVal = g.IsInt ? g.ToIntUnchecked() : g.ToFloatUnchecked();
            var bVal = b.IsInt ? b.ToIntUnchecked() : b.ToFloatUnchecked();
            var aVal = a.IsInt ? a.ToIntUnchecked() : a.ToFloatUnchecked();

            var colorData = self.GetRDataObject<ColorData>();
            colorData.Color = new UnityEngine.Color((float)rVal, (float)gVal, (float)bVal, (float)aVal);
            return state.RbNil;
        }

        [RbInstanceMethod("red")]
        public static RbValue GetR(RbState state, RbValue self)
        {
            var colorData = self.GetRDataObject<ColorData>();
            return (colorData.Color.r * 255.0f).ToValue(state);
        }

        [RbInstanceMethod("red=")]
        public static RbValue SetR(RbState state, RbValue self, RbValue r)
        {
            var rVal = r.IsInt ? r.ToIntUnchecked() : r.ToFloatUnchecked();
            var colorData = self.GetRDataObject<ColorData>();
            colorData.Color.r = (float)rVal / 255.0f;
            return state.RbNil;
        }

        [RbInstanceMethod("green")]
        public static RbValue GetG(RbState state, RbValue self)
        {
            var colorData = self.GetRDataObject<ColorData>();
            return (colorData.Color.g * 255.0f).ToValue(state);
        }

        [RbInstanceMethod("green=")]
        public static RbValue SetG(RbState state, RbValue self, RbValue g)
        {
            var gVal = g.IsInt ? g.ToIntUnchecked() : g.ToFloatUnchecked();
            var colorData = self.GetRDataObject<ColorData>();
            colorData.Color.g = (float)gVal / 255.0f;
            return state.RbNil;
        }

        [RbInstanceMethod("blue")]
        public static RbValue GetB(RbState state, RbValue self)
        {
            var colorData = self.GetRDataObject<ColorData>();
            return (colorData.Color.b * 255.0f).ToValue(state);
        }

        [RbInstanceMethod("blue=")]
        public static RbValue SetB(RbState state, RbValue self, RbValue b)
        {
            var bVal = b.IsInt ? b.ToIntUnchecked() : b.ToFloatUnchecked();
            var colorData = self.GetRDataObject<ColorData>();
            colorData.Color.b = (float)bVal * 255.0f;
            return state.RbNil;
        }

        [RbInstanceMethod("alpha")]
        public static RbValue GetA(RbState state, RbValue self)
        {
            var colorData = self.GetRDataObject<ColorData>();
            return (colorData.Color.a * 255.0f).ToValue(state);
        }

        [RbInstanceMethod("alpha=")]
        public static RbValue SetA(RbState state, RbValue self, RbValue a)
        {
            var aVal = a.IsInt ? a.ToIntUnchecked() : a.ToFloatUnchecked();
            var colorData = self.GetRDataObject<ColorData>();
            colorData.Color.a = (float)aVal * 255.0f;
            return state.RbNil;
        }
    }
}