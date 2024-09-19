using UnityEngine;
using MRuby.Library.Language;
using MRuby.Library.Mapper;

namespace RGSSUnity.RubyClasses
{
    using System;
    using System.Collections.Generic;
    using Components;
    using Object = UnityEngine.Object;

    public class WindowSkinData
    {
        public Texture2D BackgroundTexture1 { get; private set; }

        public Texture2D BackgroundTexture2 { get; private set; }

        public Texture2D WindowBorderTexture { get; private set; }
        public Texture2D[] ScrollArrayTextures { get; private set; }

        public Texture2D SelectionBorderTexture { get; private set; }
        public Texture2D[] PauseCursorTextures { get; private set; }

        private const int RegionSize = 64;

        private static readonly Dictionary<Texture2D, WindowSkinData> Cache_ = new Dictionary<Texture2D, WindowSkinData>();

        public static WindowSkinData FromTexture2D(Texture2D skinTexture)
        {
            if (Cache_.TryGetValue(skinTexture, out var skinData))
            {
                return skinData;
            }

            skinData = new WindowSkinData();

            // A
            CopyTexture(skinTexture, out var txt, 0, 0, RegionSize, RegionSize);
            skinData.BackgroundTexture1 = txt;

            // B
            CopyTexture(skinTexture, out txt, 64, 0, 64, 64);
            var clearX = 16;
            var clearY = 16;
            var clearW = 32;
            var clearH = 32;
            var colors = new UnityEngine.Color[clearW * clearH];
            for (var i = 0; i < clearW * clearH; i++)
            {
                colors[i] = UnityEngine.Color.clear;
            }
            txt.SetPixels(clearX, clearY, clearW, clearH, colors);
            txt.Apply();
            skinData.WindowBorderTexture = txt;

            skinData.ScrollArrayTextures = new Texture2D[4];

            // top array
            CopyTexture(skinTexture, out txt, 24, 16, 16, 8);
            skinData.ScrollArrayTextures[0] = txt;

            // right array
            CopyTexture(skinTexture, out txt, 40, 24, 8, 16);
            skinData.ScrollArrayTextures[1] = txt;

            // bottom array
            CopyTexture(skinTexture, out txt, 24, 40, 16, 8);
            skinData.ScrollArrayTextures[2] = txt;

            // left array
            CopyTexture(skinTexture, out txt, 16, 24, 8, 16);
            skinData.ScrollArrayTextures[3] = txt;

            // C
            CopyTexture(skinTexture, out txt, 0, 64, 64, 64);
            skinData.BackgroundTexture2 = txt;

            // D
            CopyTexture(skinTexture, out txt, 64, 64, 32, 32);
            skinData.SelectionBorderTexture = txt;

            // Pause cursor frames
            skinData.PauseCursorTextures = new Texture2D[4];
            CopyTexture(skinTexture, out txt, 64 + 16, 64, 16, 16);
            skinData.PauseCursorTextures[0] = txt;
            CopyTexture(skinTexture, out txt, 64 + 32, 64, 16, 16);
            skinData.PauseCursorTextures[1] = txt;
            CopyTexture(skinTexture, out txt, 64 + 16, 64 + 16, 16, 16);
            skinData.PauseCursorTextures[2] = txt;
            CopyTexture(skinTexture, out txt, 64 + 32, 64 + 16, 16, 16);
            skinData.PauseCursorTextures[3] = txt;

            Cache_.Add(skinTexture, skinData);

            return skinData;
        }

        private static void CopyTexture(Texture2D from, out Texture2D to, int x, int y, int w, int h)
        {
            y = from.height - y - h;
            to = new Texture2D(w, h, TextureFormat.ARGB32, false, false);
            var fromPixels = from.GetPixels(x, y, w, h);
            to.SetPixels(0, 0, w, h, fromPixels);
            to.Apply();
        }
    }

    public class WindowData : RubyData
    {
        public WindowSkinData WindowSkinData;

        // public BitmapData ContentBitmapData;
        public SpriteRenderer WindowSpriteRenderer;
        public SpriteRenderer ContentsSpriteRenderer;
        public BitmapData ContentsBitmapData;
        public long X;
        public long Y;
        public long Z = 100;
        public long Ox;
        public long Oy;
        public long Width;

        public long Height;

        // public Texture2D Texture2D;
        // public RenderTexture RenderTexture;
        public GameObject WindowGameObject;
        public GameObject ContentsGameObject;

        public long Padding;
        public long PaddingBottom;

        public bool Dirty;

        public WindowData(RbState state) : base(state)
        {
        }
    }

    [RbClass("Window", "Object", "Unity")]
    public static class Window
    {
        private static readonly int Region_ = Shader.PropertyToID("_Region");

