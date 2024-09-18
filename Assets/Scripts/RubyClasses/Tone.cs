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
        public static RbValue CreateTone(RbState state, long rVal, long gVal, long bVal, long grayVal)
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
            var rVal = r.ToIntUnchecked();
            var gVal = g.ToIntUnchecked();
            var bVal = b.ToIntUnchecked();
            var grayVal = gray.ToIntUnchecked();

            var res = CreateTone(state, rVal, gVal, bVal, grayVal);
            return res;
        }

        [RbInstanceMethod("red")]
        public static RbValue GetR(RbState state, RbValue self)
        {
            var toneData = self.GetRDataObject<ToneData>();
            return ((int)(toneData.Red * 255.0f)).ToValue(state);
        }

        [RbInstanceMethod("red=")]
        public static RbValue SetR(RbState state, RbValue self, RbValue r)
        {
            var rVal = r.ToIntUnchecked();
            var toneData = self.GetRDataObject<ToneData>();
            toneData.Red = rVal / 255.0f;
            return state.RbNil;
        }

        [RbInstanceMethod("green")]
        public static RbValue GetG(RbState state, RbValue self)
        {
            var toneData = self.GetRDataObject<ToneData>();
            return ((int)(toneData.Green * 255.0f)).ToValue(state);
        }

        [RbInstanceMethod("green=")]
        public static RbValue SetG(RbState state, RbValue self, RbValue g)
        {
            var gVal = g.ToIntUnchecked();
            var toneData = self.GetRDataObject<ToneData>();
            toneData.Green = gVal / 255.0f;
            return state.RbNil;
        }

        [RbInstanceMethod("blue")]
        public static RbValue GetB(RbState state, RbValue self)
        {
            var toneData = self.GetRDataObject<ToneData>();
            return ((int)(toneData.Blue * 255.0f)).ToValue(state);
        }

        [RbInstanceMethod("blue=")]
        public static RbValue SetB(RbState state, RbValue self, RbValue b)
        {
            var bVal = state.UnboxInt(b);
            var toneData = self.GetRDataObject<ToneData>();
            toneData.Blue = bVal / 255.0f;
            return state.RbNil;
        }

        [RbInstanceMethod("gray")]
        public static RbValue GetGray(RbState state, RbValue self)
        {
            var toneData = self.GetRDataObject<ToneData>();
            return ((int)(toneData.Gray * 255.0f)).ToValue(state);
        }

        [RbInstanceMethod("gray=")]
        public static RbValue SetGray(RbState state, RbValue self, RbValue gray)
        {
            var grayVal = gray.ToIntUnchecked();
            var toneData = self.GetRDataObject<ToneData>();
            toneData.Gray = grayVal / 255.0f;
            return state.RbNil;
        }
    }
}