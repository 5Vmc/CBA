#if UNITY_ANDROID
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Babu.Editor.Build.Babu.ThirdPart.Demo
{
    internal class AndroidBuilder : IPreprocessBuildWithReport
    {
        public int callbackOrder => BuildOrder.PLATFROM_BUILD;

        public void OnPreprocessBuild(BuildReport report)
        {
            BuildUtils.Build(() =>
            {
                // 打包android平台的代码
            });
        }
    }
}

#endif