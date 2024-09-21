using MRuby.Library.Language;
using MRuby.Library.Mapper;
using RGSSUnity.Components;
using UnityEngine;
using SpriteRenderer = UnityEngine.SpriteRenderer;

namespace RGSSUnity.RubyClasses
{
    using System;
    using Object = UnityEngine.Object;

    public class SpriteData : RubyData
    {
        public GameObject SpriteObject;
        public ToneData ToneData;
        public ColorData ColorData;
        public BitmapData BitmapData;
        public SpriteRenderer SpriteRenderer;
        public ColorData FlashColorData;
        public int FlashDuration;
        public int RemainFlashDuration;

        public int X;
        public int Y;
        public int Z;
        public int Ox;
        public int Oy;
        public int Angle;
        public int RotatedAngle;
        public UnityEngine.Rect SrcRect = new(0, 0, 0, 0);
        public bool Mirror;

        public int WaveAmp = 0;
        public int WaveLength = 72;
        public int WaveSpeed = 720;
        public float WavePhase = 0;

        public int BushDepth = 0;
        public int BushOpacity = 255;
        public int Opacity = 255;
        public int BlendType = 0;
        public bool Visible;

        public SpriteData(RbState state) : base(state)
        {
        }

        ~SpriteData()
        {
            if (SpriteObject)
            {
                Object.Destroy(SpriteObject);
            }
        }
    }

    [RbClass("Sprite", "Object", "Unity")]
    public static class Sprite
    {
        private static readonly Shader SpriteShader = Shader.Find("Custom/SpriteShader");
        private static readonly int Opacity_ = Shader.PropertyToID("_Opacity");
        private static readonly int SrcBlend_ = Shader.PropertyToID("_SrcBlend");
        private static readonly int DstBlend_ = Shader.PropertyToID("_DstBlend");
        private static readonly int WaveAmp_ = Shader.PropertyToID("_WaveAmp");
        private static readonly int WaveLength_ = Shader.PropertyToID("_WaveLength");
        private static readonly int WavePhase_ = Shader.PropertyToID("_WavePhase");
        private static readonly int BushOpacity_ = Shader.PropertyToID("_BushOpacity");
        private static readonly int BushDepth_ = Shader.PropertyToID("_BushDepth");
        private static readonly int WaveTexHeight_ = Shader.PropertyToID("_WaveTexHeight");
        private static readonly int WaveTexWidth_ = Shader.PropertyToID("_WaveTexWidth");
        private static readonly int Color_ = Shader.PropertyToID("_MixColor");
        private static readonly int Tone_ = Shader.PropertyToID("_Tone");
        private static readonly int Mirror_ = Shader.PropertyToID("_Mirror");
        private static readonly int FlashColor_ = Shader.PropertyToID("_FlashColor");
        private static readonly int FlashProgress_ = Shader.PropertyToID("_FlashProgress");

        [RbClassMethod("new_with_viewport")]
        public static RbValue NewWithViewport(RbState state, RbValue self, RbValue viewport)
        {
            var data = new SpriteData(state)
            {
                SpriteObject = new GameObject("sprite"),
                Z = 1,
                Visible = true,
            };

            data.ToneData = new ToneData(state)
            {
                Red = 0,
                Green = 0,
                Blue = 0,
                Gray = 0,
            };

            data.ColorData = new ColorData(state)
            {
                Color = UnityEngine.Color.clear
            };

            var spriteObject = data.SpriteObject;
            spriteObject.tag = "RGSSSprite";

            var viewportData = viewport.GetRDataObject<ViewportData>();
            spriteObject.transform.SetParent(viewportData.ViewportObject.transform);

            var renderer = spriteObject.AddComponent<SpriteRenderer>();
            renderer.material = new Material(SpriteShader);
            data.SpriteRenderer = renderer;
            
            spriteObject.SetActive(false);

            var dataComp = spriteObject.AddComponent<SpriteDataComponent>();
            dataComp.SpriteData = data;

            var cls = self.ToClass();
            var obj = cls.NewObjectWithRData(data);

            obj.SetInstanceVariable("@viewport", viewport);
            obj.SetInstanceVariable("@bitmap", state.RbNil);
            return obj;
        }

        [RbInstanceMethod("flash")]
        public static RbValue Flash(RbState state, RbValue self, RbValue color, RbValue duration)
        {
            var data = self.GetRDataObject<SpriteData>();
            var durationNum = duration.ToIntUnchecked();

            if (!color.IsNil)
            {
                var colorData = color.GetRDataObject<ColorData>();
                data.FlashColorData = colorData;
            }
            else
            {
                // hide sprite if no color
                data.SpriteObject.SetActive(false);
            }

            data.FlashDuration = (int)durationNum;
            data.RemainFlashDuration = (int)durationNum;

            return state.RbNil;
        }

