using System;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using YooAsset.Editor;

namespace Babu.Editor.Build
{
    public class BuildOrder
    {
        public const int BUILD_ARGS_CHECK = 0;           // build args初始化检测
        public const int LOAD_COINFIG = 10;              // 加载配置
        public const int PROCESS_GAME_CONFIG = 11;       // 处理游戏配置
        public const int PROCESS_SPLASH_SCREEN = 11;     // 启动屏幕处理
        public const int PROCESS_DEV_ICON = 11;          // 测试iconf处理
        public const int PACKAGE_RESOURCE = 19;          // 资源打包
        public const int PLATFORM_PRE_BUILD = 20;        // 平台预处理
        public const int PLATFROM_BUILD = 25;            // 处理平台
        public const int WRITE_ENVIRONMENT = 30;         // 写入环境数据

        public const int PROCESS_XCODE_PROJECT = 900;   // 处理xcode项目
    }

    class Builder : UnityEditor.Editor
    {
        //[MenuItem("Babu/Build/Build Android")]
        //static void BuildAndroid()
        //{
        //    BuildArgs args = new BuildArgs();
        //    args.Init();
        //    args.TargetPlatform = "android";
        //    Build(args);
        //}

        //[MenuItem("Babu/Build/Build iOS")]
        //static void BuildIOS()
        //{
        //    BuildArgs args = new BuildArgs();
        //    args.Init();
        //    args.TargetPlatform = "ios";
        //    Build(args);
        //}

        //static void Build()
        //{
        //    BuildArgs args = new BuildArgs();
        //    args.Init();
        //    Build(args);
        //}

        //static void Build(BuildArgs args)
        //{
        //    Debug.Log("[Babu] Begin Build...");
        //    Debug.Log("Babu Build Args: " + args.ToString());

        //    BuildOptions buildOptions = BuildOptions.None;
        //    //if (args.FromScript == false)
        //    //{
        //    //    // 手动unity里面打包都是可调式版本
        //    //    Debug.Log("[Babu] Enalbe Debug...");
        //    //    EditorUserBuildSettings.development = true;
        //    //    EditorUserBuildSettings.connectProfiler = true;
        //    //    EditorUserBuildSettings.allowDebugging = true;
        //    //    buildOptions = BuildOptions.Development | BuildOptions.ShowBuiltPlayer | BuildOptions.ConnectWithProfiler | BuildOptions.AllowDebugging | BuildOptions.CompressWithLz4;
        //    //}

        //    // 设置远端地址
        //    AddressableAssetSettingsDefaultObject.Settings.profileSettings.SetValue(AddressableAssetSettingsDefaultObject.Settings.activeProfileId, "Remote.LoadPath", args.RemoteLoadPath);

        //    BuildReport buildReport = null;
        //    if (args.TargetPlatform == "android" || args.TargetPlatform == "google_play")
        //    {
        //        string target = args.ProjectName + ".apk";
        //        if (args.TargetPlatform == "google_play")
        //        {
        //            EditorUserBuildSettings.buildAppBundle = true;
        //            EditorUserBuildSettings.androidCreateSymbolsZip = true;
        //            target = args.ProjectName + ".aab";
        //        }
        //        else
        //        {
        //            EditorUserBuildSettings.buildAppBundle = false;
        //        }
        //        if (args.Release == false)
        //        {
        //            EditorUserBuildSettings.development = true;
        //            EditorUserBuildSettings.connectProfiler = true;
        //        }
        //        var buildAction = new Action(() =>
        //        {
        //            buildReport = BuildPipeline.BuildPlayer(BuildUtils.GetBuildScenes(), target, BuildTarget.Android, buildOptions);
        //            Debug.Log("Build Report=" + buildReport);
        //            Debug.Log("Build Result=" + buildReport.summary.result);
        //        });
        //        var packAction = new Action(() =>
        //        {
        //            // 生成版号信息
        //            var versionTxtPath = Path.Combine(Application.dataPath, "LocalAsset", "Texts", "version.txt");
        //            File.WriteAllText(versionTxtPath, args.MajorVersion);
        //            AssetDatabase.Refresh();
        //            GameConfigBuilder.EncryptConfigs("Assets/LocalAsset/Config/");
        //            AddressableAssetSettings.CleanPlayerContent();
        //            AddressableAssetSettings.BuildPlayerContent();
        //        });
        //        if (Directory.Exists(Path.Combine(Application.dataPath, "..", "ServerData", "Android")))
        //        {
        //            // 删除上一个版本的远端资源包
        //            foreach (var file in Directory.GetFiles(Path.Combine(Application.dataPath, "..", "ServerData", "Android")))
        //            {
        //                File.Delete(file);
        //            }
        //        }

