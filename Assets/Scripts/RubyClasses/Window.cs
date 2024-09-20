using System;
using System.Collections.Generic;
using MRuby.Library.Language;
using MRuby.Library.Mapper;
using RGSSUnity.Components;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RGSSUnity.RubyClasses
{
    public class WindowSkinData
    {
        public Texture2D BackgroundTexture1 { get; private set; }
        public Texture2D BackgroundTexture2 { get; private set; }
        public Texture2D WindowBorderTexture { get; private set; }
        public Texture2D ScrollArrayTexture { get; private set; }
        public Texture2D SelectionBorderTexture { get; private set; }
        public Texture2D PauseCursorTexture { get; private set; }

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

            // array
            CopyTexture(skinTexture, out txt, 64, 0, 64, 64);
            skinData.ScrollArrayTexture = txt;

            // C
            CopyTexture(skinTexture, out txt, 0, 64, 64, 64);
            skinData.BackgroundTexture2 = txt;

            // D
            CopyTexture(skinTexture, out txt, 64, 64, 32, 32);
            skinData.SelectionBorderTexture = txt;

            // Pause cursor frames
            CopyTexture(skinTexture, out txt, 64 + 32, 64, 32, 32);
            skinData.PauseCursorTexture = txt;

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

        public SpriteRenderer WindowBorderSpriteRenderer;
        public SpriteRenderer WindowBackgroundSpriteRenderer;
        public SpriteRenderer ContentsSpriteRenderer;
        public SpriteRenderer CursorSpriteRenderer;
        public (SpriteRenderer T, SpriteRenderer R, SpriteRenderer B, SpriteRenderer L) ArrowObjectRenderers;
        public SpriteRenderer PauseCursorSpriteRenderer;
        public UnityEngine.Sprite[] PauseCursorSprites;
        public int PauseCursorIndex;

        public BitmapData ContentsBitmapData;
        public RectData CursorRect;

        public long X;
        public long Y;
        public long Z = 100;
        public long Ox;
        public long Oy;
        public long Width;

        public long Height;

        public GameObject WindowBorderGameObject;
        public GameObject WindowBackgroundGameObject;
        public GameObject ContentsGameObject;
        public GameObject CursorGameObject;
        public (GameObject T, GameObject R, GameObject B, GameObject L) ArrowObjects;
        public GameObject PauseGameObject;
        public int CursorFlashDirection = -1;
        public float CursorFlashAlpha = 1.0f;

        public long Padding;
        public long PaddingBottom;

        public bool ArrowsVisible;
        public bool Active;
        public bool Visible;

        public bool Dirty;

        public WindowData(RbState state) : base(state)
        {
        }
    }

    [RbClass("Window", "Object", "Unity")]
    public static class Window
    {
        private static readonly int Region_ = Shader.PropertyToID("_Region");
        private const float CursorFlashDeltaOpacity = 1.0f / 60.0f;

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
                Z = 100,
                CursorRect = new RectData(state)
                {
                    Rect = new UnityEngine.Rect(0, 0, 0, 0),
                },
                ArrowsVisible = true,
                Active = true,
                Visible = true,
            };

            var viewportData = viewport.GetRDataObject<ViewportData>();

            // begin build window basic object
            var windowObject = new GameObject("Window Object");
            windowObject.tag = "RGSSWindow";
            windowObject.transform.SetParent(viewportData.ViewportObject.transform);
            windowObject.transform.position = new Vector3(0, 0, 1);
            windowData.WindowBackgroundGameObject = windowObject;
            windowObject.SetActive(false);

            var renderer = windowObject.AddComponent<SpriteRenderer>();
            windowData.WindowBackgroundSpriteRenderer = renderer;
            renderer.sortingOrder = 100;

            var borderObject = new GameObject("Border Object");
            borderObject.transform.SetParent(windowObject.transform);
            borderObject.transform.position = new Vector3(0, 0, 1);
            windowData.WindowBorderGameObject = borderObject;

            var borderRenderer = borderObject.AddComponent<SpriteRenderer>();
            windowData.WindowBorderSpriteRenderer = borderRenderer;
            borderRenderer.sortingOrder = 101;

            // end build window basic object

            var dataComp = windowObject.AddComponent<WindowDataComponent>();
            dataComp.WindowData = windowData;

            var contentsWindowObject = new GameObject("Contents Object");
            contentsWindowObject.transform.SetParent(windowObject.transform);
            contentsWindowObject.transform.position = new Vector3(0, 0, 1);
            windowData.ContentsGameObject = contentsWindowObject;

            var contentsRenderer = contentsWindowObject.AddComponent<SpriteRenderer>();
            windowData.ContentsSpriteRenderer = contentsRenderer;
            windowData.ContentsSpriteRenderer.material = new Material(Shader.Find("Custom/SpriteMaskShader"));
            contentsRenderer.sortingOrder = 102;

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
                windowData.WindowBackgroundSpriteRenderer.sprite = null;
                windowData.WindowBorderSpriteRenderer.sprite = null;
                windowData.ContentsSpriteRenderer.sprite = null;
                windowData.CursorSpriteRenderer.sprite = null;
                windowData.PauseCursorSpriteRenderer.sprite = null;
                windowData.ArrowObjectRenderers.T.sprite = null;
                windowData.ArrowObjectRenderers.R.sprite = null;
                windowData.ArrowObjectRenderers.B.sprite = null;
                windowData.ArrowObjectRenderers.L.sprite = null;
            }
            else
            {
                var windowSkinBitmapData = windowSkin.GetRDataObject<BitmapData>();
                var skinTexture = windowSkinBitmapData.Texture2D;
                var windowSkinData = WindowSkinData.FromTexture2D(skinTexture);
                windowData.WindowSkinData = windowSkinData;

                BuildWindow(windowData);
                BuildArrows(windowData);
                BuildCursor(windowData);
                BuildPauseCursor(windowData);
            }

            self["@windowskin"] = windowSkin;
            return state.RbNil;
        }

        [RbInstanceMethod("move")]
        public static RbValue Move(RbState state, RbValue self, RbValue x, RbValue y, RbValue width, RbValue height)
        {
            var data = self.GetRDataObject<WindowData>();
            data.X = x.ToIntUnchecked();
            data.Y = y.ToIntUnchecked();
            data.Width = width.ToIntUnchecked();
            data.Height = height.ToIntUnchecked();
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
            var data = self.GetRDataObject<WindowData>();
            var cursorRect = data.CursorRect;
            return Rect.CreateRect(state, cursorRect.Rect.x, cursorRect.Rect.y, cursorRect.Rect.width, cursorRect.Rect.height);
        }

        [RbInstanceMethod("cursor_rect=")]
        public static RbValue CursorRectSet(RbState state, RbValue self, RbValue cursorRect)
        {
            var data = self.GetRDataObject<WindowData>();
            var rect = cursorRect.GetRDataObject<RectData>();
            data.CursorRect = rect;
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

            data.WindowBorderGameObject.transform.SetParent(viewportData.ViewportObject.transform);

            self["@viewport"] = viewport;
            return state.RbNil;
        }

        [RbInstanceMethod("dispose")]
        public static RbValue Dispose(RbState state, RbValue self)
        {
            var data = self.GetRDataObject<WindowData>();
            Object.Destroy(data.WindowBorderGameObject);
            data.WindowBorderGameObject = null;
            return state.RbNil;
        }

        [RbInstanceMethod("disposed?")]
        public static RbValue Disposed(RbState state, RbValue self)
        {
            var data = self.GetRDataObject<WindowData>();
            return data.WindowBorderGameObject ? state.RbTrue : state.RbFalse;
        }

        [RbInstanceMethod("update")]
        public static RbValue Update(RbState state, RbValue self)
        {
            var data = self.GetRDataObject<WindowData>();

            var alpha = data.CursorFlashAlpha;
            var direction = data.CursorFlashDirection;
            alpha += CursorFlashDeltaOpacity * direction;
            if (alpha < 0 || alpha > 1.0f)
            {
                data.CursorFlashDirection = -data.CursorFlashDirection;
            }
            data.CursorFlashAlpha = Math.Clamp(alpha, 0.0f, 1.0f);

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
            var data = self.GetRDataObject<WindowData>();
            return data.Active.ToValue(state);
        }

        [RbInstanceMethod("active=")]
        public static RbValue ActiveSet(RbState state, RbValue self, RbValue active)
        {
            var data = self.GetRDataObject<WindowData>();
            data.Active = active.IsTrue;
            return state.RbNil;
        }

        [RbInstanceMethod("visible")]
        public static RbValue Visible(RbState state, RbValue self)
        {
            var data = self.GetRDataObject<WindowData>();
            return data.Visible.ToValue(state);
        }

        [RbInstanceMethod("visible=")]
        public static RbValue VisibleSet(RbState state, RbValue self, RbValue visible)
        {
            var data = self.GetRDataObject<WindowData>();
            data.Visible = visible.IsTrue;
            return state.RbNil;
        }

        [RbInstanceMethod("arrows_visible")]
        public static RbValue ArrowsVisible(RbState state, RbValue self)
        {
            var data = self.GetRDataObject<WindowData>();
            return data.ArrowsVisible.ToValue(state);
        }

        [RbInstanceMethod("arrows_visible=")]
        public static RbValue ArrowsVisibleSet(RbState state, RbValue self, RbValue arrowsVisible)
        {
            var data = self.GetRDataObject<WindowData>();
            data.ArrowsVisible = arrowsVisible.IsTrue;
            return state.RbNil;
        }

        [RbInstanceMethod("pause")]
        public static RbValue Pause(RbState state, RbValue self)
        {
            var data = self.GetRDataObject<WindowData>();
            return data.PauseGameObject.activeSelf.ToValue(state);
        }

        [RbInstanceMethod("pause=")]
        public static RbValue PauseSet(RbState state, RbValue self, RbValue pause)
        {
            var data = self.GetRDataObject<WindowData>();
            data.PauseGameObject.SetActive(pause.IsTrue);
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
            var data = self.GetRDataObject<WindowData>();
            return data.Width.ToValue(state);
        }

        [RbInstanceMethod("width=")]
        public static RbValue WidthSet(RbState state, RbValue self, RbValue width)
        {
            var data = self.GetRDataObject<WindowData>();
            data.Width = width.ToIntUnchecked();
            return state.RbNil;
        }

        [RbInstanceMethod("height")]
        public static RbValue Height(RbState state, RbValue self)
        {
            var data = self.GetRDataObject<WindowData>();
            return data.Height.ToValue(state);
        }

        [RbInstanceMethod("height=")]
        public static RbValue HeightSet(RbState state, RbValue self, RbValue height)
        {
            var data = self.GetRDataObject<WindowData>();
            data.Height = height.ToIntUnchecked();
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
            // set order
            data.WindowBackgroundSpriteRenderer.sortingOrder = (int)data.Z;
            data.WindowBorderSpriteRenderer.sortingOrder = (int)data.Z + 1;
            data.CursorSpriteRenderer.sortingOrder = (int)data.Z + 1;
            data.ContentsSpriteRenderer.sortingOrder = (int)data.Z + 2;
            data.ArrowObjectRenderers.T.sortingOrder = (int)data.Z + 3;
            data.ArrowObjectRenderers.R.sortingOrder = (int)data.Z + 3;
            data.ArrowObjectRenderers.B.sortingOrder = (int)data.Z + 3;
            data.ArrowObjectRenderers.L.sortingOrder = (int)data.Z + 3;
            data.PauseCursorSpriteRenderer.sortingOrder = (int)data.Z + 3;

            // main window
            data.WindowBackgroundSpriteRenderer.size = new Vector2(data.Width, data.Height);
            data.WindowBorderSpriteRenderer.size = new Vector2(data.Width, data.Height);

            // contents
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

            // scroll arrays
            if (data.ArrowsVisible)
            {
                var res = CheckScroll(data);
                data.ArrowObjects.T.SetActive(res.T);
                data.ArrowObjects.R.SetActive(res.R);
                data.ArrowObjects.B.SetActive(res.B);
                data.ArrowObjects.L.SetActive(res.L);
            }
            else
            {
                data.ArrowObjects.T.SetActive(false);
                data.ArrowObjects.R.SetActive(false);
                data.ArrowObjects.B.SetActive(false);
                data.ArrowObjects.L.SetActive(false);
            }

            // cursor
            if (data.CursorRect.Rect is { width: > 0, height: > 0 })
            {
                data.CursorGameObject.SetActive(true);
                data.CursorSpriteRenderer.size = new Vector2(data.CursorRect.Rect.width, data.CursorRect.Rect.height);
                data.CursorGameObject.transform.localPosition = new Vector3(data.CursorRect.Rect.x, -data.CursorRect.Rect.y, 3);
                if (data.Active)
                {
                    var color = data.CursorSpriteRenderer.color;
                    color.a = data.CursorFlashAlpha;
                    data.CursorSpriteRenderer.color = color;
                }
            }
            else
            {
                data.CursorGameObject.SetActive(false);
            }

            // pause cursor
            if (data.PauseGameObject.activeSelf)
            {
                var sprite = data.PauseCursorSprites[data.PauseCursorIndex];
                data.PauseCursorSpriteRenderer.sprite = sprite;
                data.PauseCursorIndex = (data.PauseCursorIndex + 1) % data.PauseCursorSprites.Length;
                data.PauseGameObject.transform.localPosition =
                    new Vector3(data.Width / 2.0f - 16.0f / 2, -(data.Height - data.PaddingBottom), 3);
            }
        }

        private static void BuildWindow(WindowData data)
        {
            var skinData = data.WindowSkinData;

            // main window
            var windowBorderSpriteRenderer = data.WindowBorderSpriteRenderer;
            windowBorderSpriteRenderer.drawMode = SpriteDrawMode.Sliced;
            var windowSpriteNineSliceSprite =
                UnityEngine.Sprite.Create(skinData.WindowBorderTexture,
                    new UnityEngine.Rect(0, 0, 64, 64),
                    new Vector2(0.0f, 1.0f),
                    1.0f,
                    0,
                    SpriteMeshType.FullRect,
                    new Vector4(16, 16, 16, 16));
            windowBorderSpriteRenderer.sprite = windowSpriteNineSliceSprite;

            var windowBackgroundSpriteRender = data.WindowBackgroundSpriteRenderer;
            windowBackgroundSpriteRender.drawMode = SpriteDrawMode.Sliced;
            var windowBorderSpriteNineSliceSprite = UnityEngine.Sprite.Create(
                skinData.BackgroundTexture1,
                new UnityEngine.Rect(0, 0, 64, 64),
                new Vector2(0.0f, 1.0f),
                1.0f);
            windowBackgroundSpriteRender.sprite = windowBorderSpriteNineSliceSprite;
        }

        private static void BuildArrows(WindowData data)
        {
            var skinData = data.WindowSkinData;
            var windowObject = data.WindowBorderGameObject;
            var windowWidth = data.Width;
            var windowHeight = data.Height;

            var leftArrayObject = new GameObject("Left Array Object");
            var rightArrayObject = new GameObject("Right Array Object");
            var topArrayObject = new GameObject("Top Array Object");
            var bottomArrayObject = new GameObject("Bottom ArrayObject");

            var arrays = new GameObject[] { leftArrayObject, rightArrayObject, topArrayObject, bottomArrayObject };

            for (int i = 0; i < 4; ++i)
            {
                var array = arrays[i];
                array.transform.SetParent(windowObject.transform);
                var renderer = array.AddComponent<SpriteRenderer>();

                UnityEngine.Rect rect;
                Vector2 position;
                switch (i)
                {
                    // left
                    case 0:
                        rect = new UnityEngine.Rect(16, 24, 8, 16);
                        position = new Vector2(data.Padding, windowHeight / 2.0f - 16.0f / 2);
                        data.ArrowObjectRenderers.L = renderer;
                        break;

                    // right
                    case 1:
                        rect = new UnityEngine.Rect(40, 24, 8, 16);
                        position = new Vector2(windowWidth - data.Padding - 8.0f, windowHeight / 2.0f - 16.0f / 2);
                        data.ArrowObjectRenderers.R = renderer;
                        break;

                    // top
                    case 2:
                        rect = new UnityEngine.Rect(24, 40, 16, 8);
                        position = new Vector2(windowWidth / 2.0f - 16.0f / 2, data.Padding);
                        data.ArrowObjectRenderers.T = renderer;
                        break;

                    // bottom
                    case 3:
                        rect = new UnityEngine.Rect(24, 16, 16, 8);
                        position = new Vector2(windowWidth / 2.0f - 16.0f / 2, windowHeight - data.PaddingBottom - 8.0f);
                        data.ArrowObjectRenderers.B = renderer;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                renderer.sprite = UnityEngine.Sprite.Create(
                    skinData.ScrollArrayTexture,
                    rect,
                    new Vector2(0.0f, 1.0f),
                    1.0f);
                array.transform.localPosition = new Vector3(position.x, -position.y, 2);
                array.SetActive(false);
            }

            data.ArrowObjects = (topArrayObject, rightArrayObject, bottomArrayObject, leftArrayObject);
        }

        private static void BuildCursor(WindowData data)
        {
            var skinData = data;

            var cursorObject = new GameObject("Cursor Object");
            cursorObject.transform.SetParent(data.WindowBorderGameObject.transform);

            var renderer = cursorObject.AddComponent<SpriteRenderer>();
            renderer.drawMode = SpriteDrawMode.Sliced;
            data.CursorSpriteRenderer = renderer;

            var border = new Vector4(2, 2, 2, 2);
            renderer.sprite = UnityEngine.Sprite.Create(
                skinData.WindowSkinData.SelectionBorderTexture,
                new UnityEngine.Rect(0, 0, 32, 32),
                new Vector2(0.0f, 1.0f),
                1.0f,
                0,
                SpriteMeshType.FullRect,
                border);

            cursorObject.SetActive(false);
            data.CursorGameObject = cursorObject;
        }

        private static void BuildPauseCursor(WindowData data)
        {
            var skinData = data;

            var pauseObject = new GameObject("Pause Cursor Object");
            pauseObject.transform.SetParent(data.WindowBorderGameObject.transform);

            var renderer = pauseObject.AddComponent<SpriteRenderer>();
            data.PauseCursorSpriteRenderer = renderer;

            var sprites = new UnityEngine.Sprite[4];
            for (int i = 0; i < 4; ++i)
            {
                var x = i % 2;
                var y = i / 2;
                var region = new UnityEngine.Rect(16 * x, 16 * y, 16, 16);
                sprites[i] = UnityEngine.Sprite.Create(
                    skinData.WindowSkinData.PauseCursorTexture,
                    region,
                    new Vector2(0.0f, 1.0f),
                    1.0f);
            }

            data.PauseCursorSprites = sprites;
            data.PauseCursorIndex = 0;

            pauseObject.SetActive(false);
            data.PauseGameObject = pauseObject;
        }

        private static (bool T, bool R, bool B, bool L) CheckScroll(WindowData data)
        {
            var left = false;
            var right = false;
            var top = false;
            var bottom = false;

            var windowWidth = data.Width;
            var windowHeight = data.Height;
            var contentWidth = data.ContentsBitmapData.Texture2D.width;
            var contentHeight = data.ContentsBitmapData.Texture2D.height;

            if (data.Ox > 0)
            {
                left = true;
            }

            if (data.Oy > 0)
            {
                top = true;
            }

            if (contentWidth - data.Ox > windowWidth)
            {
                right = true;
            }

            if (contentHeight - data.Oy > windowHeight)
            {
                bottom = true;
            }

            return (top, right, bottom, left);
        }

        private static void TagDirty(WindowData data)
        {
            data.Dirty = true;
        }
    }
}