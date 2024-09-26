using MRuby.Library.Language;
using MRuby.Library.Mapper;
using UnityEngine;

namespace RGSSUnity.RubyClasses
{
    using System;
    using System.IO;
    using UnityEngine.Networking;
    using File = UnityEngine.Windows.File;
    using Object = UnityEngine.Object;

    enum FadeState
    {
        None,
        FadeIn,
        FadeOut,
        Transition,
    }

    [RbModule("Graphics", "Unity")]
    public static class Graphics
    {
        internal static long WaitCount { get; private set; }
        internal static bool Freezing { get; private set; }
        private static FadeState FadeState { get; set; } = FadeState.None;

        private static GameRenderManager RenderMgr_;

        private static long FrameCount_ = 0;
        private static float brightness = 1.0f;
        private static readonly int Brightness_ = Shader.PropertyToID("_Brightness");

        private static long FadeDuration_;
        private static long RemainDuration_;
        private static float FromBrightness_;

        private static SpriteRenderer ScreenRenderer_;

        private static RenderTexture TransitionProcessTexture_;
        private static RenderTexture FrozenRenderTex_;
        private static RenderTexture NewRenderTex_;
        private static Texture2D TransitionRenderTex_;
        private static Material TransitionMaterial_;
        private static readonly int FrozenTex_ = Shader.PropertyToID("_FrozenTex");
        private static readonly int NewTex_ = Shader.PropertyToID("_NewTex");
        private static readonly int TransitionTex_ = Shader.PropertyToID("_TransitionTex");
        private static readonly int Vague_ = Shader.PropertyToID("_Vague");
        private static readonly int Progress_ = Shader.PropertyToID("_Progress");

        [RbInitEntryPoint]
        public static void Init(RbClass cls)
        {
            RenderMgr_ = GameRenderManager.Instance;
            Application.targetFrameRate = 60;

            TransitionMaterial_ = new Material(Shader.Find("Custom/TransitionPostprocessShader"));
        }

        public static void Render()
        {
            if (WaitCount > 0)
            {
                --WaitCount;
            }

            if (RemainDuration_ > 0)
            {
                --RemainDuration_;
            }

            ++FrameCount_;
            RenderMgr_.Update();
        }

        internal static void Postprocess(RenderTexture mainRenderTexture, RenderTexture postProcessTexture, Material screenMaterial)
        {
            if (FadeState == FadeState.Transition)
            {
                TransitionMaterial_.SetFloat(Progress_, brightness);
                UnityEngine.Graphics.Blit(null, TransitionProcessTexture_, TransitionMaterial_);
                UnityEngine.Graphics.Blit(TransitionProcessTexture_, postProcessTexture);
            }
            else
            {
                screenMaterial.SetFloat(Brightness_, brightness);
                UnityEngine.Graphics.Blit(mainRenderTexture, postProcessTexture, screenMaterial);
            }
        }

        [RbModuleMethod("update")]
        private static RbValue Update(RbState state, RbValue self)
        {
            // do not update rendering when freezing
            if (Freezing)
            {
                return state.RbNil;
            }

            // force to reset the brightness value to 0.0f or 1.0f after processing fade
            if (RemainDuration_ == 0 && FadeState != FadeState.None)
            {
                brightness = FadeState switch
                {
                    FadeState.FadeIn => 1.0f,
                    FadeState.FadeOut => 0.0f,
                    FadeState.Transition => 1.0f,
                    _ => brightness
                };
                --RemainDuration_;
                return state.RbNil;
            }

            if (RemainDuration_ < 0)
            {
                FadeDuration_ = 0;
                RemainDuration_ = 0;
                FadeState = FadeState.None;

                if (FrozenRenderTex_)
                {
                    RenderTexture.ReleaseTemporary(FrozenRenderTex_);
                    FrozenRenderTex_ = null;
                }

                if (NewRenderTex_)
                {
                    RenderTexture.ReleaseTemporary(NewRenderTex_);
                    NewRenderTex_ = null;
                }

                if (TransitionProcessTexture_)
                {
                    RenderTexture.ReleaseTemporary(TransitionProcessTexture_);
                    TransitionProcessTexture_ = null;
                }

                if (TransitionRenderTex_)
                {
                    Object.Destroy(TransitionRenderTex_);
                    TransitionRenderTex_ = null;
                }
            }

            if (RemainDuration_ > 0)
            {
                brightness = FadeState switch
                {
                    FadeState.FadeIn => FromBrightness_ + (1.0f - FromBrightness_) * (1.0f - (float)RemainDuration_ / FadeDuration_),
                    FadeState.FadeOut => FromBrightness_ * RemainDuration_ / FadeDuration_,
                    FadeState.Transition => FromBrightness_ * (1.0f - (float)RemainDuration_ / FadeDuration_),
                    _ => brightness
                };
                --RemainDuration_;
            }

            Render();
            return state.RbNil;
        }