        //        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, BuildArgs.Instance.Defines.ToArray());
        //        // 打游戏包+热更资源包
        //        if (args.BuildMode.Equals("all"))
        //        {
        //            HotFixBuilder.BuildAndroid64(buildAction, packAction);
        //        }
        //        // 打热更资源包
        //        else if (args.BuildMode.Equals("ab"))
        //        {
        //            HotFixBuilder.BuildAndroid64AB(packAction);
        //        }
        //        // 本地资源包转换成远端资源包
        //        HotFixBuilder.AndroidLocalToRemote();
        //    }
        //    else
        //    {
        //        string target = "xcode";

        //        GameConfigBuilder.EncryptConfigs("Assets/LocalAsset/Config/");
        //        AddressableAssetSettings.CleanPlayerContent();
        //        AddressableAssetSettings.BuildPlayerContent();
        //        buildReport = BuildPipeline.BuildPlayer(BuildUtils.GetBuildScenes(), target, BuildTarget.iOS, buildOptions);
        //    }

        //    if (args.FromScript)
        //    {
        //        if (args.BuildMode.Equals("ab")) EditorApplication.Exit(0);

        //        if (buildReport.summary.result == BuildResult.Succeeded)
        //        {
        //            Debug.Log("[Babu] Build Succ Now Exit Unity...");
        //            EditorApplication.Exit(0);
        //        }
        //        else
        //        {
        //            Debug.Log($"[Babu] Build Failed Now Exit Unity...");
        //            EditorApplication.Exit(1);
        //        }
        //    }
        //}

        static void BuildHotFix()
        {
            // SetProfile("NotFullResProfile");
            HotFixBuilder.BuildHotFix();
        }