        [RbInstanceMethod("update")]
        public static RbValue Update(RbState state, RbValue self)
        {
            var data = self.GetRDataObject<SpriteData>();

            if (data.RemainFlashDuration > 0)
            {
                --data.RemainFlashDuration;
            }
            else
            {
                data.SpriteObject.SetActive(true);
                data.FlashDuration = 0;
                data.RemainFlashDuration = 0;
                data.FlashColorData = null;
            }

            if (data.WaveAmp > 0)
            {
                var phase = data.WavePhase + (float)data.WaveSpeed / data.WaveLength;
                if (phase > 360)
                {
                    phase -= 360;
                }
                else if (phase < 360)
                {
                    phase += 360;
                }
                data.WavePhase = phase;
            }

            return state.RbNil;
        }

        [RbInstanceMethod("dispose")]
        public static RbValue Dispose(RbState state, RbValue self)
        {
            var data = self.GetRDataObject<SpriteData>();
            Object.Destroy(data.SpriteObject);
            data.SpriteObject = null;
            return state.RbNil;
        }

        [RbInstanceMethod("disposed?")]
        public static RbValue Disposed(RbState state, RbValue self)
        {
            var data = self.GetRDataObject<SpriteData>();
            return !data.SpriteObject ? state.RbTrue : state.RbFalse;
        }

        [RbInstanceMethod("width")]
        public static RbValue Width(RbState state, RbValue self)
        {
            var data = self.GetRDataObject<SpriteData>();
            return data.SrcRect.width.ToValue(state);
        }

        [RbInstanceMethod("height")]
        public static RbValue Height(RbState state, RbValue self)
        {
            var data = self.GetRDataObject<SpriteData>();
            return data.SrcRect.height.ToValue(state);
        }

        [RbInstanceMethod("bitmap")]
        public static RbValue Bitmap(RbState state, RbValue self)
        {
            return self.GetInstanceVariable("@bitmap");
        }

