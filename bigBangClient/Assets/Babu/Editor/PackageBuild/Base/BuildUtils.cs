using LightJson;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace Babu.Editor.Build
{
    public class BuildUtils
    {
        public static string[] GetBuildScenes()
        {
            List<string> names = new List<string>();

            foreach (EditorBuildSettingsScene e in EditorBuildSettings.scenes)
            {
                if (e == null)
                    continue;
                if (e.enabled)
                    names.Add(e.path);
            }
            return names.ToArray();
        }

        public static void AddDefines(BuildTargetGroup buildTargetGroup, string defines)
        {
            // string existDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
            // if (existDefines != null && existDefines.Length > 0)
            // {
            //     existDefines = existDefines + "," + defines;
            // }
            // else
            // {
            //     existDefines = defines;
            // }
            // Debug.Log("Set Defines: " + existDefines);
            // PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, existDefines);
        }

        public static JsonValue GetChannelConfig(JsonValue config, string channelId)
        {
            if (config.AsJsonObject.ContainsKey(channelId))
            {
                return config[channelId];
            }
            return config;
        }

        // build统一入口，方便记录日志和异常捕获
        public static void Build(Action buildAction)
        {
            System.Diagnostics.StackTrace ss = new System.Diagnostics.StackTrace(true);
            System.Reflection.MethodBase mb = ss.GetFrame(1).GetMethod();

            string tag = $"[Babu] {mb.DeclaringType.FullName}.{mb.Name}";
            Debug.Log($"{tag} Begin...");
            try
            {
                buildAction();
            }
            catch (BuildFailedException e)
            {
                throw e;
            }
            catch (Exception e)
            {
                Debug.LogError($"{tag} Catch Exception: {e.Message} Stack: {e.StackTrace}");
                throw new BuildFailedException($"{tag} Build Failed!!!");
            }
            Debug.Log($"{tag} End...");
        }
    }
}
