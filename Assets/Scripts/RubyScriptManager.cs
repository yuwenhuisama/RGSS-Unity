using RGSSUnity.RubyClasses;
using MRuby.Library;
using MRuby.Library.Language;
using MRuby.Library.Mapper;
using UnityEngine;

namespace RGSSUnity
{

    public class RubyScriptManager
    {
        public static readonly RubyScriptManager Instance = new();

        public RbState State { get; private set; }
        public RbClass UnityModule { get; private set; }
        private RbContext context;

        public void Initialize()
        {
            this.State = Ruby.Open();
            this.context = this.State.NewCompileContext();

            this.UnityModule = this.State.DefineModule("Unity");

            Kernel.Init();
            RbTypeRegisterHelper.Init(this.State, new[] { typeof(UnityModule).Assembly });
        }

        public RbClass GetClassUnderUnityModule(string className) => this.UnityModule.GetConst(className).ToClass();

        public void LoadMainScript()
        {
            const string fileName = "main";
            var res = this.LoadScriptInResources(fileName);
            this.State.GcRegister(res);
        }

        public RbValue LoadScriptInResources(string fileName)
        {
            var scriptAsset = Resources.Load<TextAsset>($"RGSS/{fileName}");

            if (scriptAsset != null)
            {
                string scriptContent = scriptAsset.text;
                Debug.Log("Loaded resource script content: \n" + scriptContent);
                
                using var compiler = this.State.NewCompilerWithCodeString(scriptContent, this.context);
                this.context.SetFilename($"{fileName}.rb");
                compiler.SetFilename($"{fileName}.rb");

                return compiler.LoadString(scriptContent, this.context);
            }

            Debug.LogError("Failed to load script: " + fileName);
            return this.State.RbNil;
        }

        public void Destroy()
        {
            this.context.Dispose();
            Ruby.Close(this.State);
        }
    }
}