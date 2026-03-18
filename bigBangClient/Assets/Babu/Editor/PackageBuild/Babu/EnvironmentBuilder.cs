using System.IO;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Babu.Editor.Build.Babu
{
    class EnvironmentBuilder : Environment, IPreprocessBuildWithReport
    {
        public int callbackOrder => BuildOrder.WRITE_ENVIRONMENT;

        public void OnPreprocessBuild(BuildReport report)
        {
            BuildUtils.Build(() =>
            {
                Load();
#if UNITY_IOS
                environmentJson["package_name"] = BuildConfigBuilder.Instance.GetConfig("ios", "package_name");
#else
                environmentJson["package_name"] = BuildConfigBuilder.Instance.GetConfig("android", "package_name");
#endif
                environmentJson["channel_id"] = BuildArgs.Instance.ChannelId;
                environmentJson["release"] = BuildArgs.Instance.Release;
                environmentJson["major_version"] = BuildArgs.Instance.MajorVersion;
                environmentJson["minor_version"] = BuildArgs.Instance.MinorVersion;
                environmentJson["purchase_product_ids"] = BuildConfigBuilder.Instance.GetConfig("purchase_product_ids");
                environmentJson["purchase_product_ids"] = BuildConfigBuilder.Instance.GetConfig("purchase_product_ids");
                environmentJson["client_creat_time"] = GetBundleVersion();
                environmentJson["full_res"] = BuildArgs.Instance.FullRes;

#if DACHEN_BUNDLE
                environmentJson["operation_platform"] = "dc";
#endif

                Write();
            });
        }

        public string GetBundleVersion()
        {
            return File.ReadAllText("Assets/LocalAsset/Texts/BundleVersion.txt");
        }
    }
}
