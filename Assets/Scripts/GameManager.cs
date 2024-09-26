using UnityEngine;

namespace RGSSUnity
{
    public class GameManager : MonoBehaviour
    {
        public Camera MainCamera;
        public Camera RenderCamera;
        public GameObject ViewportsRoot;
        public GameObject ScreenRenderObject;
        public GameObject AudioSourceObject;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            GlobalConfig.Init();

            var renderMgr = GameRenderManager.Instance;
            renderMgr.Init(this.MainCamera, this.RenderCamera, this.ViewportsRoot, this.ScreenRenderObject);

            var inputMgr = InputStateRecorder.Instance;
            inputMgr.Init();

            var audioMgr = GameAudioManager.Instance;
            var bgmAudiosource = this.AudioSourceObject.transform.Find("BGMAudioSource").GetComponent<AudioSource>();
            var bgsAudiosource = this.AudioSourceObject.transform.Find("BGSAudioSource").GetComponent<AudioSource>();
            var meAudiosource = this.AudioSourceObject.transform.Find("MEAudioSource").GetComponent<AudioSource>();
            var seAudiosource = this.AudioSourceObject.transform.Find("SEAudioSource").GetComponent<AudioSource>();
            audioMgr.Init(
                bgmAudiosource,
                bgsAudiosource,
                meAudiosource,
                seAudiosource,
                coroutine => this.StartCoroutine(coroutine));

            var scriptMgr = RubyScriptManager.Instance;
            scriptMgr.Initialize();
            scriptMgr.LoadMainScript();
        }

        // Update is called once per frame
        void Update()
        {
            RubyClasses.UnityModule.Update();
            RGSSLogger.Instance.Update();
        }

        void OnApplicationQuit()
        {
            RubyScriptManager.Instance.Destroy();
        }
    }
}