using MRuby.Library.Language;
using MRuby.Library.Mapper;
using RGSSUnity.Components;
using UnityEngine;

namespace RGSSUnity.RubyClasses
{
    public class ViewportData : RubyData
    {
        public ColorData ColorData;
        public ToneData ToneData;
        public SpriteRenderer ViewportSpriteRenderer;
        public RenderTexture RenderTexture;
        public GameObject ViewportObject;
        public long LogicX;
        public long LogicY;
        public long Z;
        public long Ox;
        public long Oy;

        public ColorData FlashColorData;
        public int FlashDuration;
        public int RemainFlashDuration;
        
        public ViewportData(RbState state) : base(state)
        {
        }

        ~ViewportData()
        {
            if (ViewportObject)
            {
                Object.Destroy(ViewportObject);
            }

            if (RenderTexture)
            {
                Object.Destroy(RenderTexture);
            }
        }
    }

    [RbClass("Viewport", "Object", "Unity")]
    public static class Viewport
    {
        private static readonly int FlashColor_ = Shader.PropertyToID("_FlashColor");
        private static readonly int FlashProgress_ = Shader.PropertyToID("_FlashProgress");
        private static readonly int MixColor_ = Shader.PropertyToID("_MixColor");
        private static readonly int Tone_ = Shader.PropertyToID("_Tone");

        [RbClassMethod("new_without_rect")]
        public static RbValue NewViewportWithoutRect(RbState state, RbValue self)
        {
            var vx = 0;
            var vy = 0;

            var vw = Screen.width;
            var vh = Screen.height;

            if (GlobalConfig.LegacyMode)
            {
                vw = GlobalConfig.LegacyModeWidth;
                vh = GlobalConfig.LegacyModeHeight;
            }

            return CreateViewport(state, self.ToClass(), vx, vy, vw, vh);
        }

        [RbClassMethod("new_xyrw")]
        public static RbValue NewViewportWithXyrw(RbState state, RbValue self, RbValue x, RbValue y, RbValue width, RbValue height)
        {
            var vx = x.ToIntUnchecked();
            var vy = y.ToIntUnchecked();
            var vw = width.ToIntUnchecked();
            var vh = height.ToIntUnchecked();

            return CreateViewport(state, self.ToClass(), vx, vy, vw, vh);
        }

        [RbInstanceMethod("dispose")]
        public static RbValue Dispose(RbState state, RbValue self)
        {
            var data = self.GetRDataObject<ViewportData>();
            Object.Destroy(data.ViewportObject);
            Object.Destroy(data.RenderTexture);

            data.RenderTexture = null;
            data.ViewportObject = null;

            return state.RbNil;
        }

        [RbInstanceMethod("disposed?")]
        public static RbValue Disposed(RbState state, RbValue self)
        {
            var data = self.GetRDataObject<ViewportData>();
            return !data.RenderTexture ? state.RbTrue : state.RbFalse;
        }

        [RbInstanceMethod("flash")]
        public static RbValue Flash(RbState state, RbValue self, RbValue color, RbValue duration)
        {
            var data = self.GetRDataObject<ViewportData>();
            var durationNum = duration.ToIntUnchecked();

            if (!color.IsNil)
            {
                var colorData = color.GetRDataObject<ColorData>();
                data.FlashColorData = colorData;
            }
            else
            {
                // hide sprite if no color
                data.ViewportObject.SetActive(false);
            }

            data.FlashDuration = (int)durationNum;
            data.RemainFlashDuration = (int)durationNum;

            return state.RbNil;
        }

        [RbInstanceMethod("update")]
        public static RbValue Update(RbState state, RbValue self)
        {
            var data = self.GetRDataObject<ViewportData>();

            if (data.RemainFlashDuration > 0)
            {
                --data.RemainFlashDuration;
            }
            else
            {
                data.ViewportObject.SetActive(true);
                data.FlashDuration = 0;
                data.RemainFlashDuration = 0;
                data.FlashColorData = null;
            }

            return state.RbNil;
        }

