using UnityEngine;

namespace RGSSUnity
{
    public class GameManager : MonoBehaviour
    {
        public Camera MainCamera;
        public Camera RenderCamera;
        public GameObject ViewportsRoot;
        public GameObject ScreenRenderObject;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            var renderMgr = GameRenderManager.Instance;
            renderMgr.Init(this.MainCamera, this.RenderCamera, this.ViewportsRoot, this.ScreenRenderObject);

            var scriptMgr = RubyScriptManager.Instance;
            scriptMgr.Initialize();
            scriptMgr.LoadMainScript();
        }

        // Update is called once per frame
        void Update()
        {
            RubyClasses.UnityModule.Update();
        }
        
        void OnDestroy()
        {
            RubyScriptManager.Instance.Destroy();
        }
    }
}