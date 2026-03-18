using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Babu.Editor.Build
{
    public class BuildArgs
    {
        public static BuildArgs Instance;

        public bool FromScript = false;
        public bool Release = false;
        public string ProjectName;
        public string TargetPlatform;
        public string MajorVersion;
        public string MinorVersion;
        public string ChannelId;
        public string BuildMode = "all";
        public string RemoteLoadPath = "";
        public bool FullRes = true;

        public string ExportDir;
        public List<string> Defines = new List<string>();

        public void Init()
        {
            Instance = this;

            FromScript = GetBoolArgsValue("from_script", false);
            Release = GetBoolArgsValue("release", true);
            ProjectName = GetArgsValue("project", "test");
            TargetPlatform = GetArgsValue("target_platform", "android");
            MajorVersion = GetArgsValue("major_version", "2.23.01");
            MinorVersion = GetArgsValue("minor_version", DateTime.Now.ToString("MMdd") + "-unknown");
            ChannelId = GetArgsValue("channel_id", "MiGuPlay");
            ExportDir = GetArgsValue("export_dir", null);
            Defines = GetArgsArrayValue("defines");
            //AddDefine(Builder.talkingDataDefine);
            FullRes = GetBoolArgsValue("full_res", true);
        }

        public void AddDefine(string define)
        {
            bool isFind = Defines.Any(item => item == define);
            if (isFind == false) Defines.Add(define);
        }

        public override string ToString()
        {
            return JsonUtility.ToJson(this);
        }

        public static string GetArgsValue(string argName, string defaultValue = "")
        {
            string[] commandArgs = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < commandArgs.Length; ++i)
            {
                if (commandArgs[i] == ("-" + argName) && i < commandArgs.Length - 1 && commandArgs[i + 1].StartsWith("-") == false)
                {
                    return commandArgs[i + 1];
                }
            }
            return defaultValue;
        }

        private static bool GetBoolArgsValue(string argName, bool defaultValue = false)
        {
            string value = GetArgsValue(argName, "-1");
            if (value == "-1")
            {
                return defaultValue;
            }

            return !(value == "false" || value == "0");
        }

        private static List<string> GetArgsArrayValue(string argName)
        {
            List<string> ret = new List<string>();
            string argArrString = GetArgsValue(argName);
            if (argArrString.Length == 0)
            {
                return ret;
            }

            string[] argArr = argArrString.Split(',');
            if (argArr.Length == 0)
            {
                return ret;
            }

            for (int i = 0; i < argArr.Length; ++i)
            {
                ret.Add(argArr[i].Trim());
            }
            return ret;
        }
    }
}
