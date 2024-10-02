namespace RGSSUnity.Editor
{
    using System;
    using System.IO;
    using System.Text;
    using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
    using MRuby.Library;
    using MRuby.Library.Language;
    using UnityEngine;

    public class UnityEditorModule
    {
        public static void Init(RbState state)
        {
            var keeper = RbNativeObjectLiveKeeper<RMVAScriptArchiveWindow, NativeMethodFunc>.GetOrCreateKeeper(state);
            var module = state.DefineModule("UnityEditor");

            module.DefineClassMethod(
                "extract_script_to_file", ExtractScriptToFile, RbHelper.MRB_ARGS_REQ(4), out var delegateFn);
            keeper.Keep(delegateFn);

            module.DefineClassMethod(
                "deflate_script_content", DeflateScriptContent, RbHelper.MRB_ARGS_REQ(1), out var delegateFn2);
            keeper.Keep(delegateFn2);
        }

        public static RbValue ExtractScriptToFile(RbState state, RbValue self, params RbValue[] args)
        {
            var scriptId = args[0].ToInt();

            // var scriptName = args[1].ToString()!;
            var scriptName = GlobalConfig.CnVerRmva
                ? Encoding.GetEncoding("GBK").GetString(RbHelper.GetRawBytesFromRbStringObject(args[1]))
                : args[1].ToString()!;
            var bytes = RbHelper.GetRawBytesFromRbStringObject(args[2]);
            var scriptIndex = args[3].ToInt();

            var streamingAssetsPath = UnityEngine.Application.streamingAssetsPath;
            var outputDir = $"{streamingAssetsPath}/RMProject/ExportedScripts";
            var exportFileName = $"{outputDir}/{scriptIndex}-{scriptName}-{scriptId}.rb";

            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            // use ICSharpCode.SharpZipLib to inflate bytes
            using var inputStream = new MemoryStream(bytes);
            using var inflaterStream = new InflaterInputStream(inputStream);
            using var outputStream = new MemoryStream();
            inflaterStream.CopyTo(outputStream);
            var scriptString = System.Text.Encoding.UTF8.GetString(outputStream.ToArray());

            if (string.IsNullOrEmpty(scriptString))
            {
                Debug.Log($"Empty content, ignore {exportFileName}");
                return state.RbNil;
            }

            File.WriteAllText(exportFileName, scriptString, Encoding.UTF8);
            Debug.Log("Extracted script to file: " + exportFileName);

            return state.RbNil;
        }

        public static RbValue DeflateScriptContent(RbState state, RbValue self, params RbValue[] args)
        {
            string fileName;

            if (GlobalConfig.CnVerRmva)
            {
                var raw = RbHelper.GetRawBytesFromRbStringObject(args[0]);
                var scriptString = Encoding.UTF8.GetString(raw);
                fileName = Encoding.GetEncoding("GBK").GetString(raw);
            }
            else
            {
                fileName = args[0].ToString()!;
            }

            var content = File.ReadAllText(fileName);

            // use ICSharpCode.SharpZipLib to deflate string
            byte[] inputBytes = Encoding.UTF8.GetBytes(content);

            using var outputStream = new MemoryStream();
            using (var deflateStream = new DeflaterOutputStream(outputStream))
            {
                deflateStream.Write(inputBytes, 0, inputBytes.Length);
            }

            var bytes = outputStream.ToArray();
            return RbHelper.BuildRbStringObjectFromRawBytes(state, bytes);
        }
    }
}