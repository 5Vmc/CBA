using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Babu.Editor.Build.Babu
{
    class BuildConfigBuilder : BuildConfig, IPreprocessBuildWithReport
    {
        public static BuildConfigBuilder Instance;
        public int callbackOrder => BuildOrder.LOAD_COINFIG;

        public void OnPreprocessBuild(BuildReport report)
        {
            BuildUtils.Build(() =>
            {
                Instance = this;
                Load("Assets/Babu/BuildConfig/BabuBuildConfig.json", BuildArgs.Instance.ChannelId);
            });
        }
    }
}
