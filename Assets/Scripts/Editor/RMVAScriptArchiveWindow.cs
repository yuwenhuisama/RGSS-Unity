namespace RGSSUnity.Editor
{
    using System.IO;
    using MRuby.Library;
    using MRuby.Library.Language;
    using RubyClasses;
    using UnityEditor;
    using UnityEngine;

    public class RMVAScriptArchiveWindow : EditorWindow
    {
        private static RubyScriptManager ScriptManager_;

        [MenuItem("RGSS/RMVAScriptArchive")]
        public static void ShowWindow()
        {
            GlobalConfig.Init();

            if (ScriptManager_ != null)
            {
                ScriptManager_.Destroy();
            }

            // Show existing window instance. If one doesn't exist, make one.
            ScriptManager_ = new();
            var inst = ScriptManager_;
            inst.InitializeForEditorScripts();
            inst.LoadScriptInResources("ext/onig_regexp", out var error);
            if (error)
            {
                Debug.LogError("Failed to load script ext/onig_regexp.rb");
                return;
            }

            var state = inst.State;
            UnityEditorModule.Init(state);
            Kernel.InitForUnityEditor(state);

            var streamingAssetsPath = Application.streamingAssetsPath;
            state.SetGlobalVariable("$streaming_assets_base_path", streamingAssetsPath.ToValue(state));

            GetWindow<RMVAScriptArchiveWindow>("RMVA Script Archive Helper");
        }

        private void OnGUI()
        {
            var inst = ScriptManager_;

            // Add more GUI elements here
            if (GUILayout.Button("Extract from Script.rvdata2"))
            {
                // clear output directory
                var outputDir = $"{Application.streamingAssetsPath}/RMProject/ExportedScripts";
                if (Directory.Exists(outputDir))
                {
                    Directory.Delete(outputDir, true);
                }
                Directory.CreateDirectory(outputDir);

                inst.LoadScriptInResourcesForUnityEditor("extract_script", out var error);
                if (error)
                {
                    Debug.LogError("Failed to load script extract_script.rb");
                    return;
                }
            }

            if (GUILayout.Button("Save to Script.rvdata2"))
            {
                inst.LoadScriptInResourcesForUnityEditor("save_script", out var error);
                if (error)
                {
                    Debug.LogError("Failed to load script save_script.rb");
                    return;
                }
            }
        }
    }
}