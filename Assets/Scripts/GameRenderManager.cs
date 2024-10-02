using System.Collections.Generic;
using RGSSUnity.Components;
using RGSSUnity.RubyClasses;
using UnityEngine;
using Rect = UnityEngine.Rect;
using Sprite = UnityEngine.Sprite;

namespace RGSSUnity
{
    public class GameRenderManager
    {
        public static readonly GameRenderManager Instance = new();

        internal RenderTexture MainRenderTexture { get; private set; }
        private Camera mainCamera { get; set; }
        private Camera renderCamera { get; set; }
        private GameObject viewportsRoot { get; set; }
        private SpriteRenderer screenRenderer { get; set; }

        private RenderTexture postProcessTexture { get; set; }
        private Material screenMaterial { get; set; }

        public Camera RenderCamera => renderCamera;

        public void Init(Camera mainCamera, Camera renderCamera, GameObject viewportRoots, GameObject screenRenderObject)
        {
            this.viewportsRoot = viewportRoots;
            var pos = this.viewportsRoot.transform.position;
            pos.z = 1;
            this.viewportsRoot.transform.position = pos;
            this.screenRenderer = screenRenderObject.GetComponent<SpriteRenderer>();

            var w = Screen.width;
            var h = Screen.height;
            if (GlobalConfig.LegacyMode)
            {
                w = GlobalConfig.LegacyModeWidth;
                h = GlobalConfig.LegacyModeHeight;
            }
            this.screenRenderer.sprite =
                Sprite.Create(new Texture2D(w, h, TextureFormat.RGBA32, false, false),
                    new Rect(0, 0, w, h),
                    new Vector2(0.0f, 1.0f), 1.0f);
            this.screenRenderer.transform.position = new Vector3(-(w / 2), h / 2.0f, 1.0f);

            this.MainRenderTexture = new RenderTexture(w, h, 32, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            this.mainCamera = mainCamera;
            this.renderCamera = renderCamera;
            this.renderCamera.enabled = false;

            this.screenMaterial = new Material(Shader.Find("Custom/GraphicsPostprocessShader"));
            this.postProcessTexture = new RenderTexture(w, h, 32, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
        }

        public void Update()
        {
            this.renderCamera.enabled = true;
            this.screenRenderer.enabled = false;

            // set main camera's target to the texture of each viewport and render
            var childrenCnt = this.viewportsRoot.transform.childCount;
            var viewportRenderers = new List<SpriteRenderer>();

            // render sprites -> each viewports
            for (int i = 0; i < childrenCnt; i++)
            {
                var viewport = this.viewportsRoot.transform.GetChild(i);

                var spriteCnt = viewport.transform.childCount;

                if (spriteCnt == 0)
                {
                    continue;
                }

                var viewportRenderer = viewport.GetComponent<SpriteRenderer>();
                viewportRenderers.Add(viewportRenderer);

                var viewportData = viewport.GetComponent<ViewportDataComponent>().ViewportData;
                var viewportTex = viewportData.RenderTexture;

                var vx = viewportData.LogicX;
                var vy = viewportData.LogicY;

                this.renderCamera.targetTexture = viewportTex;

                // put viewport object start from left-top and render
                viewport.position = new Vector3(-(viewportTex.width / 2.0f), viewportTex.height / 2.0f, viewportData.Z);
                viewport.Translate(new Vector3(-viewportData.Ox, viewportData.Oy, 0));

                var scriptObjects = new List<GameObject>();

                for (int j = 0; j < spriteCnt; ++j)
                {
                    var sprite = viewport.transform.GetChild(j);

                    if (sprite.gameObject.CompareTag("RGSSSprite"))
                    {
                        var data = sprite.GetComponent<SpriteDataComponent>().SpriteData;

                        if (!data.Visible)
                        {
                            continue;
                        }

                        scriptObjects.Add(sprite.gameObject);
                        sprite.gameObject.SetActive(true);
                        sprite.localPosition = new Vector3(
                            data.X,
                            -data.Y,
                            data.Z);

                        if (data.RotatedAngle != 0)
                        {
                            sprite.RotateAround(sprite.position, Vector3.forward, data.RotatedAngle);
                            data.RotatedAngle = 0;
                        }

                        sprite.Translate(-data.Ox * sprite.localScale.x, data.Oy * sprite.localScale.y, data.Z);
                        RubyClasses.Sprite.Render(data);
                    }
                    else if (sprite.gameObject.CompareTag("RGSSPlane"))
                    {
                        var data = sprite.GetComponent<PlaneDataComponent>().PlaneData;
                        if (!data.Visible)
                        {
                            continue;
                        }

                        scriptObjects.Add(sprite.gameObject);
                        sprite.gameObject.SetActive(true);
                        sprite.localPosition = new Vector3(
                            0,
                            0,
                            data.Z);
                        RubyClasses.Plane.Render(data);
                    }
                    else if (sprite.gameObject.CompareTag("RGSSWindow"))
                    {
                        var data = sprite.GetComponent<WindowDataComponent>().WindowData;
                        if (!data.Visible)
                        {
                            continue;
                        }

                        if (data.Openness == 0)
                        {
                            continue;
                        }

                        scriptObjects.Add(sprite.gameObject);
                        sprite.gameObject.SetActive(true);

                        var openness = data.Openness;
                        var offsetY = data.Height * (1.0f - openness / 255.0f);
                        sprite.localPosition = new Vector3(
                            data.X,
                            -data.Y - offsetY / 2,
                            data.Z);
                        RubyClasses.Window.Render(data);
                    }
                }

                // render to viewport texture
                this.renderCamera.Render(); 

                foreach (var s in scriptObjects)
                {
                    s.SetActive(false);
                }

                Bitmap.RenderTextureToTexture2D(viewportTex, viewportRenderer.sprite.texture);

                // put viewport renderer to real position
                var w = Screen.width;
                var h = Screen.height;
                if (GlobalConfig.LegacyMode)
                {
                    w = GlobalConfig.LegacyModeWidth;
                    h = GlobalConfig.LegacyModeHeight;
                }
                viewport.position = new Vector3(-w / 2.0f + vx, h / 2.0f - vy, 999);
            }

            this.renderCamera.targetTexture = this.MainRenderTexture;
            foreach (var viewportRenderer in viewportRenderers)
            {
                viewportRenderer.enabled = true;
            }

            this.renderCamera.Render();

            foreach (var viewportRenderer in viewportRenderers)
            {
                viewportRenderer.enabled = false;
            }

            RubyClasses.Graphics.Postprocess(this.MainRenderTexture, this.postProcessTexture, this.screenMaterial);
            Bitmap.RenderTextureToTexture2D(this.postProcessTexture, this.screenRenderer.sprite.texture);
            this.screenRenderer.enabled = true;

            Bitmap.ResetDirtyDataSet();

            this.renderCamera.enabled = false;
        }
    }
}