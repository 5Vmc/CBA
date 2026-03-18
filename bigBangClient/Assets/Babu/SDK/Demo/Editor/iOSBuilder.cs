#if UNITY_IOS
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Babu.Editor.Build.Babu.ThirdPart.Demo
{
    internal class iOSBuilder : IPreprocessBuildWithReport
    {
        public int callbackOrder => BuildOrder.PLATFROM_BUILD;

        public void OnPreprocessBuild(BuildReport report)
        {
            BuildUtils.Build(() =>
            {
                // 打包ios平台的代码
            });
        }
    }
}

#endif