        [RbClassMethod("new_xywh")]
        public static RbValue NewXywh(RbState state, RbValue self, RbValue x, RbValue y, RbValue width, RbValue height, RbValue viewport)
        {
            var rx = x.ToIntUnchecked();
            var ry = y.ToIntUnchecked();
            var rw = width.ToIntUnchecked();
            var rh = height.ToIntUnchecked();

            var windowData = new WindowData(state)
            {
                X = rx,
                Y = ry,
                Width = rw,
                Height = rh,
            };

            var viewportData = viewport.GetRDataObject<ViewportData>();

            var windowObject = new GameObject("Window Object");
            windowObject.tag = "RGSSWindow";
            windowObject.transform.SetParent(viewportData.ViewportObject.transform);
            windowObject.transform.position = new Vector3(0, 0, 1);
            windowData.WindowGameObject = windowObject;
            windowObject.SetActive(false);

            var renderer = windowObject.AddComponent<SpriteRenderer>();
            windowData.WindowSpriteRenderer = renderer;

            var dataComp = windowObject.AddComponent<WindowDataComponent>();
            dataComp.WindowData = windowData;

            var contentsWindowObject = new GameObject("Contents Object");
            contentsWindowObject.transform.SetParent(windowObject.transform);
            contentsWindowObject.transform.position = new Vector3(0, 0, 1);
            windowData.ContentsGameObject = contentsWindowObject;

            var contentsRenderer = contentsWindowObject.AddComponent<SpriteRenderer>();
            windowData.ContentsSpriteRenderer = contentsRenderer;
            windowData.ContentsSpriteRenderer.material = new Material(Shader.Find("Custom/SpriteMaskShader"));

            var cls = self.ToClassUnchecked();
            var result = cls.NewObjectWithRData(windowData);

            result["@windowskin"] = state.RbNil;
            result["@contents"] = state.RbNil;
            result["@viewport"] = state.RbNil;
            return result;
        }

        [RbInstanceMethod("windowskin")]
        public static RbValue WindowSkin(RbState state, RbValue self)
        {
            return self["@windowskin"];
        }

        [RbInstanceMethod("windowskin=")]
        public static RbValue WindowSkinSet(RbState state, RbValue self, RbValue windowSkin)
        {
            var windowData = self.GetRDataObject<WindowData>();

            if (windowSkin.IsNil)
            {
                windowData.WindowSkinData = null;
            }
            else
            {
                var windowSkinBitmapData = windowSkin.GetRDataObject<BitmapData>();
                var skinTexture = windowSkinBitmapData.Texture2D;
                var windowSkinData = WindowSkinData.FromTexture2D(skinTexture);
                windowData.WindowSkinData = windowSkinData;

                BuildWindow(windowData);
            }

            self["@windowskin"] = windowSkin;
            return state.RbNil;
        }

        [RbInstanceMethod("move")]
        public static RbValue Move(RbState state, RbValue self, RbValue x, RbValue y, RbValue width, RbValue height)
        {
            return state.RbNil;
        }

        [RbInstanceMethod("contents")]
        public static RbValue Contents(RbState state, RbValue self)
        {
            return self["@contents"];
        }

        [RbInstanceMethod("contents=")]
        public static RbValue ContentsSet(RbState state, RbValue self, RbValue contents)
        {
            var data = self.GetRDataObject<WindowData>();
            var contentsBitmapData = contents.GetRDataObject<BitmapData>();

            // data.ContentBitmapData = contentsBitmapData;
            data.ContentsSpriteRenderer.sprite =
                UnityEngine.Sprite.Create(
                    contentsBitmapData.Texture2D,
                    new UnityEngine.Rect(
                        0,
                        0,
                        contentsBitmapData.Texture2D.width,
                        contentsBitmapData.Texture2D.height),
                    new Vector2(0.0f, 1.0f),
                    1.0f);

            data.ContentsBitmapData = contentsBitmapData;

            self["@contents"] = contents;
            return state.RbNil;
        }

        [RbInstanceMethod("cursor_rect")]
        public static RbValue CursorRect(RbState state, RbValue self)
        {
            return state.RbNil;
        }

        [RbInstanceMethod("cursor_rect=")]
        public static RbValue CursorRectSet(RbState state, RbValue self, RbValue cursorRect)
        {
            return state.RbNil;
        }

        [RbInstanceMethod("viewport")]
        public static RbValue Viewport(RbState state, RbValue self)
        {
            return self["@viewport"];
        }

        [RbInstanceMethod("viewport=")]
        public static RbValue ViewportSet(RbState state, RbValue self, RbValue viewport)
        {
            var data = self.GetRDataObject<WindowData>();
            var viewportData = self.GetRDataObject<ViewportData>();

            data.WindowGameObject.transform.SetParent(viewportData.ViewportObject.transform);

            self["@viewport"] = viewport;
            return state.RbNil;
        }