        static void SetDefines()
        {
            BuildArgs args = new BuildArgs();
            args.Init();
            if (args.TargetPlatform == "ios")
            {
                PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, args.Defines.ToArray());
                Debug.Log("SetDefines: " + PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS).ToString());
            }
            else
            {
                PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, args.Defines.ToArray());
                Debug.Log("SetDefines: " + PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android).ToString());
            }
        }
        static void SetDefinesTestBundle()
        {
            BuildArgs args = new BuildArgs();
            args.Init();
            args.AddDefine("TEST_BUNDLE");
            if (args.TargetPlatform == "ios")
            {
                PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, args.Defines.ToArray());
                Debug.Log("TEST_BUNDLE SetDefines: " + PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS).ToString());
            }
            else
            {
                PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, args.Defines.ToArray());
                Debug.Log("TEST_BUNDLE SetDefines: " + PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android).ToString());
            }
        }
        static void SetDefinesDaChenBundle()
        {
            BuildArgs args = new BuildArgs();
            args.Init();
            args.AddDefine("DACHEN_BUNDLE");
            if (args.TargetPlatform == "ios")
            {
                PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, args.Defines.ToArray());
                Debug.Log("DACHEN_BUNDLE SetDefines: " + PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS).ToString());
            }
            else
            {
                PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, args.Defines.ToArray());
                Debug.Log("DACHEN_BUNDLE SetDefines: " + PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android).ToString());
            }
        }

        public static string talkingDataDefine = "TD_GAME";
        public static string[] GetDefines(bool isDebug)
        {
            if (isDebug)
            {
                return new string[] { talkingDataDefine, "USER_DEBUG" };
            }
            else
            {
                return new string[] { talkingDataDefine, "RELEASE" };
            }
        }
        public static void SetDefinesAndroidDebug()
        {
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, GetDefines(true));
            AssetDatabase.Refresh();
            Debug.Log("SetDefines: " + PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android).ToString());
        }
        public static void SetDefinesAndroidRelease()
        {
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, GetDefines(false));
            AssetDatabase.Refresh();
            Debug.Log("SetDefines: " + PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android).ToString());
        }
        public static void SetDefinesiOSDebug()
        {
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, GetDefines(true));
            AssetDatabase.Refresh();
            Debug.Log("SetDefines: " + PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS).ToString());
        }
        public static void SetDefinesiOSRelease()
        {
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, GetDefines(false));
            AssetDatabase.Refresh();
            Debug.Log("SetDefines: " + PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS).ToString());
        }

        [MenuItem("Babu/Build/BeforeBuildFullPack")]
        static void BeforeBuildFullPack()
        {
            int bundleVersionInt = CreateBundleVersionInt();
            HotFixBuilder.SaveBundleCreateTime(bundleVersionInt);
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
            BuildYooAsset(bundleVersionInt);
            AssetDatabase.Refresh();
        }

        [MenuItem("Babu/Build/BuildNftApk")]
        static void BuildNftApk()
        {
            string[] defines = new string[] { "RELEASE", "MiGuNft" };
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, defines);
            PlayerSettings.applicationIdentifier = "digital.qd.migu";
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "digital.qd.migu");
            AssetDatabase.Refresh();
            Debug.Log("SetNftApk End");
            int bundleVersionInt = CreateBundleVersionInt();
            HotFixBuilder.SaveBundleCreateTime(bundleVersionInt);
            BuildYooAsset(bundleVersionInt);
            AssetDatabase.Refresh();
            var buildOptions = BuildOptions.None;
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions()
            {
                scenes = new string[] { "Assets/Scenes/MainScene.unity" },
                options = buildOptions,
                locationPathName = "/Users/hanzijian/Documents/test111/cba-migu-digital-release-" + DateTime.Now.ToString("yyyy-MMdd-HHmm") + ".apk",
                target = BuildTarget.Android,
                targetGroup = BuildTargetGroup.Android,
            };
            var buildReport = BuildPipeline.BuildPlayer(buildPlayerOptions);
            Debug.Log("BuildNftApk End");
        }
        [MenuItem("Babu/Build/BuildNormalApk")]
        static void BuildNormalApk()
        {
            string[] defines = new string[] { "RELEASE", "MiGuNormal" };
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, defines);
            PlayerSettings.applicationIdentifier = "com.by.cba.migu";
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.by.cba.migu");
            AssetDatabase.Refresh();
            Debug.Log("SetNormalApk End");
            int bundleVersionInt = CreateBundleVersionInt();
            HotFixBuilder.SaveBundleCreateTime(bundleVersionInt);
            BuildYooAsset(bundleVersionInt);
            AssetDatabase.Refresh();
            var buildOptions = BuildOptions.None;
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions()
            {
                scenes = new string[] { "Assets/Scenes/MainScene.unity" },
                options = buildOptions,
                locationPathName = "/Users/hanzijian/Documents/test111/cba-migu-release-" + DateTime.Now.ToString("yyyy-MMdd-HHmm") + ".apk",
                target = BuildTarget.Android,
                targetGroup = BuildTargetGroup.Android,
            };
            var buildReport = BuildPipeline.BuildPlayer(buildPlayerOptions);
            Debug.Log("BuildNormalApk End");
        }

        //[MenuItem("Babu/Build/Export AS")]
        static void ExportAs()
        {
#if !TEST_BUNDLE
            int bundleVersionInt = CreateBundleVersionInt();
            HotFixBuilder.SaveBundleCreateTime(bundleVersionInt);
#endif

            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);

            BuildArgs args = new BuildArgs();
            args.Init();
            args.TargetPlatform = "android";
            Debug.Log("Babu Export Args: " + args.ToString());
            //BuildOptions buildOptions = BuildOptions.CleanBuildCache;
            //string target = args.ProjectName + ".apk";
            string exportDir = args.ExportDir;

            if (exportDir == null)
            {
                Debug.LogError("exportDir is null");
                return;
            }

            EditorUserBuildSettings.exportAsGoogleAndroidProject = true;
            //EditorUserBuildSettings.buildAppBundle = false;
            EditorUserBuildSettings.exportAsGoogleAndroidProject = true;
            UnityEditor.Build.Reporting.BuildReport buildReport = null;
            //if (args.Release == false)
            //{
            //    EditorUserBuildSettings.development = true;
            //    EditorUserBuildSettings.connectProfiler = true;
            //}
            var buildOptions = BuildOptions.None;
            if (args.Release == false)
                buildOptions = BuildOptions.Development /*| BuildOptions.ConnectWithProfiler*/;
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions()
            {
                scenes = new string[] { "Assets/Scenes/MainScene.unity" },
                locationPathName = exportDir,
                options = buildOptions,
                target = BuildTarget.Android,
                targetGroup = BuildTargetGroup.Android,
            };
            // Debug.Log("args.FullRes = " + args.FullRes);
            // SetProfile(args.FullRes ? "FullResProfile" : "NotFullResProfile");

            var buildAction = new Action(() =>
            {
                buildReport = BuildPipeline.BuildPlayer(buildPlayerOptions);
                Debug.Log("exportDir = " + exportDir);
                //buildReport = BuildPipeline.BuildPlayer(BuildUtils.GetBuildScenes(), exportDir, BuildTarget.Android, buildOptions);
            });
            var packAction = new Action(() =>
            {
                //AddressableAssetSettings.CleanPlayerContent();
#if !TEST_BUNDLE
                AssetDatabase.Refresh();
                BuildYooAsset(bundleVersionInt);
#endif
                //HotFixBuilder.AndroidLocalToRemote(true);
            });
            HotFixBuilder.BuildAndroid64(buildAction, packAction);
        }

        static void ExportXcodeProject()
        {
#if !TEST_BUNDLE
            int bundleVersionInt = CreateBundleVersionInt();
            HotFixBuilder.SaveBundleCreateTime(bundleVersionInt);
#endif

            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.iOS, BuildTarget.iOS);

            BuildArgs args = new BuildArgs();
            args.Init();
            args.TargetPlatform = "ios";
            Debug.Log("Babu Export Args: " + args.ToString());

            string exportDir = args.ExportDir;

            if (exportDir == null)
            {
                Debug.LogError("exportDir is null");
                return;
            }
            HotFixBuilder.DeleteDirectory(exportDir, true);