        [RbInstanceMethod("rect")]
        public static RbValue GetRect(RbState state, RbValue self)
        {
            var rect = self.GetRDataObject<ViewportData>().ViewportSpriteRenderer.sprite.rect;
            return Rect.CreateRect(
                state,
                (long)rect.x,
                (long)rect.y,
                (long)rect.width,
                (long)rect.height);
        }

        [RbInstanceMethod("rect=")]
        public static RbValue SetRect(RbState state, RbValue self, RbValue rect)
        {
            var data = self.GetRDataObject<ViewportData>();
            var rectData = rect.GetRDataObject<RectData>();
            data.ViewportSpriteRenderer.sprite.rect.Set(
                rectData.Rect.x, rectData.Rect.y, rectData.Rect.width, rectData.Rect.height);
            return rect;
        }

        [RbInstanceMethod("visible")]
        public static RbValue GetVisible(RbState state, RbValue self)
        {
            var data = self.GetRDataObject<ViewportData>();
            return data.ViewportSpriteRenderer.enabled.ToValue(state);
        }

        [RbInstanceMethod("visible=")]
        public static RbValue SetVisible(RbState state, RbValue self, RbValue visible)
        {
            var data = self.GetRDataObject<ViewportData>();
            data.ViewportSpriteRenderer.enabled = visible.IsTrue;
            return state.RbNil;
        }

        [RbInstanceMethod("color")]
        public static RbValue GetColor(RbState state, RbValue self)
        {
            var data = self.GetRDataObject<ViewportData>();
            var color = data.ColorData.Color;
            return Color.CreateColor(
                state,
                (int)(color.r * 255),
                (int)(color.g * 255),
                (int)(color.b * 255),
                (int)(color.a * 255)
            );
        }

        [RbInstanceMethod("color=")]
        public static RbValue SetColor(RbState state, RbValue self, RbValue color)
        {
            var data = self.GetRDataObject<ViewportData>();
            var colorData = color.GetRDataObject<ColorData>();
            data.ColorData.Color = new UnityEngine.Color(
                colorData.Color.r / 255.0f,
                colorData.Color.g / 255.0f,
                colorData.Color.b / 255.0f,
                colorData.Color.a / 255.0f
            );
            return state.RbNil;
        }

        [RbInstanceMethod("tone")]
        public static RbValue GetTone(RbState state, RbValue self)
        {
            var data = self.GetRDataObject<ViewportData>();
            var tone = data.ToneData.Tone;
            return Tone.CreateTone(
                state,
                (int)(tone.x * 255),
                (int)(tone.y * 255),
                (int)(tone.z * 255),
                (int)(tone.w * 255)
            );
        }

        [RbInstanceMethod("tone=")]
        public static RbValue SetTone(RbState state, RbValue self, RbValue tone)
        {
            var data = self.GetRDataObject<ViewportData>();
            var toneData = tone.GetRDataObject<ToneData>();
            data.ToneData.Tone = new Vector4(
                toneData.Tone.x / 255.0f,
                toneData.Tone.y / 255.0f,
                toneData.Tone.z / 255.0f,
                toneData.Tone.w / 255.0f
            );
            return state.RbNil;
        }

        [RbInstanceMethod("ox")]
        public static RbValue GetOx(RbState state, RbValue self)
        {
            var data = self.GetRDataObject<ViewportData>();
            return data.Ox.ToValue(state);
        }

        [RbInstanceMethod("ox=")]
        public static RbValue SetOx(RbState state, RbValue self, RbValue ox)
        {
            var data = self.GetRDataObject<ViewportData>();
            data.Ox = ox.ToIntUnchecked();
            return state.RbNil;
        }

        [RbInstanceMethod("oy")]
        public static RbValue GetOy(RbState state, RbValue self)
        {
            var data = self.GetRDataObject<ViewportData>();
            return data.Oy.ToValue(state);
        }

