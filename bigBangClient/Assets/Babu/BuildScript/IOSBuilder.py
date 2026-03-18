# coding=utf-8

import os
import shutil
import json
import subprocess
import time
from Utils import log, sendDingMsg, hasChinese, sendFeiShuLinkMsg
from Builder import Builder

def getGitCommitId(gitPath,branch):
    # os.system("cd {0}; git checkout -b {1}".format(gitPath, branch))
    commitId = subprocess.getstatusoutput("cd {0}; git rev-parse --short HEAD".format(gitPath))
    print(commitId)
    return commitId[1]

class IOSBuilder(Builder):
    def __init__(self, build_args, build_config):
        Builder.__init__(self, build_args, build_config)

    def build(self):
        log('================begin build ios===============')

        self.build_args.export_dir = self.build_args.unity_project_root_path + "/../iosExport/"
        self.build_args.apk_dir = self.build_args.unity_project_root_path + "/../bigBangOut/"

        print("获取 git 版本号\n")
        git_version = getGitCommitId(self.build_args.unity_project_root_path+"/../","master")
        print("git 版本号为："+git_version)

        print("设置宏定义\n")
        self.setDefines("setDefines")

        print("让Unity导出Xcode工程\n")
        self.exportXcodeProject()

        print("编译HybridCLR修改后的libil2cpp.a文件(sh文件可能需要运行权限： chmod +x /HybridCLRData/iOSBuild/build_libil2cpp.sh)\n")
        os.chdir(self.build_args.unity_project_root_path + "/HybridCLRData/iOSBuild/")
        os.system("chmod +x ./build_libil2cpp.sh")
        os.system("./build_libil2cpp.sh")
        print("替换xcode工程的libil2cpp.a文件\n")
        shutil.copy(self.build_args.unity_project_root_path + "/HybridCLRData/iOSBuild/build/libil2cpp.a", self.build_args.export_dir + "Libraries/")
        os.chdir(self.build_args.unity_project_root_path)

        print("读取Environment.json中的客户端创建时间\n")
        environmentJsonPathAndName = self.build_args.unity_project_root_path + \
            "/Assets/Resources/Environment.json"
        clientCreatTime = ""
        with open(environmentJsonPathAndName, "r", encoding="utf-8") as f:
            content = json.load(f)
            clientCreatTime = content.get('client_creat_time')

        print("拼接生成IPA名字\n")
        ipaNewName = "ios-" + self.build_args.project_name + "-" + self.build_args.defines + \
            "-isFullRes=" + str(self.build_args.full_res) + \
            "-" + clientCreatTime + "-" + git_version

        print("在新位置创建名字和ipa一样的文件夹\n")
        newDirPath = self.build_args.apk_dir + ipaNewName + "/"
        os.mkdir(newDirPath)

        print("将打出的Xcode工程拷贝到新的文件夹\n")
        shutil.copytree(self.build_args.export_dir,
                        newDirPath+"XcodeProject-" + ipaNewName)

        print("将Environment.json拷贝到新的文件夹\n")
        shutil.copy(environmentJsonPathAndName, newDirPath)

        if self.build_args.full_res == False:
            print("读取BundleVersion.txt\n")
            bundleVersion = ""
            bundleVersionFilePathAndName = self.build_args.unity_project_root_path + \
                "/Assets/LocalAsset/Texts/BundleVersion.txt"
            with open(bundleVersionFilePathAndName, encoding='utf-8') as file:
                content = file.read()
                bundleVersion = content.rstrip()

            print("将资源拷贝到 用资源版本号为名的文件夹\n")
            oldBundleForderPath = self.build_args.unity_project_root_path + \
                "/Bundles/iOS/" + bundleVersion
            newBundleForderPath = newDirPath + bundleVersion
            shutil.copytree(oldBundleForderPath, newBundleForderPath)

            print("生成热更版本记录\n")
            # 打开文件
            f = open(newBundleForderPath + "/" +  ipaNewName + '.txt', 'w')
            # 写入文本
            f.write(ipaNewName)
            # 关闭文件
            f.close()

        time.sleep(1)

        print("使用正式热更新地址的包输出成功\n")

        # if self.build_args.release:
        #     print("开始生成用来测试热更新的包\n")
        #     self.setDefines("setDefinesTestBundle")
        #     self.exportXcodeProject()
        #     shutil.copy(self.build_args.unity_project_root_path + "/HybridCLRData/iOSBuild/build/libil2cpp.a", self.build_args.export_dir + "Libraries/")
        #     print("将打出的测试热更新Xcode工程拷贝到新的文件夹\n")
        #     shutil.copytree(self.build_args.export_dir,
        #                     newDirPath + "TestBundle-XcodeProject-" + ipaNewName)
        #     print("使用测试热更新地址的包输出成功\n")

        # print("资源整理完成，打开文件夹" + newDirPath + "\n")
        # os.system("open " + newDirPath)

        print("资源整理完成，开始压缩成zip，请稍候" + newDirPath + "\n")
        shutil.make_archive(self.build_args.apk_dir + ipaNewName, 'zip', newDirPath)
        shutil.rmtree(newDirPath)

        print("上传到文件服务器")
        zipPath = self.build_args.apk_dir + ipaNewName + ".zip"
        self.uploadFileToFileServerUseCsharp("uploadFile.log", "cba", zipPath)

        print("发送飞书通知" + newDirPath + "\n")
        feishu_title = "【打包结果】{} {} 打包成功".format(self.build_args.target_platform, self.build_args.env)
        feishu_text = "文件名：" + ipaNewName + ".zip"
        file_server_url = self.build_args.package_upload_url + ipaNewName + ".zip";
        feishu_link_text = "点击下载";
        sendFeiShuLinkMsg(self.build_args.feishu_url, feishu_title,feishu_text,file_server_url,feishu_link_text);

    def removeXCodeDir(self):
        if os.path.exists('xcode'):
            shutil.rmtree('xcode')

    def buildXCode(self):
        log('================begin build xcode===============')
        os.chdir('xcode')
        os.system(
            'security unlock-keychain -p babu21go /Users/babuyo/Library/Keychains/login.keychain')
        # 是否打包出的是workspace项目
        if os.path.exists("Unity-iPhone.xcworkspace"):
            if self.build_args.release:
                cmd = "xcodebuild -workspace Unity-iPhone.xcworkspace archive -scheme Unity-iPhone -configuration ReleaseForRunning -archivePath archive/archive -quiet"
            else:
                cmd = "xcodebuild -workspace Unity-iPhone.xcworkspace -scheme Unity-iPhone -configuration ReleaseForRunning"
        else:
            if self.build_args.release:
                cmd = "xcodebuild archive -scheme Unity-iPhone -configuration ReleaseForRunning -archivePath archive/archive -quiet"
            else:
                cmd = "xcodebuild -scheme Unity-iPhone -configuration ReleaseForRunning"
        if os.system(cmd) != 0:
            log('build xcode failed')
            raise Exception()
        os.chdir('..')
        log('================build xcode succ===============')

    def export(self):
        log('================begin export===============')
        os.chdir('xcode')
        cmd = "xcodebuild -exportArchive -archivePath archive/archive.xcarchive -exportPath export  PROVISIONING_PROFILE_SPECIFIER profilename -quiet -exportOptionsPlist ../" \
            + self.build_config.getConfig("ios.export_option_plist_path")
        if os.system(cmd) != 0:
            log('export package failed')
            raise Exception()
        os.chdir('..')
        log('================export succ===============')

    def upload(self):
        log('================begin upload===============')
        os.chdir('xcode')
        project_name = self.build_config.getConfig("project_name")
        if hasChinese(project_name):
            project_name = 'CBAFIGHT'
        else:
            project_name = project_name.replace(' ', '')
        cmd = "xcrun altool --upload-app -f export/" + project_name + ".ipa -t ios --apiKey " + self.build_config.getConfig("ios.api_key") \
            + " --apiIssuer " + \
            self.build_config.getConfig(
                "ios.api_issuer") + " --verbose --output-format xml"
        if os.system(cmd) != 0:
            log('upload package failed')
            raise Exception()
        os.chdir('..')
        log('================upload succ===============')

    '''定义发行渠道'''

    def updateDisChannel(self):
        channelFile = self.build_args.unity_project_root_path + \
            "/" + "Assets/Scripts/GameConst/DisChannel.cs"
        renameFile = self.build_args.unity_project_root_path + \
            "/" + "Assets/Scripts/GameConst/DisChannel.cs.bak"

        shutil.copy(channelFile, renameFile)
        channleName = "AppStore"

        # Read in the file
        filedata = ""
        with open(channelFile, 'r') as file:
            filedata = file.read()

        # Replace the target string
        filedata = filedata.replace('\{CHANNEL\}', channleName)

        # Write the file out again
        with open(channelFile, 'w') as file:
            file.write(filedata)

    def recoverChannelFile(self):
        channelFile = self.build_args.unity_project_root_path + \
            "/" + "Assets/Scripts/GameConst/DisChannel.cs"
        renameFile = self.build_args.unity_project_root_path + \
            "/" + "Assets/Scripts/GameConst/DisChannel.cs.bak"
        os.rename(renameFile, channelFile)

    # def sendSuccDingMsg(self):
    #     ding_msg = "【打包结果】{} {} {}打包成功".format(
    #         self.build_args.target_platform, self.build_args.env, self.build_args.channel_id)
    #     if self.build_args.release:
    #         ding_msg = ding_msg + "，并已上传到App Store"
    #     else:
    #         ding_msg = ding_msg + "，可以连接手机测试"
    #     sendDingMsg(self.build_args.feishu_url, ding_msg)