#if !TEST_BUNDLE
            PlayerSettings.bundleVersion = BuildArgs.Instance.MajorVersion;
            PlayerSettings.iOS.buildNumber = bundleVersionInt.ToString();
            PlayerSettings.iOS.appleEnableAutomaticSigning = true;
#endif

            PlayerSettings.productName = "CBA全力以赴";
            PlayerSettings.applicationIdentifier = "com.cbaqlyf.ximi.bigbang";

            EditorUserBuildSettings.symlinkSources = true;
            EditorUserBuildSettings.androidCreateSymbols = AndroidCreateSymbols.Public;
            if (args.Release == false)
            {
                PlayerSettings.iOS.appleEnableAutomaticSigning = false;
                EditorUserBuildSettings.development = true;
                EditorUserBuildSettings.connectProfiler = true;
                EditorUserBuildSettings.allowDebugging = true;
                EditorUserBuildSettings.buildWithDeepProfilingSupport = true;

                PlayerSettings.iOS.appleDeveloperTeamID = "DU67BP698S";
                PlayerSettings.iOS.iOSManualProvisioningProfileID = "62614ce3-1313-4819-9d82-9ee0cf5b84f7";
            }
            else
            {
                PlayerSettings.iOS.appleEnableAutomaticSigning = false;
                EditorUserBuildSettings.development = false;
                EditorUserBuildSettings.connectProfiler = false;
                EditorUserBuildSettings.allowDebugging = false;
                EditorUserBuildSettings.buildWithDeepProfilingSupport = false;

                PlayerSettings.iOS.appleDeveloperTeamID = "DU67BP698S";
                PlayerSettings.iOS.iOSManualProvisioningProfileID = "dbdf0ced-9762-4be3-b266-210a1f52e5a2";
            }
            PlayerSettings.iOS.targetDevice = iOSTargetDevice.iPhoneOnly;
            PlayerSettings.iOS.deferSystemGesturesMode = UnityEngine.iOS.SystemGestureDeferMode.All;
            PlayerSettings.iOS.hideHomeButton = true;
            var buildOptions = BuildOptions.None;
            if (args.Release == false)
                buildOptions = BuildOptions.Development | BuildOptions.ConnectWithProfiler;
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions()
            {
                scenes = new string[] { "Assets/Scenes/MainScene.unity" },
                locationPathName = exportDir,
                options = buildOptions,
                target = BuildTarget.iOS,
                targetGroup = BuildTargetGroup.iOS,
            };
            var buildAction = new Action(() =>
            {
                BuildPipeline.BuildPlayer(buildPlayerOptions);
            });
            var packAction = new Action(() =>
            {
                AssetDatabase.Refresh();
#if !TEST_BUNDLE
                BuildYooAsset(bundleVersionInt);
#endif
            });
            HotFixBuilder.BuildAndroid64(buildAction, packAction);
        }

        // static void SetProfile(string profile)
        // {
        //     //string profileId = AddressableAssetSettingsDefaultObject.Settings.profileSettings.GetProfileId(profile);
        //     //if (String.IsNullOrEmpty(profileId))
        //     //    Debug.LogWarning($"Couldn't find a profile named, {profile}, " +
        //     //                     $"using current profile instead.");
        //     //else
        //     //{
        //     //    AddressableAssetSettingsDefaultObject.Settings.activeProfileId = profileId;
        //     //    Debug.Log("set active profile = " + profile);
        //     //}
        // }

        public static int CreateBundleVersionInt()
        {
            return (DateTime.Now.Year - 2023) * 100000000 + int.Parse(DateTime.Now.ToString("MMddHHmm"));
        }

        public static void BuildYooAsset(int bundleVersionInt)
        {
            // 构建参数
            string defaultOutputRoot = AssetBundleBuilderHelper.GetDefaultOutputRoot();
            BuildParameters buildParameters = new BuildParameters();
            buildParameters.OutputRoot = defaultOutputRoot;
            buildParameters.BuildTarget = EditorUserBuildSettings.activeBuildTarget;
            buildParameters.BuildPipeline = EBuildPipeline.BuiltinBuildPipeline;
            buildParameters.BuildMode = EBuildMode.ForceRebuild;
            buildParameters.BuildVersion = bundleVersionInt;
            buildParameters.BuildinTags = "buildin";
            buildParameters.VerifyBuildingResult = true;
            buildParameters.EnableAddressable = false;
            buildParameters.CopyBuildinTagFiles = true;
            buildParameters.EncryptionServices = null;
            buildParameters.CompressOption = ECompressOption.LZ4;
            buildParameters.OutputNameStyle = EOutputNameStyle.BundleName_HashName_Extension;

            // 执行构建
            Debug.Log("=============开始构建AB包==============");
            AssetBundleBuilder builder = new AssetBundleBuilder();
            var buildResult = builder.Run(buildParameters);
            if (buildResult.Success)
            {
                Debug.Log($"构建成功 : {buildResult.OutputPackageDirectory}");
            }
            else
            {
                Debug.Log("构建失败");
                throw new Exception("build bundle failed!");
            }
        }

        /// <summary>
        /// python3脚本调用这个上传
        /// </summary>
        public static void UploadByPython3()
        {
            string folderName = BuildArgs.GetArgsValue("folderName", "");
            string filePath = BuildArgs.GetArgsValue("filePath", "");
            if (string.IsNullOrEmpty(filePath))
            {
                Debug.LogError("filePath is null");
                throw new Exception("filePath is null");
            }
            HotFixBuilder.UploadToCBAFileServer(folderName, filePath);
        }
    }
}
