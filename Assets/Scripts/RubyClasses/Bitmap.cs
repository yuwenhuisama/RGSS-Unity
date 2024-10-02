using System;
using System.Collections.Generic;
using System.IO;
using MRuby.Library.Language;
using MRuby.Library.Mapper;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using File = UnityEngine.Windows.File;
using Object = UnityEngine.Object;

namespace RGSSUnity.RubyClasses
{

    public class BitmapData : RubyData
    {
        public RenderTexture RenderTexture;
        public Texture2D Texture2D;
        public UnityEngine.Rect Rect;
        public FontData FontData;
        public bool Dirty;
        public bool Tex2DDirty;
        public GameObject TextMeshProObject;
        public TextMeshPro TextMeshPro;
        
        public BitmapData(RbState state) : base(state)
        {
        }
        
        ~BitmapData()
        {
            if (RenderTexture)
            {
                Object.Destroy(RenderTexture);
            }

            if (Texture2D)
            {
                Object.Destroy(Texture2D);
            }

            if (TextMeshProObject)
            {
                Object.Destroy(TextMeshProObject);
            }
        }
    }

    [RbClass("Bitmap", "Object", "Unity")]
    public static class Bitmap
    {
        private static readonly int MainTex_ = Shader.PropertyToID("_MainTex");
        private static readonly int SrcRect_ = Shader.PropertyToID("_SrcRect");
        private static readonly int Opacity_ = Shader.PropertyToID("_Opacity");
        private static readonly int Color_ = Shader.PropertyToID("_Color");
        private static readonly int Color1_ = Shader.PropertyToID("_Color1");
        private static readonly int Color2_ = Shader.PropertyToID("_Color2");
        private static readonly int Vertical_ = Shader.PropertyToID("_Vertical");

        private static readonly HashSet<BitmapData> DirtyBitmapDataSet_ = new();
        private static readonly int TempTex_ = Shader.PropertyToID("_TempTex");
        private static readonly int Division_ = Shader.PropertyToID("_Division");
        private static readonly int Angle_ = Shader.PropertyToID("_Angle");
        private static readonly int HueAngle_ = Shader.PropertyToID("_HueAngle");

        public static void ResetDirtyDataSet()
        {
            foreach (var bitmapData in DirtyBitmapDataSet_)
            {
                bitmapData.Dirty = false;
                bitmapData.Tex2DDirty = false;
            }
            DirtyBitmapDataSet_.Clear();
        }

        [RbClassMethod("new_filename")]
        public static RbValue NewWithFileName(RbState state, RbValue self, RbValue filename)
        {
            var fileNameStr = filename.ToStringUnchecked()!;
            string filePath = Path.Combine(Application.streamingAssetsPath, fileNameStr);
            RGSSLogger.Log($"Load image: {fileNameStr}");

            using UnityWebRequest www = UnityWebRequest.Get(filePath);
            www.SendWebRequest();

            while (!www.isDone)
            {
            }

            if (www.result != UnityWebRequest.Result.Success)
            {
                state.RaiseRGSSError("Failed to load image data, file not found");
                return state.RbNil;
            }

            var imgData = www.downloadHandler.data;
            var texture2D = new Texture2D(1, 1, TextureFormat.ARGB32, false, false)
            {
                wrapMode = TextureWrapMode.Clamp
            };

            if (!texture2D.LoadImage(imgData))
            {
                state.RaiseRGSSError("Failed to load image data, invalid image data");
                return state.RbNil;
            }

            var rect = new UnityEngine.Rect(0, 0, texture2D.width, texture2D.height);
            var renderTexture = new RenderTexture(texture2D.width, texture2D.height, 32, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);

            // clear renderTexture to transparent color
            CommonClear(renderTexture);

            var res = CreateBitmapObject(state, self, renderTexture, texture2D, rect);
            return res;
        }

