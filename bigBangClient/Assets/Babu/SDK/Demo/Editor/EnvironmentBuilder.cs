using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Babu.Editor.Build.Babu.ThirdPart.Demo
{
    public class EnvironmentBuilder : Environment, IPreprocessBuildWithReport
    {
        public int callbackOrder => BuildOrder.LOAD_COINFIG;

        public void OnPreprocessBuild(BuildReport report)
        {
            BuildUtils.Build(() =>
            {
                // 写入到配置文件，用于游戏启动的时候使用
                // SaveEnvironment(BuildConfigBuilder.Instance.Config, "demo_");
            });
        }
    }
}
