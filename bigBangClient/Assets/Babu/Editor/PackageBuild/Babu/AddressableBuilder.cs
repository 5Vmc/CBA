using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Babu.Editor.Build.Babu
{
    internal class AddressableBuilder : IPreprocessBuildWithReport
    {
        public int callbackOrder => BuildOrder.PACKAGE_RESOURCE;

        public void OnPreprocessBuild(BuildReport report)
        {
            //AddressableAssetSettings.CleanPlayerContent();
            //AddressableAssetSettings.BuildPlayerContent();
        }
    }
}
