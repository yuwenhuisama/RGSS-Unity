using MRuby.Library.Mapper;
using MRuby.Library.Language;
using UnityEngine;

namespace RGSSUnity.RubyClasses
{
    using Components;

    public class PlaneData : RubyData
    {
        public GameObject PlaneObject;
        public ToneData ToneData;
        public ColorData ColorData;
        public BitmapData BitmapData;
        public SpriteRenderer SpriteRenderer;
        public long ViewportWidth;
        public long ViewportHeight;

        public int Z;
        public int Ox;
        public int Oy;
        public int BlendType = 0;

        public PlaneData(RbState state) : base(state)
        {
        }

        ~PlaneData()
        {
            if (this.PlaneObject)
            {
                Object.Destroy(this.PlaneObject);
            }
        }
    }

    [RbClass("Plane", "Object", "Unity")]
    public static class Plane
    {
        private static readonly Shader PlaneShader_ = Shader.Find("Custom/PlaneShader");
        private static readonly int SrcBlend_ = Shader.PropertyToID("_SrcBlend");
        private static readonly int DstBlend_ = Shader.PropertyToID("_DstBlend");
        private static readonly int OxOy_ = Shader.PropertyToID("_OxOy");
        private static readonly int MixColor_ = Shader.PropertyToID("_MixColor");
        private static readonly int Tone_ = Shader.PropertyToID("_Tone");
        private static readonly int Scale_ = Shader.PropertyToID("_Scale");

        [RbClassMethod("new_with_viewport")]
        public static RbValue NewWithViewport(RbState state, RbValue self, RbValue viewport)
        {
            var data = new PlaneData(state)
            {
                PlaneObject = new GameObject("plane"),
                Z = 1,
                ToneData = new ToneData(state)
                {
                    Red = 0,
                    Green = 0,
                    Blue = 0,
                    Gray = 0,
                },
                ColorData = new ColorData(state)
                {
                    Color = UnityEngine.Color.clear
                },
            };

            var planeObject = data.PlaneObject;
            planeObject.tag = "RGSSPlane";

            var viewportData = viewport.GetRDataObject<ViewportData>();
            planeObject.transform.SetParent(viewportData.ViewportObject.transform);
            data.ViewportWidth = viewportData.RenderTexture.width;
            data.ViewportHeight = viewportData.RenderTexture.height;

            var renderer = planeObject.AddComponent<SpriteRenderer>();
            renderer.material = new Material(PlaneShader_);
            renderer.enabled = false;
            data.SpriteRenderer = renderer;

            var dataComp = planeObject.AddComponent<PlaneDataComponent>();
            dataComp.PlaneData = data;

            var cls = self.ToClass();
            var obj = cls.NewObjectWithRData(data);

            obj.SetInstanceVariable("@viewport", viewport);
            obj.SetInstanceVariable("@bitmap", state.RbNil);
            return obj;
        }

        [RbInstanceMethod("viewport")]
        public static RbValue Viewport(RbState state, RbValue self)
        {
            return self.GetInstanceVariable("@viewport");
        }

        [RbInstanceMethod("viewport=")]
        public static RbValue SetViewport(RbState state, RbValue self, RbValue viewport)
        {
            var data = self.GetRDataObject<PlaneData>();
            var viewportData = viewport.GetRDataObject<ViewportData>();

            data.PlaneObject.transform.SetParent(viewportData.ViewportObject.transform);

            self.SetInstanceVariable("@viewport", viewport);
            return viewport;
        }

        [RbInstanceMethod("bitmap")]
        public static RbValue Bitmap(RbState state, RbValue self)
        {
            return self.GetInstanceVariable("@bitmap");
        }