        [RbModuleMethod("frame_rate=")]
        private static RbValue SetFrameRate(RbState state, RbValue self, RbValue val)
        {
            Application.targetFrameRate = (int)val.ToIntUnchecked();
            return state.RbNil;
        }

        [RbModuleMethod("frame_rate")]
        private static RbValue GetFrameRate(RbState state, RbValue self) => Application.targetFrameRate.ToValue(state);

        [RbModuleMethod("frame_count=")]
        private static RbValue SetFrameCount(RbState state, RbValue self, RbValue val)
        {
            FrameCount_ = val.ToIntUnchecked();
            return state.RbNil;
        }

        [RbModuleMethod("frame_count")]
        private static RbValue GetFrameCount(RbState state, RbValue self) => state.BoxInt(FrameCount_);

        [RbModuleMethod("brightness=")]
        private static RbValue SetBrightness(RbState state, RbValue self, RbValue val)
        {
            brightness = val.ToIntUnchecked() / 255.0f;
            return state.RbNil;
        }

        [RbModuleMethod("wait")]
        private static RbValue Wait(RbState state, RbValue self, RbValue duration)
        {
            WaitCount = duration.ToIntUnchecked();
            return state.RbNil;
        }

        [RbModuleMethod("brightness")]
        private static RbValue GetBrightness(RbState state, RbValue self) => ((int)brightness * 255).ToValue(state);

        [RbModuleMethod("fadeout")]
        private static RbValue FadeOut(RbState state, RbValue self, RbValue duration)
        {
            FadeState = FadeState.FadeOut;
            FadeDuration_ = duration.ToIntUnchecked();
            RemainDuration_ = FadeDuration_;
            FromBrightness_ = brightness;
            return state.RbNil;
        }

        [RbModuleMethod("fadein")]
        private static RbValue FadeIn(RbState state, RbValue self, RbValue duration)
        {
            FadeState = FadeState.FadeIn;
            FadeDuration_ = duration.ToIntUnchecked();
            RemainDuration_ = FadeDuration_;
            FromBrightness_ = brightness;
            return state.RbNil;
        }

        [RbModuleMethod("freeze")]
        private static RbValue Freeze(RbState state, RbValue self)
        {
            Freezing = true;
            return state.RbNil;
        }

