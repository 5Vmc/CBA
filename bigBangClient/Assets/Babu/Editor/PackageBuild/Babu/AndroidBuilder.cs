#if UNITY_ANDROID
using LightJson;
using System;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Babu.Editor.Build.Babu
{
    class AndroidBuilder : IPreprocessBuildWithReport
    {
        public int callbackOrder => BuildOrder.PLATFROM_BUILD;

        public void OnPreprocessBuild(BuildReport report)
        {
            BuildUtils.Build(() =>
            {
                JsonValue config = BuildConfigBuilder.Instance.Config["android"];

                PlayerSettings.bundleVersion = BuildArgs.Instance.MajorVersion;
                PlayerSettings.Android.bundleVersionCode = DateTime.Now.Year % 10 * 100000000 + int.Parse(DateTime.Now.ToString("MMddHHmm"));
                PlayerSettings.Android.useCustomKeystore = config["custom_keystore"].AsBoolean;
                if (PlayerSettings.Android.useCustomKeystore)
                {
                    PlayerSettings.Android.keystoreName = config["keystore_path"].AsString;
                    PlayerSettings.Android.keystorePass = config["keystore_password"].AsString;
                    PlayerSettings.Android.keyaliasName = config["keyalias_name"].AsString;
                    PlayerSettings.Android.keyaliasPass = config["keyalias_password"].AsString;
                }

                // 常用设置
                PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel22;

                // PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, BuildArgs.Instance.Defines.ToArray());
            });
        }
    }
}
#endif