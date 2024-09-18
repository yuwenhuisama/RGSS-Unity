using TMPro;
using UnityEngine;

namespace RGSSUnity
{
    public class ShaderTest : MonoBehaviour
    {
        private static readonly int TempTex_ = Shader.PropertyToID("_TempTex");
        private static readonly int Division_ = Shader.PropertyToID("_Division");
        private static readonly int Angle_ = Shader.PropertyToID("_Angle");

        public Camera GUICamera;
        public GameObject TextMeshProObject;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            this.GUICamera.enabled = false;
            var spriteRenderer = this.GetComponent<SpriteRenderer>();
            DrawText(spriteRenderer, this.GUICamera);

            //
            // var tex = sprite.texture;
            // var newTex = new Texture2D(tex.width, tex.height, TextureFormat.ARGB32, false, false);
            // var tempTexture = RenderTexture.GetTemporary(tex.width, tex.height, 24);
            // var tempTexture2 = RenderTexture.GetTemporary(tex.width, tex.height, 24);
            //
            // var blurMaterial = new Material(Shader.Find("Custom/RadialBlurShader"));
            //
            // // First pass: Render to the temporary texture
            // blurMaterial.SetFloat(Division_, 10);
            // blurMaterial.SetFloat(Angle_, 10);
            // Graphics.Blit(tex, tempTexture, blurMaterial, 0);
            //
            // // Second pass: Render to the screen using the temporary texture
            // blurMaterial.SetFloat(Division_, 10);
            // blurMaterial.SetTexture(TempTex_, tempTexture);
            // Graphics.Blit(tempTexture, tempTexture2, blurMaterial, 1);
            //
            // // Copy the result to the new texture
            // Graphics.CopyTexture(tempTexture2, newTex);
            //
            // // Apply the new texture to the sprite
            // spriteRenderer.sprite =
            //     Sprite.Create(newTex, new Rect(0, 0, newTex.width, newTex.height), new Vector2(0.5f, 0.5f));
        }

        void DrawText(SpriteRenderer renderer, Camera textCamara)
        {
            var texture = renderer.sprite.texture;
            var newTexture = new Texture2D(texture.width, texture.height, TextureFormat.ARGB32, false, true);

            var textRenderTexture = RenderTexture.GetTemporary(40, 20, 32, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            RubyClasses.Bitmap.CommonClear(textRenderTexture);

            // Create a new GameObject for the camera
            Camera textCamera = textCamara;
            textCamera.enabled = true;
            var oriCullingMask = textCamara.cullingMask;
            textCamera.cullingMask = LayerMask.GetMask("UI");

            // Set the camera to render to the RenderTexture
            textCamera.targetTexture = textRenderTexture;

            // Create a new GameObject for the TextMeshPro object
            GameObject textObject = new GameObject("TextMeshPro Object");
            textObject.layer = LayerMask.NameToLayer("UI");

            // Add a TextMeshPro component to the GameObject
            TextMeshPro textMeshPro = textObject.AddComponent<TextMeshPro>();
            textMeshPro.text = "Hello, World!";

            // from Unity forums https://discussions.unity.com/t/textmesh-charactersize-vs-fontsize/17896/3
            // it says that
            // characterSize = targetSizeInWorldUnits*10.0f/fontSize;
            // not sure why but it works
            textMeshPro.fontSize = 24 * 10;
            textMeshPro.alignment = TextAlignmentOptions.Left;
            textMeshPro.overflowMode = TextOverflowModes.Overflow;
            textMeshPro.textWrappingMode = TextWrappingModes.NoWrap;
            textMeshPro.color = Color.red;
            textMeshPro.alignment = TextAlignmentOptions.TopLeft;
            textMeshPro.rectTransform.sizeDelta = new Vector2(0, 0);
            textMeshPro.rectTransform.pivot = new Vector2(0.0f, 1.0f);

            textMeshPro.fontMaterial.EnableKeyword("OUTLINE_ON");
            textMeshPro.fontMaterial.SetColor(ShaderUtilities.ID_OutlineColor, new Color(0, 0, 0, 128));
            textMeshPro.fontMaterial.SetFloat(ShaderUtilities.ID_OutlineWidth, 0.1f);

            textMeshPro.rectTransform.localPosition = new Vector3(-20, 10, 1);

            textCamera.Render();

            textCamera.cullingMask = oriCullingMask;
            textMeshPro.enabled = false;
            textCamera.enabled = false;
            textCamera.targetTexture = null;

            RenderTexture.active = textRenderTexture;
            newTexture.ReadPixels(new Rect(0, 0, textRenderTexture.width, textRenderTexture.height), 0, 0);
            newTexture.Apply();
            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(textRenderTexture);

            // Apply the RenderTexture to the target material
            renderer.sprite = Sprite.Create(
                newTexture,
                new Rect(0, 0, textRenderTexture.width, textRenderTexture.height),
                new Vector2(0.5f, 0.5f),
                1.0f
            );
        }

        // Update is called once per frame
        void Update()
        {
        }
    }
}