        [RbClassMethod("new_wh")]
        public static RbValue NewWithWidthAndHeight(RbState state, RbValue self, RbValue width, RbValue height)
        {
            var w = width.ToIntUnchecked();
            var h = height.ToIntUnchecked();

            var renderTexture = new RenderTexture((int)w, (int)h, 32, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            var rect = new UnityEngine.Rect(0, 0, w, h);
            var texture2D = new Texture2D((int)w, (int)h, TextureFormat.ARGB32, false, false)
            {
                wrapMode = TextureWrapMode.Clamp,
            };

            renderTexture.wrapMode = TextureWrapMode.Clamp;

            // set renderTexture to transparent color
            CommonClear(renderTexture);
            RenderTextureToTexture2D(renderTexture, texture2D);

            var res = CreateBitmapObject(state, self, renderTexture, texture2D, rect);

            res["@font"] = state.RbNil; 
            return res;
        }

        internal static RbValue NewWithTexture(RbState state, Texture2D texture2D)
        {
            var renderTexture = new RenderTexture(texture2D.width, texture2D.height, 32, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            var cls = RubyScriptManager.Instance.GetClassUnderUnityModule("Bitmap");
            var rect = new UnityEngine.Rect(0, 0, texture2D.width, texture2D.height);
            var res = CreateBitmapObject(state, cls.ClassObject, renderTexture, texture2D, rect);

            return res;
        }

        private static RbValue CreateBitmapObject(RbState state, RbValue self, RenderTexture renderTexture, Texture2D texture2D, UnityEngine.Rect rect)
        {
            var cls = self.ToClassUnchecked();
            var res = cls.NewObjectWithRData(new BitmapData(state)
            {
                RenderTexture = renderTexture,
                Texture2D = texture2D,
                Rect = rect,
            });

            res["@font"] = state.RbNil;
            res["@rect"] = Rect.CreateRect(state, rect.x, rect.y, rect.height, rect.width);
            return res;
        }

        [RbInstanceMethod("disposed?")]
        public static RbValue Disposed(RbState state, RbValue self)
        {
            var data = GetBitmapData(self);
            return !data.RenderTexture ? state.RbTrue : state.RbFalse;
        }

        [RbInstanceMethod("width")]
        public static RbValue Width(RbState state, RbValue self)
        {
            var data = GetBitmapData(self);
            return (data.RenderTexture?.width ?? 0).ToValue(state);
        }

        [RbInstanceMethod("height")]
        public static RbValue Height(RbState state, RbValue self)
        {
            var data = GetBitmapData(self);
            return (data.RenderTexture?.height ?? 0).ToValue(state);
        }

        [RbInstanceMethod("font")]
        public static RbValue Font(RbState state, RbValue self)
        {
            return self["@font"];
        }

        [RbInstanceMethod("font=")]
        public static RbValue SetFont(RbState state, RbValue self, RbValue font)
        {
            var data = GetBitmapData(self);
            var fontData = font.GetRDataObject<FontData>();
            data.FontData = fontData;
            self["@font"] = font;
            return state.RbNil;
        }

        [RbInstanceMethod("rect")]
        public static RbValue GetRect(RbState state, RbValue self)
        {
            return self["@rect"];
        }

        [RbInstanceMethod("rect=")]
        public static RbValue SetRect(RbState state, RbValue self, RbValue rect)
        {
            var data = GetBitmapData(self);
            var rectData = rect.GetRDataObject<RectData>();
            data.Rect = rectData.Rect;

            self["@rect"] = rect;
            return state.RbNil;
        }

        [RbInstanceMethod("blt")]
        public static RbValue Blt(RbState state, RbValue self,
            RbValue x, RbValue y, RbValue srcBitmap, RbValue srcRect, RbValue opacity)
        {
            var bitmapData = GetBitmapData(self);

            ApplyTexture2DChange(bitmapData);

            var srcTex = GetBitmapData(srcBitmap).Texture2D;
            var destTex = bitmapData.RenderTexture;
            var destTex2D = bitmapData.Texture2D;
            var srcRectData = srcRect.GetRDataObject<RectData>().Rect;
            var opacityValue = opacity.ToIntUnchecked() / 255.0f;
            var destRectData = new UnityEngine.Rect(x.ToIntUnchecked(), y.ToIntUnchecked(), srcRectData.width, srcRectData.height);

            // first render texture2d of src to renderTexture of dest
            CommonStretchBlt(srcTex, srcRectData, destTex, destRectData, opacityValue);

            // then render renderTexture of dest to texture2d of dest
            RenderTextureToTexture2D(destTex, destTex2D);
            TagDirty(bitmapData);
            return state.RbNil;
        }

        [RbInstanceMethod("stretch_blt")]
        public static RbValue StretchBlt(RbState state, RbValue self,
            RbValue destRect, RbValue srcBitmap, RbValue srcRect, RbValue opacity)
        {
            var bitmapData = GetBitmapData(self);

            ApplyTexture2DChange(bitmapData);

            var srcTex = GetBitmapData(srcBitmap).Texture2D;
            var destTex = bitmapData.RenderTexture;
            var destTex2D = bitmapData.Texture2D;
            var srcRectData = srcRect.GetRDataObject<RectData>().Rect;
            var destRectData = destRect.GetRDataObject<RectData>().Rect;
            var opacityValue = opacity.ToIntUnchecked() / 255.0f;

            CommonStretchBlt(srcTex, srcRectData, destTex, destRectData, opacityValue);
            RenderTextureToTexture2D(destTex, destTex2D);
            TagDirty(bitmapData);
            return state.RbNil;
        }

        public static void CommonStretchBlt(
            Texture srcTex,
            UnityEngine.Rect srcRectData,
            RenderTexture destTex,
            UnityEngine.Rect destRectData,
            float opacityValue)
        {
            Material stretchBltMaterial = new Material(Shader.Find("Custom/StretchBltShader"));
            stretchBltMaterial.SetTexture(MainTex_, srcTex);
            stretchBltMaterial.SetVector(SrcRect_, new Vector4(
                srcRectData.x / srcTex.width,
                (srcTex.height - srcRectData.y - srcRectData.height) / srcTex.height,
                srcRectData.width / srcTex.width,
                srcRectData.height / srcTex.height));
            stretchBltMaterial.SetFloat(Opacity_, opacityValue);

            RenderTexture.active = destTex;
            GL.PushMatrix();
            GL.LoadPixelMatrix(0, destTex.width, destTex.height, 0);
            UnityEngine.Graphics.DrawTexture(
                new UnityEngine.Rect(
                    destRectData.x,
                    destRectData.y,
                    destRectData.width,
                    destRectData.height),
                srcTex,
                stretchBltMaterial);
            GL.PopMatrix();
            RenderTexture.active = null;
        }

        [RbInstanceMethod("fill_rect")]
        public static RbValue FillRect(RbState state, RbValue self, RbValue x, RbValue y, RbValue w, RbValue h, RbValue color)
        {
            var data = GetBitmapData(self);
            var rx = x.ToIntUnchecked();
            var ry = y.ToIntUnchecked();
            var rw = w.ToIntUnchecked();
            var rh = h.ToIntUnchecked();

            var colorData = color.GetRDataObject<ColorData>().Color;

            ApplyTexture2DChange(data);
            FillRectInternal(data, colorData, rx, ry, rh, rw);
            TagDirty(data);
            return state.RbNil;
        }

        [RbInstanceMethod("gradient_fill_rect")]
        public static RbValue GradientFillRect(RbState state, RbValue self,
            RbValue x, RbValue y, RbValue w, RbValue h, RbValue color1, RbValue color2, RbValue vertical)
        {
            var data = GetBitmapData(self);
            var rx = x.ToIntUnchecked();
            var ry = y.ToIntUnchecked();
            var rw = w.ToIntUnchecked();
            var rh = h.ToIntUnchecked();

            var colorData1 = color1.GetRDataObject<ColorData>().Color;
            var colorData2 = color2.GetRDataObject<ColorData>().Color;
            var isVertical = vertical == state.RbTrue;

            ApplyTexture2DChange(data);
            Texture2DToRenderTexture(data.Texture2D, data.RenderTexture);

            Material gradientFillMaterial = new Material(Shader.Find("Custom/GradientFillRectShader"));
            gradientFillMaterial.SetColor(Color1_, colorData1);
            gradientFillMaterial.SetColor(Color2_, colorData2);
            gradientFillMaterial.SetFloat(Vertical_, isVertical ? 1.0f : 0.0f);

            RenderTexture.active = data.RenderTexture;
            GL.PushMatrix();
            GL.LoadPixelMatrix(0, data.RenderTexture.width, data.RenderTexture.height, 0);
            UnityEngine.Graphics.DrawTexture(
                new UnityEngine.Rect(rx, ry, rw, rh),
                Texture2D.whiteTexture,
                gradientFillMaterial);
            GL.PopMatrix();
            RenderTexture.active = null;

            RenderTextureToTexture2D(data.RenderTexture, data.Texture2D);
            TagDirty(data);
            return state.RbNil;
        }

        [RbInstanceMethod("clear")]
        public static RbValue Clear(RbState state, RbValue self)
        {
            var data = GetBitmapData(self);
            var renderTexture = data.RenderTexture;

            ApplyTexture2DChange(data);
            CommonClear(renderTexture);
            RenderTextureToTexture2D(data.RenderTexture, data.Texture2D);
            TagDirty(data);
            return state.RbNil;
        }

        public static void CommonClear(RenderTexture renderTexture) =>
            UnityEngine.Graphics.Blit(null, renderTexture, new Material(Shader.Find("Custom/BitmapClearShader")));

        [RbInstanceMethod("blur")]
        public static RbValue Blur(RbState state, RbValue self)
        {
            var data = GetBitmapData(self);
            var renderTexture = data.RenderTexture;
            var texture2D = data.Texture2D;

            ApplyTexture2DChange(data);
            Texture2DToRenderTexture(texture2D, renderTexture);

            // Create a material with the blur shader
            Material blurMaterial = new Material(Shader.Find("Custom/BlurShader"));

            // Apply the blur shader to the render texture
            RenderTexture.active = renderTexture;
            GL.PushMatrix();
            GL.LoadPixelMatrix(0, renderTexture.width, renderTexture.height, 0);
            UnityEngine.Graphics.Blit(texture2D, renderTexture, blurMaterial);
            GL.PopMatrix();
            RenderTexture.active = null;

            // Update the texture2D with the blurred render texture
            RenderTextureToTexture2D(renderTexture, texture2D);
            TagDirty(data);
            return state.RbNil;
        }

        [RbInstanceMethod("radial_blur")]
        public static RbValue RadiaBlur(RbState state, RbValue self, RbValue angle, RbValue division)
        {
            var angleNum = angle.ToIntUnchecked();
            var divisionNum = division.ToIntUnchecked();

            var data = GetBitmapData(self);

            ApplyTexture2DChange(data);

            var renderTexture = data.RenderTexture;
            var texture2D = data.Texture2D;
            var tempTexture = RenderTexture.GetTemporary(
                renderTexture.width, renderTexture.height, 32, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            CommonClear(tempTexture);

            var blurMaterial = new Material(Shader.Find("Custom/RadialBlurShader"));

            // First pass: Render rotation
            blurMaterial.SetFloat(Division_, divisionNum);
            blurMaterial.SetFloat(Angle_, angleNum);
            UnityEngine.Graphics.Blit(texture2D, tempTexture, blurMaterial, 0);

            // Second pass: Render radial blur using previously rendered result
            blurMaterial.SetFloat(Division_, divisionNum);
            blurMaterial.SetTexture(TempTex_, tempTexture);
            UnityEngine.Graphics.Blit(null, renderTexture, blurMaterial, 1);

            // Update the texture2D with the blurred render texture
            RenderTextureToTexture2D(renderTexture, texture2D);

            RenderTexture.ReleaseTemporary(tempTexture);
            TagDirty(data);
            return state.RbNil;
        }

        [RbInstanceMethod("clear_rect")]
        public static RbValue ClearRect(RbState state, RbValue self, RbValue x, RbValue y, RbValue w, RbValue h)
        {
            var data = GetBitmapData(self);
            var rx = x.ToIntUnchecked();
            var ry = y.ToIntUnchecked();
            var rw = w.ToIntUnchecked();
            var rh = h.ToIntUnchecked();

            ApplyTexture2DChange(data);
            FillRectInternal(data, UnityEngine.Color.clear, rx, ry, rh, rw);
            TagDirty(data);
            return state.RbNil;
        }

        [RbInstanceMethod("get_pixel")]
        public static RbValue GetPixel(RbState state, RbValue self, RbValue x, RbValue y)
        {
            var data = GetBitmapData(self);
            ApplyTexture2DChange(data);

            var rx = x.ToIntUnchecked();
            var ry = y.ToIntUnchecked();
            var tex = data.Texture2D;
            var color = tex.GetPixel((int)rx, (int)(tex.height - ry));

            return Color.CreateColor(state,
                (int)(color.r * 255),
                (int)(color.g * 255),
                (int)(color.b * 255),
                (int)(color.a * 255));
        }

        [RbInstanceMethod("set_pixel")]
        public static RbValue SetPixel(RbState state, RbValue self, RbValue x, RbValue y, RbValue color)
        {
            var data = GetBitmapData(self);
            ApplyTexture2DChange(data);

            var rx = x.ToIntUnchecked();
            var ry = y.ToIntUnchecked();
            var tex = data.Texture2D;
            var colorData = color.GetRDataObject<ColorData>().Color;

            tex.SetPixel((int)rx, tex.height - 1 - (int)ry, colorData);
            TagDirty(data, true);

            return state.RbNil;
        }

        [RbInstanceMethod("hue_change")]
        public static RbValue HueChange(RbState state, RbValue self, RbValue hue)
        {
            var data = GetBitmapData(self);
            var hueAngle = hue.ToInt();
            var texture2D = data.Texture2D;
            var renderTexture = data.RenderTexture;

            ApplyTexture2DChange(data);
            Texture2DToRenderTexture(texture2D, renderTexture);

            // Create a material with the blur shader
            Material blurMaterial = new Material(Shader.Find("Custom/HueShiftShader"));
            blurMaterial.SetFloat(HueAngle_, hueAngle % 360);

            // Apply the blur shader to the render texture
            RenderTexture.active = renderTexture;
            GL.PushMatrix();
            GL.LoadPixelMatrix(0, renderTexture.width, renderTexture.height, 0);
            UnityEngine.Graphics.Blit(texture2D, renderTexture, blurMaterial);
            GL.PopMatrix();
            RenderTexture.active = null;

            // Update the texture2D with the blurred render texture
            RenderTextureToTexture2D(renderTexture, texture2D);
            TagDirty(data);

            return state.RbNil;
        }

        [RbInstanceMethod("text_size")]
        public static RbValue TextSize(RbState state, RbValue self, RbValue text)
        {
            var data = GetBitmapData(self);
            var fontData = data.FontData;
            var textStr = text.ToString();

            GenerateTextMeshProObject(data, textStr, fontData, 0, 0, 0, out var textMehsObject, out var textMeshPro);

            // Calculate the size of the rendered text
            var rect = Rect.CreateRect(
                state,
                0,
                0,
                (long)Math.Floor(textMeshPro.preferredWidth),
                (long)Math.Floor(textMeshPro.preferredHeight));
            
            textMehsObject.SetActive(false);
            return rect;
        }

        [RbInstanceMethod("draw_text")]
        public static RbValue DrawText(RbState state, RbValue self, RbValue x, RbValue y, RbValue w, RbValue h, RbValue text, RbValue align)
        {
            var data = GetBitmapData(self);
            var fontData = data.FontData;
            ApplyTexture2DChange(data);

            var alignType = (int)align.ToIntUnchecked(); 
            
            var textToRender = text.ToStringUnchecked();
            var rx = x.ToIntUnchecked();
            var ry = y.ToIntUnchecked();
            var rw = w.ToIntUnchecked();
            var rh = h.ToIntUnchecked();

            var textRenderTexture = RenderTexture.GetTemporary(
                (int)rw, (int)rh, 32, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            CommonClear(textRenderTexture);

            // Create a new GameObject for the camera
            Camera textCamera = GameRenderManager.Instance.RenderCamera;
            textCamera.enabled = true;
            var oriCullingMask = textCamera.cullingMask;
            textCamera.cullingMask = LayerMask.GetMask("UI");

            // Set the camera to render to the RenderTexture
            textCamera.targetTexture = textRenderTexture;

            // Create a new GameObject for the TextMeshPro object
            GenerateTextMeshProObject(data, textToRender, fontData, rw, rh, alignType, out var textObject, out var textMeshPro);

            textCamera.Render();

            // todo: make shadow effect for rendered text
            textCamera.cullingMask = oriCullingMask;
            textCamera.enabled = false;
            textCamera.targetTexture = null;

            // copy the textRenderTexture to texture2d with pos (rx, ry)
            var texture2d = data.Texture2D;
            var renderTexture = data.RenderTexture;
            textObject.SetActive(false);
            RGSSLogger.Log("set text object to false");

            CommonStretchBlt(
                textRenderTexture,
                new UnityEngine.Rect(0, 0, rw, rh),
                renderTexture,
                new UnityEngine.Rect(rx, ry, rw, rh),
                1.0f);
            RenderTextureToTexture2D(renderTexture, texture2d);

            RenderTexture.ReleaseTemporary(textRenderTexture);
            TagDirty(data);
            return state.RbNil;
        }

        private static void GenerateTextMeshProObject(
            BitmapData bitmapData,
            string rtext,
            FontData fontData,
            long rw,
            long rh,
            int alignType,
            out GameObject textObject,
            out TextMeshPro textMeshPro)
        {
            if (!bitmapData.TextMeshProObject)
            {
                // Create a new GameObject for the TextMeshPro object
                textObject = new GameObject("TextMeshPro Object")
                {
                    layer = LayerMask.NameToLayer("UI"),
                };
                bitmapData.TextMeshProObject = textObject;
                // Add a TextMeshPro component to the GameObject
                textMeshPro = textObject.AddComponent<TextMeshPro>();
                bitmapData.TextMeshPro = textMeshPro;
            }
            else
            {
                textObject = bitmapData.TextMeshProObject;
                textObject.SetActive(true);
                textMeshPro = bitmapData.TextMeshPro;
            }

            // Create a new GameObject for the TextMeshPro object
            // textObject = new GameObject("TextMeshPro Object")
            // {
            //     layer = LayerMask.NameToLayer("UI"),
            // };
            // Add a TextMeshPro component to the GameObject
            // textMeshPro = textObject.AddComponent<TextMeshPro>();
            
            textMeshPro.text = rtext;
            // from Unity forums https://discussions.unity.com/t/textmesh-charactersize-vs-fontsize/17896/3
            // it says that
            // characterSize = targetSizeInWorldUnits*10.0f/fontSize;
            // not sure why but it works
            textMeshPro.fontSize = fontData.Size * 10;

            if (alignType == 1)
            {
                textMeshPro.alignment = TextAlignmentOptions.Center;
            }
            else if (alignType == 2)
            {
                textMeshPro.alignment = TextAlignmentOptions.Right;
            }
            else
            {
                textMeshPro.alignment = TextAlignmentOptions.Left;
            }

            textMeshPro.overflowMode = TextOverflowModes.Overflow;
            textMeshPro.textWrappingMode = TextWrappingModes.NoWrap;
            textMeshPro.color = fontData.Color;
            textMeshPro.rectTransform.sizeDelta = new Vector2(rw, rh);
            textMeshPro.rectTransform.pivot = new Vector2(0.0f, 1.0f);

            var fontStyle = FontStyles.Normal;
            if (fontData.Bold)
            {
                fontStyle |= FontStyles.Bold;
            }
            if (fontData.Italic)
            {
                fontStyle |= FontStyles.Italic;
            }

            textMeshPro.fontStyle = fontStyle;

            if (fontData.Outline)
            {
                textMeshPro.fontMaterial.EnableKeyword("OUTLINE_ON");
                textMeshPro.fontMaterial.SetColor(ShaderUtilities.ID_OutlineColor, new UnityEngine.Color(0, 0, 0, 128));
                textMeshPro.fontMaterial.SetFloat(ShaderUtilities.ID_OutlineWidth, 0.1f);
            }
            else
            {
                textMeshPro.fontMaterial.DisableKeyword("OUTLINE_ON");
            }

            textMeshPro.rectTransform.localPosition = new Vector3(-(rw / 2.0f), rh / 2.0f, 1.0f);
        }

        [RbInstanceMethod("dispose")]
        public static RbValue Dispose(RbState state, RbValue self)
        {
            var data = GetBitmapData(self);
            Object.Destroy(data.RenderTexture);
            Object.Destroy(data.Texture2D);
            Object.Destroy(data.TextMeshProObject);
            data.RenderTexture = null;
            data.Texture2D = null;
            data.TextMeshPro = null;
            data.TextMeshProObject = null;
            data.Dirty = false;
            data.Tex2DDirty = false;
            DirtyBitmapDataSet_.Remove(data);
            return state.RbNil;
        }

        private static void FillRectInternal(BitmapData data, UnityEngine.Color colorData, long rx, long ry, long rh, long rw)
        {
            Texture2DToRenderTexture(data.Texture2D, data.RenderTexture);

            Material fillRectMaterial = new Material(Shader.Find("Custom/FillRectShader"));
            fillRectMaterial.SetColor(Color_, colorData);

            RenderTexture.active = data.RenderTexture;
            GL.PushMatrix();
            GL.LoadPixelMatrix(0, data.RenderTexture.width, data.RenderTexture.height, 0);
            UnityEngine.Graphics.DrawTexture(
                new UnityEngine.Rect(rx, ry, rw, rh),
                Texture2D.whiteTexture,
                fillRectMaterial);
            GL.PopMatrix();
            RenderTexture.active = null;

            RenderTextureToTexture2D(data.RenderTexture, data.Texture2D);
        }

        private static void TagDirty(BitmapData data, bool tex2DDirty = false)
        {
            if (data.Dirty && data.Tex2DDirty)
            {
                return;
            }

            if (!data.Tex2DDirty && tex2DDirty)
            {
                data.Tex2DDirty = true;
            }

            data.Dirty = true;
            DirtyBitmapDataSet_.Add(data);
        }

        public static void RenderTextureToTexture2D(RenderTexture renderTexture, Texture2D texture2D)
        {
            RenderTexture.active = renderTexture;
            texture2D.ReadPixels(new UnityEngine.Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            texture2D.Apply();
            RenderTexture.active = null;
        }

        private static void Texture2DToRenderTexture(Texture2D texture2D, RenderTexture renderTexture)
        {
            UnityEngine.Graphics.Blit(texture2D, renderTexture);
        }

        internal static void ApplyTexture2DChange(BitmapData data)
        {
            if (!data.Tex2DDirty)
            {
                return;
            }

            data.Texture2D.Apply();
            data.Tex2DDirty = false;
        }

        private static BitmapData GetBitmapData(RbValue self) =>
            self.GetRDataObject<BitmapData>();
    }
}