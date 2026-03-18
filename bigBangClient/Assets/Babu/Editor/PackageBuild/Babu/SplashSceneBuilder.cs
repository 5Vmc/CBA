
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Babu.Editor.Build.Babu
{
    // 启动屏设置
    class SplashSceneBuilder : IPreprocessBuildWithReport
    {
        public int callbackOrder => BuildOrder.PROCESS_SPLASH_SCREEN;

        public void OnPreprocessBuild(BuildReport report)
        {
            BuildUtils.Build(() =>
            {
                PlayerSettings.SplashScreen.show = false;
            });
        }
    }
}
