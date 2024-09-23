using RGSSUnity.RubyClasses;
using MRuby.Library;
using MRuby.Library.Language;
using MRuby.Library.Mapper;
using UnityEngine;

namespace RGSSUnity
{
    using System;
    using System.Runtime.InteropServices;

    public class RubyScriptManager
    {
        public static readonly RubyScriptManager Instance = new();

        public RbState State { get; private set; }
        public RbClass UnityModule { get; private set; }
        private RbContext context;

        [DllImport("libmruby_marshal_ext_x64", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private static extern void mrb_mruby_marshal_c_gem_init(IntPtr mrb);

        public void Initialize()
        {
            this.State = Ruby.Open();
            this.context = this.State.NewCompileContext();

            mrb_mruby_marshal_c_gem_init(this.State.NativeHandler);

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