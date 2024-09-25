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
            CopyTextureToNewTexture(skinTexture, out var txt, 0, 0, RegionSize, RegionSize);
            skinData.BackgroundTexture1 = txt;

            // B
            CopyTextureToNewTexture(skinTexture, out txt, 64, 0, 64, 64);
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
            CopyTextureToNewTexture(skinTexture, out txt, 64, 0, 64, 64);
            skinData.ScrollArrayTexture = txt;

            // C
            CopyTextureToNewTexture(skinTexture, out txt, 0, 64, 64, 64);
            skinData.BackgroundTexture2 = txt;

            // D
            CopyTextureToNewTexture(skinTexture, out txt, 64, 64, 32, 32);
            skinData.SelectionBorderTexture = txt;

            // Pause cursor frames
            CopyTextureToNewTexture(skinTexture, out txt, 64 + 32, 64, 32, 32);
            skinData.PauseCursorTexture = txt;

            Cache_.Add(skinTexture, skinData);

            return skinData;
        }

        public void UpdateFromTexture2D(Texture2D skinTexture)
        {
            // A
            CopyTexture(skinTexture, this.BackgroundTexture1, 0, 0, RegionSize, RegionSize);

            // B
            CopyTexture(skinTexture, this.WindowBorderTexture, 64, 0, 64, 64);
            var clearX = 16;
            var clearY = 16;
            var clearW = 32;
            var clearH = 32;
            var colors = new UnityEngine.Color[clearW * clearH];
            for (var i = 0; i < clearW * clearH; i++)
            {
                colors[i] = UnityEngine.Color.clear;
            }
            this.WindowBorderTexture.SetPixels(clearX, clearY, clearW, clearH, colors);
            this.WindowBorderTexture.Apply();

            // array
            CopyTexture(skinTexture, this.ScrollArrayTexture, 64, 0, 64, 64);

            // C
            CopyTexture(skinTexture, this.BackgroundTexture2, 0, 64, 64, 64);

            // D
            CopyTexture(skinTexture, this.SelectionBorderTexture, 64, 64, 32, 32);

            // Pause cursor frames
            CopyTexture(skinTexture, this.PauseCursorTexture, 64 + 32, 64, 32, 32);
        }

        private static void CopyTexture(Texture2D from, Texture2D to, int x, int y, int w, int h)
        {
            y = from.height - y - h;
            var fromPixels = from.GetPixels(x, y, w, h);
            to.SetPixels(0, 0, w, h, fromPixels);
            to.Apply();
        }

        private static void CopyTextureToNewTexture(Texture2D from, out Texture2D to, int x, int y, int w, int h)
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
        public SpriteRenderer WindowBackgroundTiledSpriteRenderer;
        public SpriteRenderer WindowBackgroundSpriteRenderer;
        public SpriteRenderer ContentsSpriteRenderer;
        public SpriteRenderer CursorSpriteRenderer;
        public (SpriteRenderer T, SpriteRenderer R, SpriteRenderer B, SpriteRenderer L) ArrowObjectRenderers;
        public SpriteRenderer PauseCursorSpriteRenderer;
        public UnityEngine.Sprite[] PauseCursorSprites;
        public int PauseCursorIndex;

        public BitmapData ContentsBitmapData;
        public BitmapData WindowSkinBitmapData;

        public ToneData ToneData;

        public RectData CursorRect;

        public float X;
        public float Y;
        public int Z = 100;
        public int Ox;
        public int Oy;
        public int Width;
        public int Height;
        public int Openness;

        public GameObject WindowBorderGameObject;
        public GameObject WindowBackgroundTiledObject;
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

        public long Opacity;
        public long ContentOpacity;
        public long BackOpacity;

        public WindowData(RbState state) : base(state)
        {
        }
    }

    [RbClass("Window", "Object", "Unity")]
    public static class Window
    {
        private static readonly int Region_ = Shader.PropertyToID("_Region");
        private static readonly int Scale_ = Shader.PropertyToID("_Scale");
        private static readonly int Tone_ = Shader.PropertyToID("_Tone");
        private const float CursorFlashDeltaOpacity = 1.0f / 60.0f;

        [RbClassMethod("new_xywh")]
        public static RbValue NewXywh(RbState state, RbValue self, RbValue x, RbValue y, RbValue width, RbValue height, RbValue viewport)
        {
            var rx = x.IsInt ? x.ToIntUnchecked() : x.ToFloatUnchecked();
            var ry = y.IsInt ? y.ToIntUnchecked() : y.ToFloatUnchecked();
            var rw = width.ToIntUnchecked();
            var rh = height.ToIntUnchecked();

            var windowData = new WindowData(state)
            {
                X = (float)rx,
                Y = (float)ry,
                Width = (int)rw,
                Height = (int)rh,
                Z = 100,
                CursorRect = new RectData(state)
                {
                    Rect = new UnityEngine.Rect(0, 0, 0, 0),
                },
                ArrowsVisible = true,
                Active = true,
                Visible = true,
                Opacity = 255,
                ContentOpacity = 255,
                BackOpacity = 196,
                Openness = 255,
                ToneData = new ToneData(state)
                {
                    Tone = new Vector4(0.0f, 0.0f, 0.0f, 0.0f),
                },
            };

            var viewportData = viewport.GetRDataObject<ViewportData>();

            // begin build window basic object
            var windowObject = new GameObject("Window Object");
            windowObject.tag = "RGSSWindow";
            windowObject.transform.SetParent(viewportData.ViewportObject.transform);
            windowObject.transform.SetSiblingIndex(0);
            windowObject.transform.position = new Vector3(0, 0, 1);
            windowData.WindowBackgroundGameObject = windowObject;
            windowObject.SetActive(false);

            var renderer = windowObject.AddComponent<SpriteRenderer>();
            windowData.WindowBackgroundSpriteRenderer = renderer;
            renderer.material = new Material(Shader.Find("Custom/WindowBackgroundShader"));

            var borderObject = new GameObject("Border Object");
            borderObject.transform.SetParent(windowObject.transform);
            borderObject.transform.position = new Vector3(0, 0, 1);
            windowData.WindowBorderGameObject = borderObject;

            var borderRenderer = borderObject.AddComponent<SpriteRenderer>();
            windowData.WindowBorderSpriteRenderer = borderRenderer;

            var tiledBackgroundObject = new GameObject("Tiled Background Object");
            tiledBackgroundObject.transform.SetParent(windowObject.transform);
            tiledBackgroundObject.transform.position = new Vector3(0, 0, 1);
            windowData.WindowBackgroundTiledObject = tiledBackgroundObject;

            var tiledBackgroundRenderer = tiledBackgroundObject.AddComponent<SpriteRenderer>();
            tiledBackgroundRenderer.material = new Material(Shader.Find("Custom/TiledBackgroundShader"));
            windowData.WindowBackgroundTiledSpriteRenderer = tiledBackgroundRenderer;

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
                windowData.WindowBackgroundTiledSpriteRenderer.sprite = null;
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
                windowData.WindowSkinBitmapData = windowSkinBitmapData;

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
            data.X = (float)(x.IsInt ? x.ToIntUnchecked() : x.ToFloatUnchecked());
            data.Y = (float)(x.IsInt ? y.ToIntUnchecked() : x.ToFloatUnchecked());
            data.Width = (int)width.ToIntUnchecked();
            data.Height = (int)height.ToIntUnchecked();
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

            data.WindowBackgroundGameObject.transform.SetParent(viewportData.ViewportObject.transform);

            self["@viewport"] = viewport;
            return state.RbNil;
        }

        [RbInstanceMethod("dispose")]
        public static RbValue Dispose(RbState state, RbValue self)
        {
            var data = self.GetRDataObject<WindowData>();
            Object.Destroy(data.WindowBackgroundGameObject);
            data.WindowBackgroundGameObject = null;
            return state.RbNil;
        }

        [RbInstanceMethod("disposed?")]
        public static RbValue Disposed(RbState state, RbValue self)
        {
            var data = self.GetRDataObject<WindowData>();
            return data.WindowBackgroundGameObject ? state.RbTrue : state.RbFalse;
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
            var data = self.GetRDataObject<WindowData>();
            return (data.Openness == 255).ToValue(state);
        }

        [RbInstanceMethod("close?")]
        public static RbValue Close(RbState state, RbValue self)
        {
            var data = self.GetRDataObject<WindowData>();
            return (data.Openness == 0).ToValue(state);
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
            data.X = (float)(x.IsInt ? x.ToIntUnchecked() : x.ToFloatUnchecked());
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
            data.Y = (float)(y.IsInt ? y.ToIntUnchecked() : y.ToFloatUnchecked());
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
            data.Width = (int)width.ToIntUnchecked();
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
            data.Height = (int)height.ToIntUnchecked();
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
            data.Ox = (int)ox.ToIntUnchecked();
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
            data.Oy = (int)oy.ToIntUnchecked();
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
            var data = self.GetRDataObject<WindowData>();
            return data.Opacity.ToValue(state);
        }

        [RbInstanceMethod("opacity=")]
        public static RbValue OpacitySet(RbState state, RbValue self, RbValue opacity)
        {
            var data = self.GetRDataObject<WindowData>();
            data.Opacity = opacity.ToIntUnchecked();
            return state.RbNil;
        }

        [RbInstanceMethod("back_opacity")]
        public static RbValue BackOpacity(RbState state, RbValue self)
        {
            var data = self.GetRDataObject<WindowData>();
            return data.BackOpacity.ToValue(state);
        }

        [RbInstanceMethod("back_opacity=")]
        public static RbValue BackOpacitySet(RbState state, RbValue self, RbValue backOpacity)
        {
            var data = self.GetRDataObject<WindowData>();
            data.BackOpacity = backOpacity.ToIntUnchecked();
            return state.RbNil;
        }

        [RbInstanceMethod("contents_opacity")]
        public static RbValue ContentsOpacity(RbState state, RbValue self)
        {
            var data = self.GetRDataObject<WindowData>();
            return data.ContentOpacity.ToValue(state);
        }

        [RbInstanceMethod("contents_opacity=")]
        public static RbValue ContentsOpacitySet(RbState state, RbValue self, RbValue contentsOpacity)
        {
            var data = self.GetRDataObject<WindowData>();
            data.ContentOpacity = contentsOpacity.ToIntUnchecked();
            return state.RbNil;
        }

        [RbInstanceMethod("openness")]
        public static RbValue Openness(RbState state, RbValue self)
        {
            var data = self.GetRDataObject<WindowData>();
            return data.Openness.ToValue(state);
        }

        [RbInstanceMethod("openness=")]
        public static RbValue OpennessSet(RbState state, RbValue self, RbValue openness)
        {
            var data = self.GetRDataObject<WindowData>();
            data.Openness = (int)openness.ToIntUnchecked();
            return state.RbNil;
        }

        [RbInstanceMethod("tone")]
        public static RbValue GetTone(RbState state, RbValue self)
        {
            var data = self.GetRDataObject<WindowData>();
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
            var data = self.GetRDataObject<WindowData>();
            var toneData = tone.GetRDataObject<ToneData>();
            data.ToneData.Tone = new Vector4(
                toneData.Tone.x / 255.0f,
                toneData.Tone.y / 255.0f,
                toneData.Tone.z / 255.0f,
                toneData.Tone.w / 255.0f
            );
            return state.RbNil;
        }

        public static void Render(WindowData data)
        {
            if (data.WindowSkinBitmapData is { Dirty: true })
            {
                Bitmap.ApplyTexture2DChange(data.WindowSkinBitmapData);
                data.WindowSkinData.UpdateFromTexture2D(data.WindowSkinBitmapData.Texture2D);
            }

            if (data.ContentsBitmapData is { Dirty: true })
            {
                Bitmap.ApplyTexture2DChange(data.ContentsBitmapData);
            }

            // set order
            data.WindowBackgroundSpriteRenderer.sortingOrder = data.Z;
            data.WindowBackgroundTiledSpriteRenderer.sortingOrder = data.Z + 1;
            data.WindowBorderSpriteRenderer.sortingOrder = data.Z + 2;

            data.CursorSpriteRenderer.sortingOrder = data.Z + 2;
            data.ContentsSpriteRenderer.sortingOrder = data.Z + 3;
            data.ArrowObjectRenderers.T.sortingOrder = data.Z + 4;
            data.ArrowObjectRenderers.R.sortingOrder = data.Z + 4;
            data.ArrowObjectRenderers.B.sortingOrder = data.Z + 4;
            data.ArrowObjectRenderers.L.sortingOrder = data.Z + 4;
            data.PauseCursorSpriteRenderer.sortingOrder = data.Z + 4;

            // main window
            var opacity = data.Opacity / 255.0f;
            var color1 = data.WindowBackgroundSpriteRenderer.color;
            color1.a = data.Opacity / 255.0f * opacity;
            data.WindowBorderSpriteRenderer.color = color1;
            data.WindowBackgroundSpriteRenderer.color = color1;
            data.WindowBackgroundTiledSpriteRenderer.color = color1;

            data.WindowBackgroundSpriteRenderer.material.SetVector(Tone_, data.ToneData.Tone);

            data.WindowBackgroundTiledSpriteRenderer.material.SetVector(
                Scale_,
                new Vector4((float)(data.Width - 2) / data.WindowSkinData.BackgroundTexture2.width,
                    (float)(data.Height - 2) / data.WindowSkinData.BackgroundTexture2.height,
                    2.0f / data.WindowSkinData.BackgroundTexture2.width,
                    2.0f / data.WindowSkinData.BackgroundTexture2.height));
            data.WindowBackgroundTiledSpriteRenderer.material.SetVector(Tone_, data.ToneData.Tone);

            data.WindowBackgroundSpriteRenderer.size = new Vector2(data.Width - 2, (data.Height - 2) * (data.Openness / 255.0f));
            data.WindowBorderSpriteRenderer.size = new Vector2(data.Width, data.Height * (data.Openness / 255.0f));


            var scale = data.WindowBackgroundTiledSpriteRenderer.transform.localScale;
            scale.y = data.Openness / 255.0f;
            data.WindowBackgroundTiledSpriteRenderer.transform.localScale = scale;

            if (data.Openness == 255)
            {
                data.ContentsGameObject.SetActive(true);

                // contents
                var contentWidth = data.Width - data.Padding * 2;
                var contentHeight = data.Height - data.Padding - data.PaddingBottom;
                var contentRealWidth = data.ContentsBitmapData.Texture2D.width;
                var contentRealHeight = data.ContentsBitmapData.Texture2D.height;

                var visualRegion = new Vector4(
                    (float)data.Ox / contentRealWidth,
                    (float)data.Oy / contentRealHeight,
                    Math.Clamp((float)contentWidth / contentRealWidth, 0.0f, 1.0f),
                    Math.Clamp((float)contentHeight / contentRealHeight, 0.0f, 1.0f)
                );
                data.ContentsSpriteRenderer.material.SetVector(Region_, visualRegion);
                data.ContentsGameObject.transform.localPosition = new Vector3(data.Padding - data.Ox, -data.Padding + data.Oy, 1);

                var color2 = data.ContentsSpriteRenderer.color;
                color2.a = data.ContentOpacity / 255.0f * opacity;
                data.ContentsSpriteRenderer.color = color2;
            }
            else
            {
                data.ContentsGameObject.SetActive(false);
            }

            // scroll arrays
            if (data.ArrowsVisible && data.Openness == 255)
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
            if (data.CursorRect.Rect is { width: > 0, height: > 0 } && data.Openness == 255)
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
            if (data.PauseGameObject.activeSelf && data.Openness == 255)
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

            var windowTileSpriteRenderer = data.WindowBackgroundTiledSpriteRenderer;
            windowTileSpriteRenderer.sprite = UnityEngine.Sprite.Create(
                skinData.BackgroundTexture2,
                new UnityEngine.Rect(0, 0, 64, 64),
                new Vector2(0.0f, 1.0f),
                1.0f);
            windowTileSpriteRenderer.size = new Vector2(data.Width, data.Height);
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
    }
}