        [RbInstanceMethod("bitmap=")]
        public static RbValue BitmapSet(RbState state, RbValue self, RbValue bitmap)
        {
            var data = self.GetRDataObject<SpriteData>();

            if (!bitmap.IsNil)
            {
                var bitmapData = bitmap.GetRDataObject<BitmapData>();

                var texture = bitmapData.Texture2D;
                var spriteObj = data.SpriteObject;
                var renderer = data.SpriteRenderer;
                data.SrcRect = new UnityEngine.Rect(0, 0, texture.width, texture.height);
                data.BitmapData = bitmapData;

                CreateTextureToSpriteRenderer(texture, data, renderer);
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

        [RbInstanceMethod("src_rect")]
        public static RbValue SrcRect(RbState state, RbValue self)
        {
            var data = self.GetRDataObject<SpriteData>();

            return Rect.CreateRect(
                state,
                (long)data.SrcRect.x,
                (long)data.SrcRect.y,
                (long)data.SrcRect.width,
                (long)data.SrcRect.height);
        }

        [RbInstanceMethod("src_rect=")]
        public static RbValue SetSrcRect(RbState state, RbValue self, RbValue rect)
        {
            var data = self.GetRDataObject<SpriteData>();
            var rectData = rect.GetRDataObject<RectData>();
            data.SrcRect = rectData.Rect;

            var spriteRenderer = data.SpriteRenderer;
            var bitmap = data.BitmapData;

            if (bitmap == null)
            {
                return state.RbNil;
            }

            var bitmapTex = bitmap.Texture2D;
            CreateTextureToSpriteRenderer(bitmapTex, data, spriteRenderer);
            return state.RbNil;
        }

        [RbInstanceMethod("viewport")]
        public static RbValue Viewport(RbState state, RbValue self)
        {
            return self.GetInstanceVariable("@viewport");
        }

        [RbInstanceMethod("viewport=")]
        public static RbValue SetViewport(RbState state, RbValue self, RbValue viewport)
        {
            var data = self.GetRDataObject<SpriteData>();
            var viewportData = viewport.GetRDataObject<ViewportData>();

            data.SpriteObject.transform.SetParent(viewportData.ViewportObject.transform);

            self.SetInstanceVariable("@viewport", viewport);
            return viewport;
        }

        [RbInstanceMethod("tone")]
        public static RbValue GetTone(RbState state, RbValue self)
        {
            var data = self.GetRDataObject<SpriteData>();
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
            var data = self.GetRDataObject<SpriteData>();
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
            var data = self.GetRDataObject<SpriteData>();
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
            var data = self.GetRDataObject<SpriteData>();
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
            var data = self.GetRDataObject<SpriteData>();
            return data.SpriteObject.activeSelf.ToValue(state);
        }

        [RbInstanceMethod("visible=")]
        public static RbValue SetVisible(RbState state, RbValue self, RbValue visible)
        {
            var data = self.GetRDataObject<SpriteData>();
            data.SpriteObject.SetActive(visible.IsTrue);
            return state.RbNil;
        }

        [RbInstanceMethod("x")]
        public static RbValue GetX(RbState state, RbValue self)
        {
            var data = self.GetRDataObject<SpriteData>();
            return data.X.ToValue(state);
        }

        [RbInstanceMethod("x=")]
        public static RbValue SetX(RbState state, RbValue self, RbValue x)
        {
            var data = self.GetRDataObject<SpriteData>();
            data.X = (int)x.ToIntUnchecked();
            return state.RbNil;
        }

        [RbInstanceMethod("y")]
        public static RbValue GetY(RbState state, RbValue self)
        {
            var data = self.GetRDataObject<SpriteData>();
            return data.Y.ToValue(state);
        }

        [RbInstanceMethod("y=")]
        public static RbValue SetY(RbState state, RbValue self, RbValue y)
        {
            var data = self.GetRDataObject<SpriteData>();
            data.Y = (int)y.ToIntUnchecked();
            return state.RbNil;
        }

        [RbInstanceMethod("z")]
        public static RbValue GetZ(RbState state, RbValue self)
        {
            var data = self.GetRDataObject<SpriteData>();
            return data.Z.ToValue(state);
        }

        [RbInstanceMethod("z=")]
        public static RbValue SetZ(RbState state, RbValue self, RbValue z)
        {
            var data = self.GetRDataObject<SpriteData>();
            data.Z = (int)z.ToIntUnchecked();
            return state.RbNil;
        }

        [RbInstanceMethod("ox")]
        public static RbValue GetOx(RbState state, RbValue self)
        {
            var data = self.GetRDataObject<SpriteData>();
            return data.Ox.ToValue(state);
        }

        [RbInstanceMethod("ox=")]
        public static RbValue SetOx(RbState state, RbValue self, RbValue ox)
        {
            var data = self.GetRDataObject<SpriteData>();
            data.Ox = (int)ox.ToIntUnchecked();
            return state.RbNil;
        }

        [RbInstanceMethod("oy")]
        public static RbValue GetOy(RbState state, RbValue self)
        {
            var data = self.GetRDataObject<SpriteData>();
            return data.Oy.ToValue(state);
        }

        [RbInstanceMethod("oy=")]
        public static RbValue SetOy(RbState state, RbValue self, RbValue oy)
        {
            var data = self.GetRDataObject<SpriteData>();
            data.Oy = (int)oy.ToIntUnchecked();
            return state.RbNil;
        }

        [RbInstanceMethod("zoom_x")]
        public static RbValue GetZoomX(RbState state, RbValue self)
        {
            var data = self.GetRDataObject<SpriteData>();
            return data.SpriteObject.transform.localScale.x.ToValue(state);
        }

        [RbInstanceMethod("zoom_x=")]
        public static RbValue SetZoomX(RbState state, RbValue self, RbValue zoomX)
        {
            var data = self.GetRDataObject<SpriteData>();
            var zoom = zoomX.IsFloat ? (float)zoomX.ToFloatUnchecked() : (int)zoomX.ToIntUnchecked();

            var scale = data.SpriteObject.transform.localScale;
            scale.x = zoom;
            data.SpriteObject.transform.localScale = scale;
            return state.RbNil;
        }

        [RbInstanceMethod("zoom_y")]
        public static RbValue GetZoomY(RbState state, RbValue self)
        {
            var data = self.GetRDataObject<SpriteData>();
            return data.SpriteObject.transform.localScale.y.ToValue(state);
        }

        [RbInstanceMethod("zoom_y=")]
        public static RbValue SetZoomY(RbState state, RbValue self, RbValue zoomY)
        {
            var data = self.GetRDataObject<SpriteData>();
            var zoom = zoomY.IsFloat ? (float)zoomY.ToFloatUnchecked() : (int)zoomY.ToIntUnchecked();

            var scale = data.SpriteObject.transform.localScale;
            scale.y = zoom;
            data.SpriteObject.transform.localScale = scale;
            return state.RbNil;
        }

        [RbInstanceMethod("angle")]
        public static RbValue GetAngle(RbState state, RbValue self)
        {
            var data = self.GetRDataObject<SpriteData>();
            return data.Angle.ToValue(state);
        }

        [RbInstanceMethod("angle=")]
        public static RbValue SetAngle(RbState state, RbValue self, RbValue angle)
        {
            var data = self.GetRDataObject<SpriteData>();
            var newAngle = (int)angle.ToInt();
            if (data.Angle != newAngle)
            {
                data.Angle = newAngle;
                data.RotatedAngle = newAngle;
            }
            return state.RbNil;
        }

        [RbInstanceMethod("wave_amp")]
        public static RbValue GetWaveAmp(RbState state, RbValue self)
        {
            var data = self.GetRDataObject<SpriteData>();
            return data.WaveAmp.ToValue(state);
        }

        [RbInstanceMethod("wave_amp=")]
        public static RbValue SetWaveAmp(RbState state, RbValue self, RbValue waveAmp)
        {
            var data = self.GetRDataObject<SpriteData>();
            data.WaveAmp = (int)waveAmp.ToIntUnchecked();

            var spriteRenderer = data.SpriteRenderer;

            var bitmap = data.BitmapData;
            if (bitmap == null)
            {
                return state.RbNil;
            }
            var bitmapTex = bitmap.Texture2D;
            CreateTextureToSpriteRenderer(bitmapTex, data, spriteRenderer);

            return state.RbNil;
        }

        [RbInstanceMethod("wave_length")]
        public static RbValue GetWaveLength(RbState state, RbValue self)
        {
            var data = self.GetRDataObject<SpriteData>();
            return data.WaveLength.ToValue(state);
        }

        [RbInstanceMethod("wave_length=")]
        public static RbValue SetWaveLength(RbState state, RbValue self, RbValue waveLength)
        {
            var data = self.GetRDataObject<SpriteData>();
            data.WaveLength = (int)waveLength.ToIntUnchecked();
            return state.RbNil;
        }

        [RbInstanceMethod("wave_speed")]
        public static RbValue GetWaveSpeed(RbState state, RbValue self)
        {
            var data = self.GetRDataObject<SpriteData>();
            return data.WaveSpeed.ToValue(state);
        }

        [RbInstanceMethod("wave_speed=")]
        public static RbValue SetWaveSpeed(RbState state, RbValue self, RbValue waveSpeed)
        {
            var data = self.GetRDataObject<SpriteData>();
            data.WaveSpeed = (int)waveSpeed.ToIntUnchecked();
            return state.RbNil;
        }

        [RbInstanceMethod("wave_phase")]
        public static RbValue GetWavePhase(RbState state, RbValue self)
        {
            var data = self.GetRDataObject<SpriteData>();
            return ((int)data.WavePhase).ToValue(state);
        }

        [RbInstanceMethod("wave_phase=")]
        public static RbValue SetWavePhase(RbState state, RbValue self, RbValue wavePhase)
        {
            var data = self.GetRDataObject<SpriteData>();
            data.WavePhase = (int)wavePhase.ToIntUnchecked();
            return state.RbNil;
        }

        [RbInstanceMethod("mirror")]
        public static RbValue GetMirror(RbState state, RbValue self)
        {
            var data = self.GetRDataObject<SpriteData>();
            return data.Mirror.ToValue(state);
        }

        [RbInstanceMethod("mirror=")]
        public static RbValue SetMirror(RbState state, RbValue self, RbValue mirror)
        {
            var data = self.GetRDataObject<SpriteData>();
            data.Mirror = mirror.IsTrue;
            return state.RbNil;
        }

        [RbInstanceMethod("bush_depth")]
        public static RbValue GetBushDepth(RbState state, RbValue self)
        {
            var data = self.GetRDataObject<SpriteData>();
            return data.BushDepth.ToValue(state);
        }

        [RbInstanceMethod("bush_depth=")]
        public static RbValue SetBushDepth(RbState state, RbValue self, RbValue bushDepth)
        {
            var data = self.GetRDataObject<SpriteData>();
            data.BushDepth = (int)bushDepth.ToIntUnchecked();
            return state.RbNil;
        }

        [RbInstanceMethod("bush_opacity")]
        public static RbValue GetBushOpacity(RbState state, RbValue self)
        {
            var data = self.GetRDataObject<SpriteData>();
            return data.BushOpacity.ToValue(state);
        }

        [RbInstanceMethod("bush_opacity=")]
        public static RbValue SetBushOpacity(RbState state, RbValue self, RbValue bushOpacity)
        {
            var data = self.GetRDataObject<SpriteData>();
            data.BushOpacity = (int)bushOpacity.ToIntUnchecked();
            return state.RbNil;
        }

        [RbInstanceMethod("opacity")]
        public static RbValue GetOpacity(RbState state, RbValue self)
        {
            var data = self.GetRDataObject<SpriteData>();
            return data.Opacity.ToValue(state);
        }

        [RbInstanceMethod("opacity=")]
        public static RbValue SetOpacity(RbState state, RbValue self, RbValue opacity)
        {
            var data = self.GetRDataObject<SpriteData>();
            data.Opacity = (int)opacity.ToIntUnchecked();
            return state.RbNil;
        }

        [RbInstanceMethod("blend_type")]
        public static RbValue GetBlendType(RbState state, RbValue self)
        {
            var data = self.GetRDataObject<SpriteData>();
            return data.BlendType.ToValue(state);
        }

        [RbInstanceMethod("blend_type=")]
        public static RbValue SetBlendType(RbState state, RbValue self, RbValue blendType)
        {
            var data = self.GetRDataObject<SpriteData>();
            data.BlendType = (int)blendType.ToIntUnchecked();
            return state.RbNil;
        }

        public static void Render(SpriteData data)
        {
            // if bitmap changed, then recreate the texture for sprite
            if (data.BitmapData is { Dirty: true })
            {
                RubyClasses.Bitmap.ApplyTexture2DChange(data.BitmapData);

                CreateTextureToSpriteRenderer(
                    data.BitmapData.Texture2D,
                    data,
                    data.SpriteRenderer);
            }

            data.SpriteRenderer.sortingOrder = data.Z;
            
            SetShaderProperties(data);
        }

        internal static void SetSpriteToSpriteRenderer(SpriteRenderer spriteRenderer, Texture2D newTexture)
        {
            var oldSprite = spriteRenderer.sprite;
            Object.Destroy(oldSprite);
            spriteRenderer.sprite = UnityEngine.Sprite.Create(
                newTexture, new UnityEngine.Rect(0, 0, newTexture.width, newTexture.height), new Vector2(0, 1), 1.0f);
        }

        private static void TextureCopy(Texture2D src, Texture2D dest, UnityEngine.Rect srcRect, int destX, int destY)
        {
            var tmpRenderTex = RenderTexture.GetTemporary(
                dest.width,
                dest.height,
                32,
                RenderTextureFormat.ARGB32,
                RenderTextureReadWrite.sRGB);
            var destRect = new UnityEngine.Rect(destX, destY, src.width, src.height);
            RubyClasses.Bitmap.CommonStretchBlt(src, srcRect, tmpRenderTex, destRect, 1.0f);
            RubyClasses.Bitmap.RenderTextureToTexture2D(tmpRenderTex, dest);
            RenderTexture.ReleaseTemporary(tmpRenderTex);
        }
        
        private static void SetShaderProperties(SpriteData data)
        {
            // todo: cache the sprite data and check if need to update the shader properties
            var renderer = data.SpriteRenderer;
            var width = renderer.sprite.texture.width;
            var height = renderer.sprite.texture.height;
            var material = renderer.material;

            if (!renderer.sprite)
            {
                return;
            }

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

            material.SetFloat(Mirror_, data.Mirror ? 1.0f : 0.0f);
            material.SetFloat(Opacity_, data.Opacity / 255.0f);

            material.SetFloat(WaveAmp_, data.WaveAmp);
            material.SetFloat(WaveTexWidth_, width);
            material.SetFloat(WaveTexHeight_, height);
            material.SetFloat(WaveLength_, data.WaveLength);
            material.SetFloat(WavePhase_, data.WavePhase);

            material.SetFloat(BushOpacity_, data.BushOpacity / 255.0f);
            material.SetFloat(BushDepth_, data.BushDepth / 255.0f);

            if (data.ColorData != null)
            {
                material.SetVector(Color_, data.ColorData.Color);
            }

            if (data.ToneData != null)
            {
                material.SetVector(Tone_, data.ToneData.Tone);
            }
        }

        private static void CreateTextureToSpriteRenderer(Texture2D srcTexture, SpriteData data, SpriteRenderer renderer)
        {
            var newTexture = new Texture2D(
                (int)data.SrcRect.width + 2 * data.WaveAmp,
                (int)data.SrcRect.height,
                TextureFormat.ARGB32,
                false, false)
            {
                wrapMode = TextureWrapMode.Clamp
            };
            TextureCopy(srcTexture, newTexture, data.SrcRect, data.WaveAmp, 0);
            SetSpriteToSpriteRenderer(renderer, newTexture);
        }
    }
}