        [RbInstanceMethod("dispose")]
        public static RbValue Dispose(RbState state, RbValue self)
        {
            var data = self.GetRDataObject<WindowData>();
            Object.Destroy(data.WindowGameObject);
            data.WindowGameObject = null;
            return state.RbNil;
        }

        [RbInstanceMethod("disposed?")]
        public static RbValue Disposed(RbState state, RbValue self)
        {
            var data = self.GetRDataObject<WindowData>();
            return data.WindowGameObject ? state.RbTrue : state.RbFalse;
        }

        [RbInstanceMethod("update")]
        public static RbValue Update(RbState state, RbValue self)
        {
            return state.RbNil;
        }

        [RbInstanceMethod("open?")]
        public static RbValue Open(RbState state, RbValue self)
        {
            return state.RbFalse;
        }

        [RbInstanceMethod("close")]
        public static RbValue Close(RbState state, RbValue self)
        {
            return state.RbNil;
        }

        [RbInstanceMethod("active")]
        public static RbValue Active(RbState state, RbValue self)
        {
            return state.RbFalse;
        }

        [RbInstanceMethod("active=")]
        public static RbValue ActiveSet(RbState state, RbValue self, RbValue active)
        {
            return state.RbNil;
        }

        [RbInstanceMethod("visible")]
        public static RbValue Visible(RbState state, RbValue self)
        {
            var data = self.GetRDataObject<WindowData>();
            return data.WindowGameObject.activeSelf.ToValue(state);
        }

        [RbInstanceMethod("visible=")]
        public static RbValue VisibleSet(RbState state, RbValue self, RbValue visible)
        {
            var data = self.GetRDataObject<WindowData>();
            data.WindowGameObject.SetActive(visible.IsTrue);
            return state.RbNil;
        }

        [RbInstanceMethod("arrows_visible")]
        public static RbValue ArrowsVisible(RbState state, RbValue self)
        {
            return state.RbFalse;
        }

        [RbInstanceMethod("arrows_visible=")]
        public static RbValue ArrowsVisibleSet(RbState state, RbValue self, RbValue arrowsVisible)
        {
            return state.RbNil;
        }

        [RbInstanceMethod("pause")]
        public static RbValue Pause(RbState state, RbValue self)
        {
            return state.RbFalse;
        }

        [RbInstanceMethod("pause=")]
        public static RbValue PauseSet(RbState state, RbValue self, RbValue pause)
        {
            return state.RbNil;
        }

        [RbInstanceMethod("x")]
        public static RbValue X(RbState state, RbValue self)
        {
            var data = self.GetRDataObject<WindowData>();
            return data.X.ToValue(state);
        }

        [RbInstanceMethod("x=")]
        public static RbValue XSet(RbState state, RbValue self, RbValue x)
        {
            var data = self.GetRDataObject<WindowData>();
            data.X = x.ToIntUnchecked();
            return state.RbNil;
        }

        [RbInstanceMethod("y")]
        public static RbValue Y(RbState state, RbValue self)
        {
            var data = self.GetRDataObject<WindowData>();
            return data.Y.ToValue(state);
        }

        [RbInstanceMethod("y=")]
        public static RbValue YSet(RbState state, RbValue self, RbValue y)
        {
            var data = self.GetRDataObject<WindowData>();
            data.Y = y.ToIntUnchecked();
            return state.RbNil;
        }

        [RbInstanceMethod("width")]
        public static RbValue Width(RbState state, RbValue self)
        {
            return state.RbNil;
        }

        [RbInstanceMethod("width=")]
        public static RbValue WidthSet(RbState state, RbValue self, RbValue width)
        {
            return state.RbNil;
        }

        [RbInstanceMethod("height")]
        public static RbValue Height(RbState state, RbValue self)
        {
            return state.RbNil;
        }

        [RbInstanceMethod("height=")]
        public static RbValue HeightSet(RbState state, RbValue self, RbValue height)
        {
            return state.RbNil;
        }

        [RbInstanceMethod("z")]
        public static RbValue Z(RbState state, RbValue self)
        {
            var data = self.GetRDataObject<WindowData>();
            return data.Z.ToValue(state);
        }

        [RbInstanceMethod("z=")]
        public static RbValue ZSet(RbState state, RbValue self, RbValue z)
        {
            var data = self.GetRDataObject<WindowData>();
            data.Z = (int)z.ToIntUnchecked();
            return state.RbNil;
        }

        [RbInstanceMethod("ox")]
        public static RbValue Ox(RbState state, RbValue self)
        {
            var data = self.GetRDataObject<WindowData>();
            return data.Ox.ToValue(state);
        }

        [RbInstanceMethod("ox=")]
        public static RbValue OxSet(RbState state, RbValue self, RbValue ox)
        {
            var data = self.GetRDataObject<WindowData>();
            data.Ox = ox.ToIntUnchecked();
            return state.RbNil;
        }

