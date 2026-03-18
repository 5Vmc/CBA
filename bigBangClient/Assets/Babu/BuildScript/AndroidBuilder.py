# coding=utf-8

from operator import index
import os
import shutil
import subprocess
import time
import re
import json
from BuildArgs import BuildArgs
from Utils import log, sendDingMsg, sendFeiShuLinkMsg
from Builder import Builder

def getGitCommitId(gitPath,branch):
    # os.system("cd {0}; git checkout -b {1}".format(gitPath, branch))
    commitId = subprocess.getstatusoutput("cd {0}; git rev-parse --short HEAD".format(gitPath))
    print(commitId)
    return commitId[1]

class AndroidBuilder(Builder):
    def __init__(self, build_args: BuildArgs, build_config):
        Builder.__init__(self, build_args, build_config)

    def build(self):

        self.build_args.export_dir = self.build_args.unity_project_root_path + "/../androidExport/"
        self.build_args.apk_dir = self.build_args.unity_project_root_path + "/../bigBangOut/"
        buildAssetsDir = self.build_args.unity_project_root_path + "/BuildAssets/"
        buildAssetsTargetPath = self.build_args.unity_project_root_path + "/Assets/Plugins/Android/"
        buildForderLibPath = self.build_args.export_dir + "unityLibrary/libs/"
        quickBuildAssetsName = "quick_module-release.aar"
        dachenBuildAssetsName = "dachen_module-release.aar"
        if os.path.exists(buildForderLibPath):
            shutil.rmtree(buildForderLibPath)
        if os.path.exists(buildAssetsTargetPath + quickBuildAssetsName):
            os.remove(buildAssetsTargetPath + quickBuildAssetsName)
        if os.path.exists(buildAssetsTargetPath + dachenBuildAssetsName):
            os.remove(buildAssetsTargetPath + dachenBuildAssetsName)
        shutil.copy(buildAssetsDir + quickBuildAssetsName , buildAssetsTargetPath + quickBuildAssetsName)
        unitySrcAssetsdir = self.build_args.export_dir + "unityLibrary/src/main/assets/"
        if os.path.exists(unitySrcAssetsdir):
            shutil.rmtree(unitySrcAssetsdir)
        unityLibraryAssetsdir = self.build_args.export_dir + "unityLibrary/build/intermediates/"
        if os.path.exists(unityLibraryAssetsdir):
            shutil.rmtree(unityLibraryAssetsdir)
        launcherLibraryAssetsdir = self.build_args.export_dir + "launcher/build/intermediates/"
        if os.path.exists(launcherLibraryAssetsdir):
            shutil.rmtree(launcherLibraryAssetsdir)
        self.setDefines("setDefines")
        self.exportAs()
        # ret = self.buildFromAs()
        # if ret == 0:

        #     print("获取 git 版本号\n")
        #     git_version = getGitCommitId(self.build_args.unity_project_root_path+"/../","master")
        #     print("git 版本号为："+git_version)

        #     print("读取Environment.json中的客户端创建时间\n")
        #     environmentJsonPathAndName = self.build_args.unity_project_root_path + \
        #         "/Assets/Resources/Environment.json"
        #     clientCreatTime = ""
        #     with open(environmentJsonPathAndName, "r", encoding="utf-8") as f:
        #         content = json.load(f)
        #         clientCreatTime = content.get('client_creat_time')

        #     print("拼接生成APK名字\n")
        #     apkNewEnd = ".apk"
        #     apkNewName = "android-miguplay-" + self.build_args.project_name + "-" + self.build_args.defines + \
        #         "-isFullRes=" + str(self.build_args.full_res) + \
        #         "-" + clientCreatTime + "-" + git_version

        #     print("在新位置创建名字和apk一样的文件夹\n")
        #     if os.path.exists(self.build_args.apk_dir):
        #         shutil.rmtree(self.build_args.apk_dir)
        #     os.mkdir(self.build_args.apk_dir)
        #     newDirPath = self.build_args.apk_dir + apkNewName + "/"
        #     apkFullPathAndNameQuickBundle = newDirPath + "QuickSdkBundle-" + apkNewName + apkNewEnd
        #     os.mkdir(newDirPath)

        #     print("将生成出的apk拷贝到新的文件夹\n")
        #     apkPathAndNameInExportDir = ""
        #     if self.build_args.release:
        #         apkPathAndNameInExportDir = self.build_args.export_dir + \
        #             "/launcher/build/outputs/apk/release/launcher-release.apk"
        #     else:
        #         apkPathAndNameInExportDir = self.build_args.export_dir + \
        #             "/launcher/build/outputs/apk/debug/launcher-debug.apk"
        #     shutil.copy(apkPathAndNameInExportDir, apkFullPathAndNameQuickBundle)

        #     print("将生成出的符号表，压缩成zip，并拷贝到新的文件夹\n")
        #     apkSymbolsDir = self.build_args.export_dir + "/unityLibrary/symbols"
        #     shutil.make_archive(newDirPath + "Symbols-" + apkNewName , 'zip', apkSymbolsDir)

        #     print("将Environment.json拷贝到新的文件夹\n")
        #     shutil.copy(environmentJsonPathAndName, newDirPath)

        #     if self.build_args.full_res == False:
        #         print("读取BundleVersion.txt\n")
        #         bundleVersion = ""
        #         bundleVersionFilePathAndName = self.build_args.unity_project_root_path + \
        #             "/Assets/LocalAsset/Texts/BundleVersion.txt"
        #         with open(bundleVersionFilePathAndName, encoding='utf-8') as file:
        #             content = file.read()
        #             bundleVersion = content.rstrip()

        #         print("将资源拷贝到 用资源版本号为名的文件夹\n")
        #         oldBundleForderPath = self.build_args.unity_project_root_path + \
        #             "/Bundles/Android/" + bundleVersion
        #         newBundleForderPath = newDirPath + bundleVersion
        #         shutil.copytree(oldBundleForderPath, newBundleForderPath)

        #         print("生成热更版本记录\n")
        #         # 打开文件
        #         f = open(newBundleForderPath + "/" + apkNewName + '.txt', 'w')
        #         # 写入文本
        #         f.write(apkNewName)
        #         # 关闭文件
        #         f.close()

        #     time.sleep(1)
        #     # #self.uploadToServer(renameApk)
        #     # #os.remove(renameApk)
        #     os.remove(apkPathAndNameInExportDir)

            # print("使用正式热更新地址的包输出成功\n")

            # if self.build_args.release:
            #     print("开始生成用来长尾大臣的包\n")
            #     if os.path.exists(buildForderLibPath):
            #         shutil.rmtree(buildForderLibPath)
            #     if os.path.exists(buildAssetsTargetPath + quickBuildAssetsName):
            #         os.remove(buildAssetsTargetPath + quickBuildAssetsName)
            #     if os.path.exists(buildAssetsTargetPath + dachenBuildAssetsName):
            #         os.remove(buildAssetsTargetPath + dachenBuildAssetsName)
            #     shutil.copy(buildAssetsDir + dachenBuildAssetsName , buildAssetsTargetPath + dachenBuildAssetsName)
            #     unitySrcAssetsdir = self.build_args.export_dir + "unityLibrary/src/main/assets/"
            #     if os.path.exists(unitySrcAssetsdir):
            #         shutil.rmtree(unitySrcAssetsdir)
            #     unityLibraryAssetsdir = self.build_args.export_dir + "unityLibrary/build/intermediates/"
            #     if os.path.exists(unityLibraryAssetsdir):
            #         shutil.rmtree(unityLibraryAssetsdir)
            #     launcherLibraryAssetsdir = self.build_args.export_dir + "launcher/build/intermediates/"
            #     if os.path.exists(launcherLibraryAssetsdir):
            #         shutil.rmtree(launcherLibraryAssetsdir)
            #     self.setDefines("setDefinesDaChenBundle")
            #     self.exportAs()
            #     ret = self.buildFromAs()
            #     if ret == 0:
            #         apkFullPathAndNameDaChenBundle = newDirPath + \
            #             "DaChenBundle-" + apkNewName + apkNewEnd
            #         shutil.copy(apkPathAndNameInExportDir,
            #                     apkFullPathAndNameDaChenBundle)
            #         time.sleep(1)
            #         os.remove(apkPathAndNameInExportDir)
            #         print("长尾大臣的包生成成功\n")
            #     else:
            #         print("长尾大臣的包生成失败\n")

            # if self.build_args.release:
            #     print("开始生成用来测试热更新的包\n")
            #     if os.path.exists(buildForderLibPath):
            #         shutil.rmtree(buildForderLibPath)
            #     if os.path.exists(buildAssetsTargetPath + quickBuildAssetsName):
            #         os.remove(buildAssetsTargetPath + quickBuildAssetsName)
            #     if os.path.exists(buildAssetsTargetPath + dachenBuildAssetsName):
            #         os.remove(buildAssetsTargetPath + dachenBuildAssetsName)
            #     shutil.copy(buildAssetsDir + quickBuildAssetsName , buildAssetsTargetPath + quickBuildAssetsName)
            #     unitySrcAssetsdir = self.build_args.export_dir + "unityLibrary/src/main/assets/"
            #     if os.path.exists(unitySrcAssetsdir):
            #         shutil.rmtree(unitySrcAssetsdir)
            #     unityLibraryAssetsdir = self.build_args.export_dir + "unityLibrary/build/intermediates/"
            #     if os.path.exists(unityLibraryAssetsdir):
            #         shutil.rmtree(unityLibraryAssetsdir)
            #     launcherLibraryAssetsdir = self.build_args.export_dir + "launcher/build/intermediates/"
            #     if os.path.exists(launcherLibraryAssetsdir):
            #         shutil.rmtree(launcherLibraryAssetsdir)
            #     self.setDefines("setDefinesTestBundle")
            #     self.exportAs()
            #     ret = self.buildFromAs()
            #     if ret == 0:
            #         apkFullPathAndNameTestBundle = newDirPath + \
            #             "TestBundle-" + apkNewName + apkNewEnd
            #         shutil.copy(apkPathAndNameInExportDir,
            #                     apkFullPathAndNameTestBundle)
            #         time.sleep(1)
            #         os.remove(apkPathAndNameInExportDir)
            #         print("用来测试热更新的包生成成功\n")
            #     else:
            #         print("用来测试热更新的包生成失败\n")

            # print("资源整理完成，打开文件夹" + newDirPath + "\n")
            # os.system("open " + newDirPath)

            # print("资源整理完成，开始压缩成zip，请稍候" + newDirPath + "\n")
            # shutil.make_archive(self.build_args.apk_dir + apkNewName, 'zip', newDirPath)
            # # shutil.rmtree(newDirPath)

            # print("上传到文件服务器")
            # # zipPath = self.build_args.apk_dir + apkNewName + ".zip"
            # # self.uploadFileToFileServerUseCsharp("uploadFile.log", "cba", zipPath)

            # print("发送飞书通知" + newDirPath + "\n")
            # # feishu_title = "【打包结果】{} {} 打包成功".format(self.build_args.target_platform, self.build_args.env)
            # # feishu_text = "文件名：" + apkNewName + ".zip"
            # # file_server_url = self.build_args.package_upload_url + apkNewName + ".zip";
            # # feishu_link_text = "点击下载";
            # # sendFeiShuLinkMsg(self.build_args.feishu_url, feishu_title,feishu_text,file_server_url,feishu_link_text);

    def buildFromAs(self):
        asPrj = self.build_args.export_dir
        launcherDir = asPrj + "/launcher"
        launcherGradle = launcherDir + "/build.gradle"
        manifestFile = launcherDir + "/src/main/AndroidManifest.xml"

        self.addNetWorkSecurity(manifestFile)

        ret = -1
        '''change version'''

        unityLibrary = asPrj + "/unityLibrary"
        manifestFile = unityLibrary + "/src/main/AndroidManifest.xml"

        self.fileReplace(manifestFile, "android:minSdkVersion=\"9\"", "")
        if self.build_args.release:
            os.chdir(asPrj)
            ret = os.system("./gradlew assembleRelease")
            ret = os.system("./gradlew assembleDebug")
        else:
            os.chdir(asPrj)
            ret = os.system("./gradlew assembleDebug")

        if ret != 0:
            print("\n====================build apk failed=========================\n")
            raise Exception()
        else:
            print("\n")
            print(asPrj + "/launcher/build/outputs/apk")
            print(
                "\n====================build apk success!!!!!=========================\n")

        return ret

    # android:networkSecurityConfig="@xml/network_security_config"
    def addNetWorkSecurity(self, file):
        file_data = ""
        with open(file, "r", encoding="utf-8") as f:
            for line in f:
                matchObj = re.match(
                    r'.*(\<application).*(\>)', line, re.M | re.I)
                if matchObj and "network_security_config" not in line:
                    # print("matchObj.group(1) : ", matchObj.group(1))
                    # print("matchObj.group(2) : ", matchObj.group(2))
                    line = line.replace(
                        ">", " android:networkSecurityConfig=\"@xml/network_security_config\" >")
                    print(line)
                    file_data += line
                else:
                    file_data += line
        with open(file, "w", encoding="utf-8") as f:
            f.write(file_data)

    def fileReplace(self, file, old_str, new_str):
        file_data = ""
        with open(file, "r", encoding="utf-8") as f:
            for line in f:
                if old_str in line:
                    line = line.replace(old_str, new_str)
                file_data += line
        with open(file, "w", encoding="utf-8") as f:
            f.write(file_data)

    def protected(self):
        log('============begin protected=============')
        os.chdir("Assets/Babu/BuildScript/Protected/")
        cmd = "python client.py -u 1105e7e5a3236125 -i " + self.build_args.unity_project_root_path + "/" + self.build_args.project_name + ".apk" + \
            ' -o ' + self.build_args.unity_project_root_path + "/" + \
            self.build_args.project_name + ".protected.apk -p"
        if os.system(cmd) != 0:
            log("protected failed")
            raise Exception()
        os.chdir(self.build_args.unity_project_root_path)
        os.remove(self.build_args.project_name + ".apk")
        shutil.move(self.build_args.project_name + ".protected.apk",
                    self.build_args.project_name + ".apk")
        log('============protected succ!=============')

    # 重新签名
    def sign(self):
        log('============begin sign===============')
        cmd = "jarsigner -verbose -keystore bigbang.keystore -storepass bigbang21go -signedjar " + \
            self.build_args.project_name + ".signed.apk -digestalg SHA1 -sigalg MD5withRSA " + \
            self.build_args.project_name + ".apk bigbang"
        if os.system(cmd) != 0:
            log("signed failed")
            raise Exception()
        os.remove(self.build_args.project_name + ".apk")
        shutil.move(self.build_args.project_name + ".signed.apk",
                    self.build_args.project_name + ".apk")
        log('================sign succ!================')

    def removePackage(self):
        os.remove(self.build_args.project_name + ".apk")

    # def sendSuccDingMsg(self, download_url):
    #     ding_msg = "【打包结果】{} {} {}打包成功，下载地址：{}".format(
    #         self.build_args.target_platform, self.build_args.env, self.build_args.channel_id, download_url)
    #     sendDingMsg(self.build_args.feishu_url, ding_msg)
