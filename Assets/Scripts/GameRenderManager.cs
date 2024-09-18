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
            this.screenRenderer.sprite =
                Sprite.Create(new Texture2D(Screen.width, Screen.height, TextureFormat.RGBA32, false, false),
                    new Rect(0, 0, Screen.width, Screen.height),
                    new Vector2(0.0f, 1.0f), 1.0f);
            this.screenRenderer.transform.position = new Vector3(-(Screen.width / 2), Screen.height / 2.0f, 1.0f);

            this.MainRenderTexture = new RenderTexture(Screen.width, Screen.height, 32, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            this.mainCamera = mainCamera;
            this.renderCamera = renderCamera;
            this.renderCamera.enabled = false;

            this.screenMaterial = new Material(Shader.Find("Custom/GraphicsPostprocessShader"));
            this.postProcessTexture = new RenderTexture(Screen.width, Screen.height, 32, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
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
                viewport.transform.position = new Vector3(-(viewportTex.width / 2.0f), viewportTex.height / 2.0f, 1);
                viewport.transform.Translate(new Vector3(-viewportData.Ox, viewportData.Oy, 0));

                var renderers = new List<SpriteRenderer>();

                for (int j = 0; j < spriteCnt; ++j)
                {
                    var sprite = viewport.transform.GetChild(j);
                    var spriteRenderer = sprite.GetComponent<SpriteRenderer>();
                    var texture = spriteRenderer.sprite.texture;

                    if (!texture)
                    {
                        continue;
                    }

                    if (sprite.gameObject.CompareTag("RGSSSprite"))
                    {
                        var data = sprite.GetComponent<SpriteDataComponent>().SpriteData;

                        sprite.transform.position = new Vector3(
                            data.X - viewportTex.width / 2.0f,
                            -data.Y + viewportTex.height / 2.0f,
                            data.Z);

                        if (data.RotatedAngle != 0)
                        {
                            sprite.transform.RotateAround(sprite.transform.position, Vector3.forward, data.RotatedAngle);
                            data.RotatedAngle = 0;
                        }

                        sprite.transform.Translate(-data.Ox * sprite.transform.localScale.x, data.Oy * sprite.transform.localScale.y, 0);
                        RubyClasses.Sprite.Render(data);
                    }
                    else if (sprite.gameObject.CompareTag("RGSSPlane"))
                    {
                        var data = sprite.GetComponent<PlaneDataComponent>().PlaneData;

                        sprite.transform.position = new Vector3(
                            -viewportTex.width / 2.0f,
                            viewportTex.height / 2.0f,
                            data.Z);
                        RubyClasses.Plane.Render(data);
                    }

                    spriteRenderer.enabled = true;
                    renderers.Add(spriteRenderer);

                    // render to viewport texture
                    this.renderCamera.Render();
                }

                foreach (var s in renderers)
                {
                    s.enabled = false;
                }

                Bitmap.RenderTextureToTexture2D(viewportTex, viewportRenderer.sprite.texture);

                // put viewport renderer to real position
                viewport.transform.position = new Vector3(-Screen.width / 2.0f + vx, Screen.height / 2.0f - vy, 1);
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