        [RbInstanceMethod("oy=")]
        public static RbValue SetOy(RbState state, RbValue self, RbValue oy)
        {
            var data = self.GetRDataObject<ViewportData>();
            data.Oy = oy.ToIntUnchecked();
            return state.RbNil;
        }

        [RbInstanceMethod("z")]
        public static RbValue GetZ(RbState state, RbValue self)
        {
            var data = self.GetRDataObject<ViewportData>();
            return data.Z.ToValue(state);
        }

        [RbInstanceMethod("z=")]
        public static RbValue SetZ(RbState state, RbValue self, RbValue z)
        {
            var data = self.GetRDataObject<ViewportData>();
            var zVal = z.ToIntUnchecked();
            data.Z = zVal;
            return state.RbNil;
        }

        internal static void Render(ViewportData data)
        {
            var renderer = data.ViewportSpriteRenderer;
            var material = renderer.material;

            if (!renderer.sprite)
            {
                return;
            }

            renderer.sortingOrder = (int)data.Z;

            if (data.RemainFlashDuration > 0 && data.FlashColorData == null)
            {
                return;
            }

            if (data.RemainFlashDuration > 0 && data.FlashColorData != null)
            {
                material.SetColor(FlashColor_, data.FlashColorData.Color);
                material.SetFloat(FlashProgress_, (float)data.RemainFlashDuration / data.FlashDuration);   
            }
            else
            {
                material.SetFloat(FlashProgress_, 0.0f);
            }

            if (data.ColorData != null)
            {
                material.SetVector(MixColor_, data.ColorData.Color);
            }

            if (data.ToneData != null)
            {
                material.SetVector(Tone_, data.ToneData.Tone);
            }
        }
        
        private static RbValue CreateViewport(RbState state, RbClass viewportCls, long vx, long vy, long vw, long vh)
        {
            var rect = new UnityEngine.Rect(0, 0, vw, vh);
            var color = UnityEngine.Color.clear;
            var tone = new Vector4(0, 0, 0, 0);
            var renderTexture = new RenderTexture((int)vw, (int)vh, 32, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            var texture2d = new Texture2D((int)vw, (int)vh, TextureFormat.ARGB32, false, false);

            // set texture2d to transparent color
            var colors = new UnityEngine.Color[(int)(vw * vh)];
            for (var i = 0; i < colors.Length; i++)
            {
                colors[i] = new UnityEngine.Color(0, 0, 0, 0);
            }
            texture2d.SetPixels(colors);
            texture2d.Apply();

            var sprite = UnityEngine.Sprite.Create(texture2d, rect, new Vector2(0, 1), 1.0f);

            var viewportObject = new GameObject("viewport");
            viewportObject.transform.SetParent(GameObject.Find("Viewports").transform);
            viewportObject.transform.SetSiblingIndex(0);

            var renderer = viewportObject.AddComponent<SpriteRenderer>();
            var viewportDataComp = viewportObject.AddComponent<ViewportDataComponent>();

            renderer.enabled = false;

            viewportObject.transform.position = new Vector3(0, 0, 1.0f);

            renderer.material = new Material(Shader.Find("Custom/ViewportShader"));
            renderer.sprite = sprite;

            // var rectTransform = viewportObject.AddComponent<RectTransform>();
            // rectTransform.pivot = new Vector2(0.0f, 1.0f);
            // rectTransform.anchorMin = new Vector2(0.0f, 0.0f);
            // rectTransform.anchorMax = new Vector2(1.0f, 1.0f);

            var data = new ViewportData(state)
            {
                ToneData = new ToneData(state)
                {
                    Tone = tone,
                },
                ColorData = new ColorData(state)
                {
                    Color = color,
                },
                ViewportSpriteRenderer = renderer,
                RenderTexture = renderTexture,
                ViewportObject = viewportObject,
                LogicX = vx,
                LogicY = vy,
                Z = 1,
            };

            viewportDataComp.ViewportData = data;

            var cls = viewportCls;
            var res = cls.NewObjectWithRData(data);
            return res;
        }
    }
}