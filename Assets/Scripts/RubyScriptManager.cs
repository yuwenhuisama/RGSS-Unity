using RGSSUnity.RubyClasses;
using MRuby.Library;
using MRuby.Library.Language;
using MRuby.Library.Mapper;
using UnityEngine;
using System;
using System.Runtime.InteropServices;

namespace RGSSUnity
{
    public class RubyScriptManager
    {
        public static readonly RubyScriptManager Instance = new();

        public RbState State { get; private set; }
        public RbClass UnityModule { get; private set; }
        public RbClass UnityEditorModule { get; private set; }
        private RbContext context;
        private RbCompiler compiler;

        [DllImport("libmruby_marshal_c_ext_x64", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private static extern void mrb_mruby_marshal_c_gem_init(IntPtr mrb);

        [DllImport("libmruby_dir_glob_ext_x64", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private static extern void mrb_mruby_dir_glob_gem_init(IntPtr mrb);

        [DllImport("libmruby_onig_regexp_ext_x64", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private static extern void mrb_mruby_onig_regexp_gem_init(IntPtr mrb);
        
        [DllImport("libmruby_zlib_ext_x64", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private static extern void mrb_mruby_zlib_gem_init(IntPtr mrb);

        public void Initialize()
        {
            this.State = Ruby.Open();
            this.context = this.State.NewCompileContext();
            this.compiler = this.State.NewCompiler();

            mrb_mruby_marshal_c_gem_init(this.State.NativeHandler);
            mrb_mruby_onig_regexp_gem_init(this.State.NativeHandler);
            mrb_mruby_dir_glob_gem_init(this.State.NativeHandler);
            mrb_mruby_zlib_gem_init(this.State.NativeHandler);

            this.UnityModule = this.State.DefineModule("Unity");
            this.State.DefineModule("RPG");

            Kernel.Init();
            RbTypeRegisterHelper.Init(this.State, new[] { typeof(UnityModule).Assembly });
        }

        public void InitializeForEditorScripts()
        {
            this.State = Ruby.Open();
            this.context = this.State.NewCompileContext();
            this.compiler = this.State.NewCompiler();

            mrb_mruby_marshal_c_gem_init(this.State.NativeHandler);
            mrb_mruby_onig_regexp_gem_init(this.State.NativeHandler);
            mrb_mruby_dir_glob_gem_init(this.State.NativeHandler);
            mrb_mruby_zlib_gem_init(this.State.NativeHandler);
        }
        
        public RbValue LoadScriptInResourcesForUnityEditor(string fileName, out bool error)
        {
            var scriptAsset = Resources.Load<TextAsset>($"EditorScript/{fileName}");

            error = false;
            if (scriptAsset)
            {
                string scriptContent = scriptAsset.text;
                Debug.Log($"Loaded resource script content: EditorScript/{fileName}");

                this.context.SetFilename($"{fileName}.rb");
                this.compiler.SetFilename($"{fileName}.rb");

                var result = this.State.Protect((_, _, _) =>
                    this.compiler.LoadString(scriptContent, this.context), ref error, out var func);

                return result;
            }

            Debug.LogError("Failed to load script: " + fileName);
            return this.State.RbNil;
        }
        
        public RbClass GetClassUnderUnityModule(string className) => this.UnityModule.GetConst(className).ToClass();

        public void LoadMainScript()
        {
            const string fileName = "main";
            var res = this.LoadScriptInResources(fileName, out var _);
            this.State.GcRegister(res);
        }

        public RbValue LoadScriptInResources(string fileName, out bool error)
        {
            var scriptAsset = Resources.Load<TextAsset>($"RGSS/{fileName}");

            error = false;
            if (scriptAsset != null)
            {
                string scriptContent = scriptAsset.text;
                Debug.Log($"Loaded resource script content: RGSS/{fileName}");

                this.context.SetFilename($"{fileName}.rb");
                this.compiler.SetFilename($"{fileName}.rb");

                var result = this.State.Protect((_, _, _) =>
                    this.compiler.LoadString(scriptContent, this.context), ref error, out var func);

                return result;
            }

            Debug.LogError("Failed to load script: " + fileName);
            return this.State.RbNil;
        }

        public RbValue LoadAllScriptInResources(string path, out bool error)
        {
            var scriptAssets = Resources.LoadAll<TextAsset>($"RGSS/{path}");

            foreach (var scriptAsset in scriptAssets)
            {
                var scriptName = $"{path}/{scriptAsset.name}";
                Debug.Log($"scriptName: {scriptName}");
                if (Kernel.IsScriptLoaded(scriptName))
                {
                    continue;
                }

                Kernel.AddPath(scriptName);
                var res = LoadScriptInResources(scriptName, out error);

                if (error)
                {
                    return res;
                }
            }

            error = false;
            return this.State.RbNil;
        }

        public RbValue LoadScriptContentWithFileName(string fileName, string scriptContent, out bool error)
        {
            error = false;

            if (string.IsNullOrEmpty(scriptContent.Trim()))
            {
                return this.State.RbNil;
            }

            Debug.Log($"Loaded rmva script content {fileName}");

            this.context.SetFilename($"{fileName}.rmvascript");
            this.compiler.SetFilename($"{fileName}.rmvascript");

            var result = this.State.Protect((_, _, _) =>
                this.compiler.LoadString(scriptContent, this.context), ref error, out var func);

            return result;
        }

        public void Destroy()
        {
            this.compiler.Dispose();
            this.context.Dispose();
            Ruby.Close(this.State);
        }
    }
}