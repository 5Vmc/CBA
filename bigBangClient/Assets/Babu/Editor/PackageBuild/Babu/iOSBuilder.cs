#if UNITY_IOS
using Babu.Globalization.Editor;
using LightJson;
using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEngine;

namespace Babu.Editor.Build.Babu
{
    class iOSBuilder : IPreprocessBuildWithReport
    {
        public int callbackOrder => BuildOrder.PLATFROM_BUILD;

        public void OnPreprocessBuild(BuildReport report)
        {
            // BuildUtils.Build(() =>
            // {
            //     JsonValue config = BuildUtils.GetChannelConfig(BuildConfigBuilder.Instance.Config["ios"], BuildArgs.Instance.ChannelId);

            //     PlayerSettings.bundleVersion = BuildArgs.Instance.MajorVersion;
            //     PlayerSettings.iOS.buildNumber = DateTime.Now.ToString("MMddHHmm");
            //     PlayerSettings.iOS.appleEnableAutomaticSigning = true;
            //     // PlayerSettings.iOS.appleEnableAutomaticSigning = false;
            //     // PlayerSettings.iOS.appleDeveloperTeamID = config["team_id"].AsString;
            //     // if (BuildArgs.Instance.Release)
            //     // {
            //     //     PlayerSettings.iOS.iOSManualProvisioningProfileID = config["release_profile_id"].AsString;
            //     //     PlayerSettings.iOS.iOSManualProvisioningProfileType = ProvisioningProfileType.Distribution;
            //     // }
            //     // else
            //     // {
            //     //     PlayerSettings.iOS.iOSManualProvisioningProfileID = config["debug_profile_id"].AsString;
            //     //     PlayerSettings.iOS.iOSManualProvisioningProfileType = ProvisioningProfileType.Development;
            //     // }

            //     // 常用设置
            //     PlayerSettings.iOS.targetDevice = iOSTargetDevice.iPhoneOnly;
            //     PlayerSettings.iOS.deferSystemGesturesMode = UnityEngine.iOS.SystemGestureDeferMode.All;
            //     PlayerSettings.iOS.hideHomeButton = true;

            //     // PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, new string[] { "TD_GAME", "USER_DEBUG" });
            //     PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, BuildArgs.Instance.Defines.ToArray());
            // });
        }

        [PostProcessBuild(BuildOrder.PROCESS_XCODE_PROJECT)]
        private static void OnPostProcessBuild(BuildTarget target, string pathToBuiltProject)
        {
            BuildUtils.Build(() =>
            {
                var projPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);
                var proj = new PBXProject();
                proj.ReadFromString(File.ReadAllText(projPath));

                var targetGUID = proj.GetUnityFrameworkTargetGuid();//UnityFramework的UUID
                proj.SetBuildProperty(targetGUID, "ENABLE_BITCODE", "NO");

                // 增加依赖的ios库
                proj.AddFrameworkToProject(targetGUID, "AdServices.framework", false);
                proj.AddFrameworkToProject(targetGUID, "iAd.framework", false);
                proj.AddFrameworkToProject(targetGUID, "StoreKit.framework", false);
                proj.AddFrameworkToProject(targetGUID, "AppTrackingTransparency.framework", false);
                proj.AddFrameworkToProject(targetGUID, "AdSupport.framework", false);
                proj.AddFrameworkToProject(targetGUID, "CoreTelephony.framework", false);
                proj.AddFrameworkToProject(targetGUID, "Security.framework", false);
                proj.AddFrameworkToProject(targetGUID, "SystemConfiguration.framework", false);
                proj.AddFrameworkToProject(targetGUID, "JavaScriptCore.framework", false);
                proj.AddFrameworkToProject(targetGUID, "WebKit.framework", false);
                proj.AddFrameworkToProject(targetGUID, "CoreMotion.framework", false);
                proj.AddFrameworkToProject(targetGUID, "AVFoundation.framework", false);
                proj.AddFrameworkToProject(targetGUID, "CFNetwork.framework", false);

                proj.AddFileToBuild(targetGUID, proj.AddFile("usr/lib/libz.tbd", "Frameworks/libz.tbd", PBXSourceTree.Sdk));
                proj.AddFileToBuild(targetGUID, proj.AddFile("usr/lib/libc++.tbd", "Frameworks/libc++.tbd", PBXSourceTree.Sdk));
                proj.AddFileToBuild(targetGUID, proj.AddFile("usr/lib/libresolv.9.tbd", "Frameworks/libresolv.9.tbd", PBXSourceTree.Sdk));
                proj.AddFileToBuild(targetGUID, proj.AddFile("usr/lib/libresolv.tbd", "Frameworks/libresolv.tbd", PBXSourceTree.Sdk));
                proj.AddFileToBuild(targetGUID, proj.AddFile("usr/lib/libsqlite3.tbd", "Frameworks/libsqlite3.tbd", PBXSourceTree.Sdk));

                var targetGUIDiphone = proj.GetUnityMainTargetGuid();//Unity-iPhone的UUID

                // 这种方式没用了！
                // proj.AddCapability(targetGUIDiphone, PBXCapabilityType.AccessWiFiInformation, null, false);
                // proj.AddCapability(targetGUIDiphone, PBXCapabilityType.InAppPurchase, null, false);

                proj.AddBuildProperty(targetGUIDiphone, "OTHER_LDFLAGS", "-ObjC");
                proj.AddBuildProperty(targetGUIDiphone, "PRODUCT_NAME_APP", "CBAFIGHT");
                proj.SetBuildProperty(targetGUIDiphone, "ENABLE_BITCODE", "NO");

                //根据Data目录相对路径获取UUID
                string resGUID = proj.FindFileGuidByProjectPath("Frameworks/Plugins/iOS/QuickGame/jywlRes.bundle");
                //移除jywlRes.bundle的UnityFramework的 TargetMemberShip
                proj.RemoveFileFromBuild(targetGUID, resGUID);
                //获取UnityFramework下的PBXResourcesBuildPhase Section UUID
                string resourceTarget = proj.GetResourcesBuildPhaseByTarget(targetGUIDiphone);
                //添加UUID进file目录，jywlRes.bundle的Unity-iPhone的 TargetMemberShip
                proj.AddFileToBuildSection(targetGUIDiphone, resourceTarget, resGUID);

                File.WriteAllText(projPath, proj.WriteToString());

                string projectPathiPhone = pathToBuiltProject + "/Unity-iPhone.xcodeproj/project.pbxproj";
                ProjectCapabilityManager projectCapabilityManager = new ProjectCapabilityManager(projectPathiPhone, "Unity-iPhone.entitlements", "Unity-iPhone");
                projectCapabilityManager.AddAccessWiFiInformation();
                projectCapabilityManager.AddInAppPurchase();
                projectCapabilityManager.AddSignInWithApple();
                projectCapabilityManager.WriteToFile();

                // 增加info.plist文件内容
                var plistPath = Path.Combine(pathToBuiltProject, "Info.plist");
                var plist = new PlistDocument();
                plist.ReadFromFile(plistPath);
                plist.root.SetString("NSUserTrackingUsageDescription", "开启权限不会获取你在其他站点的隐私信息，仅用于标识设备并保障服务安全与提升浏览体验");
                plist.root.SetBoolean("ITSAppUsesNonExemptEncryption", false);
                plist.root.SetString("NSMotionUsageDescription", "App需要您的同意，才能访问运动与健身");

                //游客保存账号密码到ios图库
                plist.root.SetString("NSPhotoLibraryAddUsageDescription", "为了便于您记忆账号和密码，需保存账号密码至悠的相册");
                plist.root.SetString("NSPhotoLibraryUsageDescription", "为了便于您记忆账号和密码，需保存账号密码至悠的相册");

                plist.WriteToFile(plistPath);

                // 多语言info.plist
                PlatformProjectLocale.AddLocalizedStringsIOS(pathToBuiltProject, Path.Combine(Application.dataPath, "Babu/BuildConfig/Globalization/iOS"));

                ChangeFile1(pathToBuiltProject);
                //ChangeFile2(pathToBuiltProject);
                //ChangeFile3(pathToBuiltProject);
            });


        }

