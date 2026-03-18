using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Babu.Editor.Build.Babu
{
    // 启动屏设置
    class DevIconBuilder : IPreprocessBuildWithReport
    {
        public int callbackOrder => BuildOrder.PROCESS_DEV_ICON;

        public void OnPreprocessBuild(BuildReport report)
        {
            BuildUtils.Build(() =>
            {
                if (BuildArgs.Instance.Release == false)
                {
                    // 测试版icon
                    Texture2D devIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(BuildConfigBuilder.Instance.GetConfig("dev_icon_path"));
                    PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Android, new Texture2D[] { devIcon });
                }
            });
        }
    }
}
