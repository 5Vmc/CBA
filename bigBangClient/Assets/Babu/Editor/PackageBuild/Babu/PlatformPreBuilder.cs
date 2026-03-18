using Babu.Editor.Build.Resolver;
using GooglePlayServices;
using System;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Babu.Editor.Build.Babu
{
    class PlatformPreBuilder : IPreprocessBuildWithReport
    {
        public int callbackOrder => BuildOrder.PLATFORM_PRE_BUILD;

        public void OnPreprocessBuild(BuildReport report)
        {
            BuildUtils.Build(() =>
            {
                //if (report.summary.platform == BuildTarget.iOS)
                //{
                //    PlayerSettings.SetApplicationIdentifier(report.summary.platformGroup, BuildConfigBuilder.Instance.GetConfig("ios", "package_name"));
                //}
                //else
                //{
                //    PlayerSettings.SetApplicationIdentifier(report.summary.platformGroup, BuildConfigBuilder.Instance.GetConfig("android", "package_name"));
                //}

                if (report.summary.platform == BuildTarget.Android)
                {

                    bool result = PlayServicesResolver.ResolveSync(true);
                    Debug.Log("Resolve Result: " + result);
                    if (!result)
                    {
                        throw new Exception("Resolve Failed!");
                    }

                    // 替换google的仓库，不然打包会很慢
                    FileUtils.ReplaceFileContent("Assets/Plugins/Android/mainTemplate.gradle", "maven.google.com", "maven.aliyun.com/repository/google");
                    AndroidManifestResolver.Resolve();
                }
            });
        }
    }
}