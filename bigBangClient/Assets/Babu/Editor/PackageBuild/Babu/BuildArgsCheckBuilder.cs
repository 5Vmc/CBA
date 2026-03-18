using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Babu.Editor.Build.Babu
{
    class BuildArgsCheckBuilder : IPreprocessBuildWithReport
    {
        public int callbackOrder => BuildOrder.BUILD_ARGS_CHECK;

        public void OnPreprocessBuild(BuildReport report)
        {
            BuildUtils.Build(() =>
            {
                if (BuildArgs.Instance != null)
                {
                    return;
                }

                BuildArgs args = new BuildArgs();
                args.Init();
                if (report.summary.platform == BuildTarget.StandaloneWindows)
                {
                    args.TargetPlatform = "pc";
                }
                else if (report.summary.platform == BuildTarget.Android)
                {
                    args.TargetPlatform = "android";
                    args.ChannelId = "jrtt";
                }
                else if (report.summary.platform == BuildTarget.iOS)
                {
                    args.TargetPlatform = "ios";
                    args.ChannelId = "app_store";
                }
            });
        }
    }
}