        private static void ChangeFile1(string pathToBuiltProject)
        {
            // 修改main.mm文件 / PBXProject版本过旧，更新Unity可能会解决这个问题
            string mainAppPath = Path.Combine(pathToBuiltProject, "MainApp", "main.mm");
            string mainContent = File.ReadAllText(mainAppPath);
            string newContent = mainContent.Replace("#include <UnityFramework/UnityFramework.h>", @"#include ""../UnityFramework/UnityFramework.h""");
            File.WriteAllText(mainAppPath, newContent);
        }
        private static void ChangeFile2(string pathToBuiltProject)//已不使用 QuickSDK
        {
            // 添加QuickSDK的头文件
            string mainAppPath = Path.Combine(pathToBuiltProject, "Classes", "UnityAppController.mm");
            string mainContent = File.ReadAllText(mainAppPath);
            string newContent = mainContent.Replace("#include <mach/mach_time.h>", "\n#include <mach/mach_time.h>\n#import <SMPCQuickSDK/SMPCQuickSDK.h>\n#import \"QuickSDK_ios.h\"");
            File.WriteAllText(mainAppPath, newContent);
        }
        private static void ChangeFile3(string pathToBuiltProject)//已不使用 QuickSDK
        {
            // 添加QuickSDK的初始化代码
            string mainAppPath = Path.Combine(pathToBuiltProject, "Classes", "UnityAppController.mm");
            string mainContent = File.ReadAllText(mainAppPath);
            string newContent = mainContent.Replace("[KeyboardDelegate Initialize];", @"
    [KeyboardDelegate Initialize];
    
    //注册事件监听QuickSDK
    [[QuickSDK_ios shareInstance] addNotifications];
    //初始化QuickSDK
    SMPCQuickSDKInitConfigure *cfg = [[SMPCQuickSDKInitConfigure alloc] init];
    cfg.productKey = @""50580605"";
    cfg.productCode = @""09703011423078907545949266524525"";
    int error = [[SMPCQuickSDK defaultInstance] initWithConfig: cfg application:application didFinishLaunchingWithOptions:launchOptions];
    if (error != 0)
    {
        NSLog(@""不能启动初始化QuickSDK：%d"", error);
    }
            ");
            File.WriteAllText(mainAppPath, newContent);
        }

        internal static void CopyAndReplaceDirectory(string srcPath, string dstPath)
        {
            //路径下该文件夹若存在，则删除
            if (Directory.Exists(dstPath))
            {
                Directory.Delete(dstPath);
            }
            //路径下的文件若存在，则删除
            if (File.Exists(dstPath))
            {
                File.Delete(dstPath);
            }
            //创建该路径下文件夹
            Directory.CreateDirectory(dstPath);
            Debug.Log(dstPath + "----" + srcPath);

            foreach (var file in Directory.GetFiles(srcPath))
            {
                Debug.Log(Path.Combine(dstPath, Path.GetFileName(file)));
                File.Copy(file, Path.Combine(dstPath, Path.GetFileName(file)));
            }


            foreach (var dir in Directory.GetDirectories(srcPath))
                CopyAndReplaceDirectory(dir, Path.Combine(dstPath, Path.GetFileName(dir)));
        }
    }
}
#endif