        [RbInstanceMethod("oy")]
        public static RbValue Oy(RbState state, RbValue self)
        {
            var data = self.GetRDataObject<WindowData>();
            return data.Oy.ToValue(state);
        }

        [RbInstanceMethod("oy=")]
        public static RbValue OySet(RbState state, RbValue self, RbValue oy)
        {
            var data = self.GetRDataObject<WindowData>();
            data.Oy = oy.ToIntUnchecked();
            return state.RbNil;
        }

        [RbInstanceMethod("padding")]
        public static RbValue Padding(RbState state, RbValue self)
        {
            var data = self.GetRDataObject<WindowData>();
            return data.Padding.ToValue(state);
        }

        [RbInstanceMethod("padding=")]
        public static RbValue PaddingSet(RbState state, RbValue self, RbValue padding)
        {
            var data = self.GetRDataObject<WindowData>();
            data.Padding = padding.ToIntUnchecked();
            data.PaddingBottom = data.Padding;
            return state.RbNil;
        }

        [RbInstanceMethod("padding_bottom")]
        public static RbValue PaddingBottom(RbState state, RbValue self)
        {
            var data = self.GetRDataObject<WindowData>();
            return data.PaddingBottom.ToValue(state);
        }

        [RbInstanceMethod("padding_bottom=")]
        public static RbValue PaddingBottomSet(RbState state, RbValue self, RbValue paddingBottom)
        {
            var data = self.GetRDataObject<WindowData>();
            data.PaddingBottom = paddingBottom.ToIntUnchecked();
            return state.RbNil;
        }

        [RbInstanceMethod("opacity")]
        public static RbValue Opacity(RbState state, RbValue self)
        {
            return state.RbNil;
        }

        [RbInstanceMethod("opacity=")]
        public static RbValue OpacitySet(RbState state, RbValue self, RbValue opacity)
        {
            return state.RbNil;
        }

        [RbInstanceMethod("back_opacity")]
        public static RbValue BackOpacity(RbState state, RbValue self)
        {
            return state.RbNil;
        }

        [RbInstanceMethod("back_opacity=")]
        public static RbValue BackOpacitySet(RbState state, RbValue self, RbValue backOpacity)
        {
            return state.RbNil;
        }

        [RbInstanceMethod("contents_opacity")]
        public static RbValue ContentsOpacity(RbState state, RbValue self)
        {
            return state.RbNil;
        }

        [RbInstanceMethod("contents_opacity=")]
        public static RbValue ContentsOpacitySet(RbState state, RbValue self, RbValue contentsOpacity)
        {
            return state.RbNil;
        }

        [RbInstanceMethod("openness")]
        public static RbValue Openness(RbState state, RbValue self)
        {
            return state.RbNil;
        }

        [RbInstanceMethod("openness=")]
        public static RbValue OpennessSet(RbState state, RbValue self, RbValue openness)
        {
            return state.RbNil;
        }

        public static void Render(WindowData data)
        {
            var contentWidth = data.Width - data.Padding * 2;
            var contentHeight = data.Height - data.Padding - data.PaddingBottom;
            var contentRealWidth = data.ContentsBitmapData.Texture2D.width;
            var contentRealHeight = data.ContentsBitmapData.Texture2D.height;

            var visualRegion = new UnityEngine.Vector4(
                (float)data.Ox / contentRealWidth,
                (float)data.Oy / contentRealHeight,
                Math.Clamp((float)contentWidth / contentRealWidth, 0.0f, 1.0f),
                Math.Clamp((float)contentHeight / contentRealHeight, 0.0f, 1.0f)
            );
            data.ContentsSpriteRenderer.material.SetVector(Region_, visualRegion);
            
            data.ContentsGameObject.transform.localPosition = new Vector3(data.Padding - data.Ox, -data.Padding + data.Oy, 1);
        }

        private static void BuildWindow(WindowData data)
        {
            var skinData = data.WindowSkinData;

            // main window
            var windowSpriteRenderer = data.WindowSpriteRenderer;
            windowSpriteRenderer.drawMode = SpriteDrawMode.Sliced;
            var windowSpriteNineSliceSprite =
                UnityEngine.Sprite.Create(skinData.WindowBorderTexture,
                    new UnityEngine.Rect(0, 0, 64, 64),
                    new Vector2(0.0f, 1.0f),
                    1.0f,
                    0,
                    SpriteMeshType.FullRect,
                    new Vector4(16, 16, 16, 16));
            windowSpriteRenderer.sprite = windowSpriteNineSliceSprite;
            windowSpriteRenderer.size = new Vector2(data.Width, data.Height);
        }

        private static void TagDirty(WindowData data)
        {
            data.Dirty = true;
        }
    }
}