        [RbInstanceMethod("bitmap=")]
        public static RbValue BitmapSet(RbState state, RbValue self, RbValue bitmap)
        {
            var data = self.GetRDataObject<PlaneData>();

            if (!bitmap.IsNil)
            {
                var bitmapData = bitmap.GetRDataObject<BitmapData>();

                var spriteObj = data.PlaneObject;
                var renderer = data.SpriteRenderer;
                data.BitmapData = bitmapData;

                var tex2dRepeat = new Texture2D(
                    bitmapData.Texture2D.width,
                    bitmapData.Texture2D.height,
                    TextureFormat.ARGB32,
                    false,
                    false);
                Sprite.TextureCopy(
                    bitmapData.Texture2D,
                    tex2dRepeat,
                    new UnityEngine.Rect(0, 0, bitmapData.Texture2D.width, bitmapData.Texture2D.height),
                    0,
                    0);
                Sprite.SetSpriteToSpriteRenderer(renderer, tex2dRepeat);

                spriteObj.transform.position = new Vector3(0, 0, 1.0f);
            }
            else
            {
                var sprite = data.SpriteRenderer.sprite;
                data.BitmapData = null;

                Object.Destroy(sprite);
                data.SpriteRenderer.sprite = null;
            }

            self.SetInstanceVariable("@bitmap", bitmap);
            return state.RbNil;
        }

        [RbInstanceMethod("tone")]
        public static RbValue GetTone(RbState state, RbValue self)
        {
            var data = self.GetRDataObject<PlaneData>();
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
            var data = self.GetRDataObject<PlaneData>();
            var toneData = tone.GetRDataObject<ToneData>();
            data.ToneData.Tone = new Vector4(
                toneData.Tone.x / 255.0f,
                toneData.Tone.y / 255.0f,
                toneData.Tone.z / 255.0f,
                toneData.Tone.w / 255.0f
            );
            return state.RbNil;
        }

        [RbInstanceMethod("color")]
        public static RbValue GetColor(RbState state, RbValue self)
        {
            var data = self.GetRDataObject<PlaneData>();
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
            var data = self.GetRDataObject<PlaneData>();
            var colorData = color.GetRDataObject<ColorData>();
            data.ColorData.Color = new UnityEngine.Color(
                colorData.Color.r / 255.0f,
                colorData.Color.g / 255.0f,
                colorData.Color.b / 255.0f,
                colorData.Color.a / 255.0f
            );
            return state.RbNil;
        }

        [RbInstanceMethod("visible")]
        public static RbValue GetVisible(RbState state, RbValue self)
        {
            var data = self.GetRDataObject<PlaneData>();
            return data.PlaneObject.activeSelf.ToValue(state);
        }

        [RbInstanceMethod("visible=")]
        public static RbValue SetVisible(RbState state, RbValue self, RbValue visible)
        {
            var data = self.GetRDataObject<PlaneData>();
            data.PlaneObject.SetActive(visible.IsTrue);
            return state.RbNil;
        }

        [RbInstanceMethod("z")]
        public static RbValue GetZ(RbState state, RbValue self)
        {
            var data = self.GetRDataObject<PlaneData>();
            return data.Z.ToValue(state);
        }

        [RbInstanceMethod("z=")]
        public static RbValue SetZ(RbState state, RbValue self, RbValue z)
        {
            var data = self.GetRDataObject<PlaneData>();
            data.Z = (int)z.ToIntUnchecked();
            return state.RbNil;
        }

        [RbInstanceMethod("ox")]
        public static RbValue GetOx(RbState state, RbValue self)
        {
            var data = self.GetRDataObject<PlaneData>();
            return data.Ox.ToValue(state);
        }

        [RbInstanceMethod("ox=")]
        public static RbValue SetOx(RbState state, RbValue self, RbValue ox)
        {
            var data = self.GetRDataObject<PlaneData>();
            data.Ox = (int)ox.ToIntUnchecked();
            return state.RbNil;
        }

        [RbInstanceMethod("oy")]
        public static RbValue GetOy(RbState state, RbValue self)
        {
            var data = self.GetRDataObject<PlaneData>();
            return data.Oy.ToValue(state);
        }

        [RbInstanceMethod("oy=")]
        public static RbValue SetOy(RbState state, RbValue self, RbValue oy)
        {
            var data = self.GetRDataObject<PlaneData>();
            data.Oy = (int)oy.ToIntUnchecked();
            return state.RbNil;
        }

        [RbInstanceMethod("zoom_x")]
        public static RbValue GetZoomX(RbState state, RbValue self)
        {
            var data = self.GetRDataObject<PlaneData>();
            return data.PlaneObject.transform.localScale.x.ToValue(state);
        }

