using MRuby.Library.Language;
using MRuby.Library.Mapper;

namespace RGSSUnity.RubyClasses
{
    using UnityEngine;

    public class ToneData : RubyData
    {
        public Vector4 Tone;

        public ToneData(RbState state) : base(state)
        {
        }

        public float Red
        {
            get => Tone.x;
            set => Tone.x = value;
        }
        
        public float Green
        {
            get => Tone.y;
            set => Tone.y = value;
        }
        
        public float Blue
        {
            get => Tone.z;
            set => Tone.z = value;
        }
        
        public float Gray
        {
            get => Tone.w;
            set => Tone.w = value;
        }
    }

    [RbClass("Tone", "Object", "Unity")]
    public static class Tone
    {
        public static RbValue CreateTone(RbState state, float rVal, float gVal, float bVal, float grayVal)
        {
            var tone = new ToneData(state)
            {
                Tone = new Vector4(rVal / 255.0f, gVal / 255.0f, bVal / 255.0f, grayVal / 255.0f)
            };
            var cls = RubyScriptManager.Instance.GetClassUnderUnityModule("Tone");
            var res = cls.NewObjectWithRData(tone);
            return res;
        }

        [RbClassMethod("new_rgbg")]
        public static RbValue NewTone(RbState state, RbValue self, RbValue r, RbValue g, RbValue b, RbValue gray)
        {
            var rVal = r.IsInt ? r.ToIntUnchecked() : r.ToFloatUnchecked();
            var gVal = g.IsInt ? g.ToIntUnchecked() : g.ToFloatUnchecked();
            var bVal = b.IsInt ? b.ToIntUnchecked() : b.ToFloatUnchecked();
            var grayVal = gray.IsInt ? gray.ToIntUnchecked() : gray.ToFloatUnchecked();

            var res = CreateTone(state, (float)rVal, (float)gVal, (float)bVal, (float)grayVal);
            return res;
        }
        
        [RbInstanceMethod("set_rgbg")]
        public static RbValue SetRgbg(RbState state, RbValue self, RbValue r, RbValue g, RbValue b, RbValue gray)
        {
            var rVal = r.IsInt ? r.ToIntUnchecked() : r.ToFloatUnchecked();
            var gVal = g.IsInt ? g.ToIntUnchecked() : g.ToFloatUnchecked();
            var bVal = b.IsInt ? b.ToIntUnchecked() : b.ToFloatUnchecked();
            var grayVal = gray.IsInt ? gray.ToIntUnchecked() : gray.ToFloatUnchecked();

            var toneData = self.GetRDataObject<ToneData>();
            toneData.Red = (float)rVal / 255.0f;
            toneData.Green = (float)gVal / 255.0f;
            toneData.Blue = (float)bVal / 255.0f;
            toneData.Gray = (float)grayVal / 255.0f;
            return state.RbNil;
        }

        [RbInstanceMethod("red")]
        public static RbValue GetR(RbState state, RbValue self)
        {
            var toneData = self.GetRDataObject<ToneData>();
            return (toneData.Red * 255.0f).ToValue(state);
        }

        [RbInstanceMethod("red=")]
        public static RbValue SetR(RbState state, RbValue self, RbValue r)
        {
            var rVal = r.IsInt ? r.ToIntUnchecked() : r.ToFloatUnchecked();
            var toneData = self.GetRDataObject<ToneData>();
            toneData.Red = (float)rVal / 255.0f;
            return state.RbNil;
        }

        [RbInstanceMethod("green")]
        public static RbValue GetG(RbState state, RbValue self)
        {
            var toneData = self.GetRDataObject<ToneData>();
            return (toneData.Green * 255.0f).ToValue(state);
        }

        [RbInstanceMethod("green=")]
        public static RbValue SetG(RbState state, RbValue self, RbValue g)
        {
            var gVal = g.IsInt ? g.ToIntUnchecked() : g.ToFloatUnchecked();
            var toneData = self.GetRDataObject<ToneData>();
            toneData.Green = (float)gVal / 255.0f;
            return state.RbNil;
        }

        [RbInstanceMethod("blue")]
        public static RbValue GetB(RbState state, RbValue self)
        {
            var toneData = self.GetRDataObject<ToneData>();
            return (toneData.Blue * 255.0f).ToValue(state);
        }

        [RbInstanceMethod("blue=")]
        public static RbValue SetB(RbState state, RbValue self, RbValue b)
        {
            var bVal = b.IsInt ? b.ToIntUnchecked() : b.ToFloatUnchecked();
            var toneData = self.GetRDataObject<ToneData>();
            toneData.Blue = (float)bVal / 255.0f;
            return state.RbNil;
        }

        [RbInstanceMethod("gray")]
        public static RbValue GetGray(RbState state, RbValue self)
        {
            var toneData = self.GetRDataObject<ToneData>();
            return (toneData.Gray * 255.0f).ToValue(state);
        }

        [RbInstanceMethod("gray=")]
        public static RbValue SetGray(RbState state, RbValue self, RbValue gray)
        {
            var grayVal = gray.IsInt ? gray.ToIntUnchecked() : gray.ToFloatUnchecked();
            var toneData = self.GetRDataObject<ToneData>();
            toneData.Gray = (float)grayVal / 255.0f;
            return state.RbNil;
        }
    }
}