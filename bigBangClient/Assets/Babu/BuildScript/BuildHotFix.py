# coding=utf-8

import subprocess
import requests
from Builder import UnityOutputLogThread
from Utils import logFail, logSucc
from Utils import log, sendFeiShuLinkMsg, sendFeiShuMsg
import sys
import sys
import os
import getopt
import platform
import json
import time
import shutil

def getGitCommitId(gitPath,branch):
    # os.system("cd {0}; git checkout -b {1}".format(gitPath, branch))
    commitId = subprocess.getstatusoutput("cd {0}; git rev-parse --short HEAD".format(gitPath))
    print(commitId)
    return commitId[1]

if __name__ == '__main__':

    # unity_path = "/Applications/Unity/Hub/Editor/2021.3.6f1c1/Unity.app/Contents/MacOS/Unity"
    unity_path = "/Applications/Unity/Unity.app/Contents/MacOS/Unity"
    log_path = "buildHotFix.log"
    debugSign = sys.argv[1]
    targetPlatform = sys.argv[2]
    projectPath = os.getcwd() + "/"
    resFolderPath = projectPath + "../bigBangOut/"
    feishu_url = "https://open.feishu.cn/open-apis/bot/v2/hook/a5906aef-0e1f-405b-a561-70ff11e4c279"
    file_server_path = "http://package.win.babuyo.com:8080/cba_hotfix/"

    try:
        print("\n==================== start change define =========================\n")

        print("设置宏定义\n")
        cmd = ""
        if str.lower(targetPlatform) == "ios":
            if str.lower(debugSign) == "debug":
                cmd = unity_path + \
                    " -batchmode " + \
                    " -projectPath " + projectPath + \
                    " -executeMethod " + "Babu.Editor.Build.Builder.SetDefinesiOSDebug" + \
                    " -logfile " + log_path + \
                    " -quit"
            else:
                cmd = unity_path + \
                    " -batchmode " + \
                    " -projectPath " + projectPath + \
                    " -executeMethod " + "Babu.Editor.Build.Builder.SetDefinesiOSRelease" + \
                    " -logfile " + log_path + \
                    " -quit"
        else:
            if str.lower(debugSign) == "debug":
                cmd = unity_path + \
                    " -batchmode " + \
                    " -projectPath " + projectPath + \
                    " -executeMethod " + "Babu.Editor.Build.Builder.SetDefinesAndroidDebug" + \
                    " -logfile " + log_path + \
                    " -quit"
            else:
                cmd = unity_path + \
                    " -batchmode " + \
                    " -projectPath " + projectPath + \
                    " -executeMethod " + "Babu.Editor.Build.Builder.SetDefinesAndroidRelease" + \
                    " -logfile " + log_path + \
                    " -quit"
        log(cmd)

        log('=============== start change define begin ! =============')
        log_thread = UnityOutputLogThread(log_path)
        log_thread.start()
        ret = os.system(cmd)

        time.sleep(10)
        log_thread.stop = True
        log_thread.join()
        if ret != 0:
            logFail('============== start change define failed ! ============')
            raise Exception()
        logSucc('============ start change define succ ! ============')



        print("打热更包\n")
        print("\n==================== start build hot fix =========================\n")

        cmd = unity_path + \
            " -batchmode " + \
            " -executeMethod " + "Babu.Editor.Build.Builder.BuildHotFix"
        if str.lower(targetPlatform) == "ios":
            cmd = cmd + " -target_platform ios"
        else:
            cmd = cmd + " -target_platform android"
        cmd = cmd + \
            " -logfile " + log_path + \
            " -quit"
        log(cmd)

        log('=============== build hot fix begin ! =============')
        log_thread = UnityOutputLogThread(log_path)
        log_thread.start()
        ret = os.system(cmd)

        time.sleep(10)
        log_thread.stop = True
        log_thread.join()
        if ret != 0:
            logFail('============== build hot fix failed ! ============')
            raise Exception()
        logSucc('============ build hot fix succ ! ============')

        

        print("读取BundleVersion.txt\n")
        bundleVersion = ""
        unity_project_root_path = os.getcwd()
        bundleVersionFilePathAndName = unity_project_root_path + \
            "/Assets/LocalAsset/Texts/BundleVersion.txt"
        with open(bundleVersionFilePathAndName, encoding='utf-8') as file:
            content = file.read()
            bundleVersion = content.rstrip()

        print("将资源拷贝到 用资源版本号为名的文件夹\n")
        oldBundleForderPath = unity_project_root_path
        if str.lower(targetPlatform) == "ios":
            oldBundleForderPath = oldBundleForderPath + "/Bundles/iOS/" + bundleVersion
        else:
            oldBundleForderPath = oldBundleForderPath + "/Bundles/Android/" + bundleVersion
            
        newDirName = ""
        if debugSign == "debug":
            newDirName = newDirName + "DEBUG"
        else:
            newDirName = newDirName + "RELEASE"
        if str.lower(targetPlatform) == "ios":
            newDirName = newDirName + "-IOS-"
        else:
            newDirName = newDirName + "-ANDROID-"
        newDirName = newDirName + bundleVersion + "-"

        print("获取 git 版本号\n")
        git_version = getGitCommitId(projectPath+"../","master")
        print("git 版本号为："+git_version)

        newDirName = newDirName + git_version

        newDirPathRoot = resFolderPath + newDirName 
        newDirPath = newDirPathRoot + "/" + bundleVersion + "/"
        shutil.copytree(oldBundleForderPath, newDirPath)

        print("生成热更版本记录\n")
        # 打开文件
        f = open(newDirPath + newDirName + '.txt', 'w')
        # 写入文本
        f.write(newDirName)
        # 关闭文件
        f.close()

        # print("资源整理完成，打开文件夹" + newDirPath + "\n")
        # # 这是mac指令
        # os.system("open " + newDirPath)

        print("资源整理完成，开始压缩成zip，请稍候" + newDirPathRoot + "\n")
        shutil.make_archive(newDirPathRoot, 'zip', newDirPathRoot + "/")
        shutil.rmtree(newDirPathRoot)

        print("上传到文件服务器")
        zipPath = newDirPathRoot + ".zip"
        file_path = zipPath
        print("\n==================== start upload " + file_path + " =========================\n")
        cmd = unity_path + \
            " -batchmode " + \
            " -executeMethod " + "Babu.Editor.Build.Builder.UploadByPython3" + \
            " -filePath " + file_path + \
            " -folderName " + "cba_hotfix" + \
            " -logfile " + log_path + \
            " -quit"
        log(cmd)

        log('=============== build upload begin ! =============')
        log_thread = UnityOutputLogThread(log_path)
        log_thread.start()
        ret = os.system(cmd)
        time.sleep(10)
        log_thread.stop = True
        log_thread.join()
        if ret != 0:
            logFail('============== build upload failed ! ============')
            raise Exception()
        logSucc('============ build upload succ ! ============')

        zipFileName = newDirName + ".zip"
        print("发送飞书通知" + newDirPathRoot + "\n")
        feishu_title = "【打热更包结果】{} {} 打热更新包成功".format(targetPlatform, debugSign)
        feishu_text = "文件名：" + zipFileName
        file_server_url = file_server_path + zipFileName;
        feishu_link_text = "点击下载";
        sendFeiShuLinkMsg(feishu_url, feishu_title,feishu_text,file_server_url,feishu_link_text);

        print("\n==================== build hot fix end =========================\n")
    except Exception as e:
        logFail(str(e))
        ding_msg = "【打包结果】{} {} 打热更新包失败".format(targetPlatform, debugSign)
        sendFeiShuMsg(feishu_url, ding_msg)