        [RbInstanceMethod("zoom_x=")]
        public static RbValue SetZoomX(RbState state, RbValue self, RbValue zoomX)
        {
            var data = self.GetRDataObject<PlaneData>();
            var zoom = zoomX.IsFloat ? (float)zoomX.ToFloatUnchecked() : (int)zoomX.ToIntUnchecked();

            var scale = data.PlaneObject.transform.localScale;
            scale.x = zoom;
            data.PlaneObject.transform.localScale = scale;
            return state.RbNil;
        }

        [RbInstanceMethod("zoom_y")]
        public static RbValue GetZoomY(RbState state, RbValue self)
        {
            var data = self.GetRDataObject<PlaneData>();
            return data.PlaneObject.transform.localScale.y.ToValue(state);
        }

        [RbInstanceMethod("zoom_y=")]
        public static RbValue SetZoomY(RbState state, RbValue self, RbValue zoomY)
        {
            var data = self.GetRDataObject<PlaneData>();
            var zoom = zoomY.IsFloat ? (float)zoomY.ToFloatUnchecked() : (int)zoomY.ToIntUnchecked();

            var scale = data.PlaneObject.transform.localScale;
            scale.y = zoom;
            data.PlaneObject.transform.localScale = scale;
            return state.RbNil;
        }

        [RbInstanceMethod("dispose")]
        public static RbValue Dispose(RbState state, RbValue self)
        {
            var data = self.GetRDataObject<PlaneData>();
            Object.Destroy(data.PlaneObject);
            data.PlaneObject = null;
            return state.RbNil;
        }

        [RbInstanceMethod("disposed?")]
        public static RbValue Disposed(RbState state, RbValue self)
        {
            var data = self.GetRDataObject<SpriteData>();
            return !data.SpriteObject ? state.RbTrue : state.RbFalse;
        }

        [RbInstanceMethod("blend_type")]
        public static RbValue GetBlendType(RbState state, RbValue self)
        {
            var data = self.GetRDataObject<PlaneData>();
            return data.BlendType.ToValue(state);
        }

        [RbInstanceMethod("blend_type=")]
        public static RbValue SetBlendType(RbState state, RbValue self, RbValue blendType)
        {
            var data = self.GetRDataObject<PlaneData>();
            data.BlendType = (int)blendType.ToIntUnchecked();
            return state.RbNil;
        }

        public static void Render(PlaneData data)
        {
            SetShaderProperties(data);
        }

        private static void SetShaderProperties(PlaneData data)
        {
            // todo: cache the sprite data and check if need to update the shader properties
            var renderer = data.SpriteRenderer;
            var width = data.ViewportWidth;
            var height = data.ViewportHeight;
            var material = renderer.material;

            var bitmapWidth = renderer.sprite.texture.width;
            var bitmapHeight = renderer.sprite.texture.height;

            if (!renderer.sprite)
            {
                return;
            }

            switch (data.BlendType)
            {
                case 0:
                    // normal mode
                    material.SetFloat(SrcBlend_, (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    material.SetFloat(DstBlend_, (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    break;
                case 1:
                    // additive mode
                    material.SetFloat(SrcBlend_, (float)UnityEngine.Rendering.BlendMode.One);
                    material.SetFloat(DstBlend_, (float)UnityEngine.Rendering.BlendMode.One);
                    break;
                case 2:
                    // subtractive mode
                    material.SetFloat(SrcBlend_, (float)UnityEngine.Rendering.BlendMode.OneMinusDstColor);
                    material.SetFloat(DstBlend_, (float)UnityEngine.Rendering.BlendMode.Zero);
                    break;
            }

            material.SetVector(OxOy_, new Vector2(data.Ox / (float)width, data.Oy / (float)height));
            material.SetVector(Scale_, new Vector2((float)width / bitmapWidth, (float)height / bitmapHeight));

            if (data.ColorData != null)
            {
                material.SetVector(MixColor_, data.ColorData.Color);
            }

            if (data.ToneData != null)
            {
                material.SetVector(Tone_, data.ToneData.Tone);
            }
        }
    }
}