        [RbModuleMethod("transition")]
        private static RbValue Transition(RbState state, RbValue self, RbValue duration, RbValue filename, RbValue vague)
        {
            // clear fade state
            Freezing = false;
            FadeState = FadeState.None;
            FromBrightness_ = brightness;

            if (filename.IsNil)
            {
                brightness = 0.0f;
                return FadeIn(state, self, duration);
            }

            var transFileName = filename.ToStringUnchecked()!;

            var vagueVal = vague.ToIntUnchecked();
            FadeDuration_ = duration.ToIntUnchecked();
            RemainDuration_ = FadeDuration_;

            var mainTexture = RenderMgr_.MainRenderTexture;
            FrozenRenderTex_ = RenderTexture.GetTemporary(mainTexture.width, mainTexture.height, 32, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            NewRenderTex_ = RenderTexture.GetTemporary(mainTexture.width, mainTexture.height, 32, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            TransitionProcessTexture_ = RenderTexture.GetTemporary(Screen.width, Screen.height, 32, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);

            // save current mainRenderTexture
            UnityEngine.Graphics.CopyTexture(mainTexture, FrozenRenderTex_);

            // Render new frame
            Render();

            // set transition state to transition to avoid standard rendering
            FadeState = FadeState.Transition;

            // load transition texture
            TransitionRenderTex_ = new Texture2D(1, 1, TextureFormat.ARGB32, false, false);
            var filePath = Path.Combine(Application.streamingAssetsPath, transFileName);

            using UnityWebRequest www = UnityWebRequest.Get(filePath);
            www.SendWebRequest();

            while (!www.isDone)
            {
            }

            if (www.result != UnityWebRequest.Result.Success)
            {
                RGSSLogger.LogError($"Failed to load image data from {filePath} :{www.error}");
                state.RaiseRGSSError("Failed to load image data, file not found");
                return state.RbNil;
            }

            var bytes = www.downloadHandler.data;

            if (!TransitionRenderTex_.LoadImage(bytes))
            {
                RGSSLogger.LogError($"Failed to load transition texture: {filePath}");
                state.RaiseRGSSError("Failed to load image data, invalid image data");
                return state.RbNil;
            }

            // get new texture
            UnityEngine.Graphics.CopyTexture(mainTexture, NewRenderTex_);

            TransitionMaterial_.SetTexture(FrozenTex_, FrozenRenderTex_);
            TransitionMaterial_.SetTexture(NewTex_, NewRenderTex_);
            TransitionMaterial_.SetTexture(TransitionTex_, TransitionRenderTex_);
            TransitionMaterial_.SetFloat(Vague_, vagueVal / 255.0f);

            return state.RbNil;
        }

        [RbModuleMethod("resize_screen")]
        private static RbValue ResizeScreen(RbState state, RbValue self, RbValue width, RbValue height)
        {
            var w = (int)width.ToIntUnchecked();
            var h = (int)height.ToIntUnchecked();

            Screen.SetResolution(w, h, false);
            return state.RbNil;
        }

        [RbModuleMethod("frame_reset")]
        private static RbValue FrameReset(RbState state, RbValue self)
        {
            FrameCount_ = 0;
            return state.RbNil;
        }

        [RbModuleMethod("width")]
        private static RbValue GetWidth(RbState state, RbValue self) => 
            GlobalConfig.LegacyMode ? GlobalConfig.LegacyModeWidth.ToValue(state) : Screen.width.ToValue(state);

        [RbModuleMethod("height")]
        private static RbValue GetHeight(RbState state, RbValue self) =>
            GlobalConfig.LegacyMode ? GlobalConfig.LegacyModeHeight.ToValue(state) : Screen.height.ToValue(state);

        [RbModuleMethod("snap_to_bitmap")]
        private static RbValue SnapToBitmap(RbState state, RbValue self)
        {
            var mainTexture = RenderMgr_.MainRenderTexture;
            var tex2d = new Texture2D(mainTexture.width, mainTexture.height, TextureFormat.RGBA32, false, false)
            {
                wrapMode = TextureWrapMode.Clamp
            };
            RenderTexture.active = mainTexture;
            tex2d.ReadPixels(new UnityEngine.Rect(0, 0, mainTexture.width, mainTexture.height), 0, 0);
            tex2d.Apply();
            RenderTexture.active = null;

            var bitmap = Bitmap.NewWithTexture(state, tex2d);
            return bitmap;
        }

        [RbModuleMethod("play_movie")]
        private static RbValue PlayMovie(RbState state, RbValue self, RbValue filename)
        {
            state.RaiseNotImplementError();
            return state.RbNil;
        }
    }
}