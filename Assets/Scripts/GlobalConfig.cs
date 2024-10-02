namespace RGSSUnity
{
    using System.IO;
    using UnityEngine;
    using UnityEngine.Networking;

    public static class GlobalConfig
    {
        [System.Serializable]
        private class RmConfig
        {
            public string rtp_path;
            public string project_path;
            public bool legacy_mode;
            public int legacy_mode_width;
            public int legacy_mode_height;
            public bool cn_ver_rmva;
        }

        private static RmConfig Config;

        public static void Init()
        {
            // read text via UnityWebRequest
            var path = Path.Join(Application.streamingAssetsPath, "rm_conf.json");
            using UnityWebRequest www = UnityWebRequest.Get(path);
            www.SendWebRequest();
            while (!www.isDone)
            {
            }

            if (www.result != UnityWebRequest.Result.Success)
            {
                RGSSLogger.LogError($"Failed to read rm_conf.json: {www.error}");
            }

            // BOM issue, see also: https://discussions.unity.com/t/jsonutility-fromjson-error-invalid-value/635192
            var res = www.downloadHandler.data;
            string jsonString;
            jsonString = System.Text.Encoding.UTF8.GetString(res, 3, res.Length - 3);

            Config = JsonUtility.FromJson<RmConfig>(jsonString);
        }

        public static string RtpPath => Config.rtp_path;
        public static string ProjectPath => Config.project_path;
        public static bool LegacyMode => Config.legacy_mode;

        public static int LegacyModeWidth => Config.legacy_mode_width;
        public static int LegacyModeHeight => Config.legacy_mode_height;

        public static bool CnVerRmva => Config.cn_ver_rmva;
    }
}