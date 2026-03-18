# coding=utf-8

import sys
import os
import getopt
import platform
import json
import time
from Utils import log


class BuildArgs:
    def __init__(self):
        self.unity_version = '2021.3.6f1c1'      # unity 版本号
        self.project = 'unknown'                 # 游戏名字
        self.major_version = '0.0.0'             # 主版本号
        self.env = 'unknown'                     # 打包环境（develop or online）
        self.target_platform = 'unknown'         # 目标平台（ios or android or google_play）
        self.package_upload_url = ''             # 打包上传地址
        self.feishu_url = ''                     # 飞书通知url
        self.defines = ''                        # 自定义宏
        self.channel_id = 'unknown'              # 渠道id
        self.remove_package = True               # 是否删除打包的文件
        self.public_package = False              # 包是否发布到外网
        self.commit_id = '0'                      # build的主分支最新commit号

        self.minor_version = ''                  # 子版本号，时间戳形式YYYYmmDDHHMM
        # 项目名，project + env + major_version + minor_version
        self.project_name = ''
        self.unity_path = ''
        self.release = False                     # 是否release
        self.google_play = False                 # 是否google play
        self.unity_project_root_path = ''        # unity项目跟目录
        self.export_dir = ''                     # 输出项目目录
        self.apk_dir = ''                        # 输出apk目录
        self.full_res = True                     # 输出整包（否则将资源分离）

    def init(self):
        long_options = [
            "unity_version=",
            "project=",
            "major_version=",
            "env=",
            "target_platform=",
            "feishu_url=",
            "defines=",
            'channel_id=',
            'remove_package=',
            'public_package=',
            'commit_id=',
            'export_dir=',
            'apk_dir=',
            'full_res=',
            'package_upload_url='
        ]
        try:
            opts, _ = getopt.getopt(sys.argv[1:], "", long_options)
        except getopt.GetoptError:
            log('need this args: ' + json.dumps(long_options) +
                " now is: " + json.dumps(sys.argv[1:]))
            sys.exit(2)
        for opt, arg in opts:
            if opt == '--unity_version':
                self.unity_version = arg
            elif opt == '--project':
                self.project = arg
            elif opt == '--major_version':
                self.major_version = arg
            elif opt == '--env':
                self.env = arg
            elif opt == '--target_platform':
                self.target_platform = arg
            elif opt == '--package_upload_url':
                self.package_upload_url = arg
                log('--package_upload_url = ' + self.package_upload_url)
            elif opt == '--feishu_url':
                self.feishu_url = arg
            elif opt == '--defines':
                self.defines = arg
            elif opt == '--channel_id':
                self.channel_id = arg
            elif opt == '--remove_package':
                self.remove_package = not (
                    arg == '0' or str.lower(arg) == 'false')
            elif opt == '--public_package':
                self.public_package = not (
                    arg == '0' or str.lower(arg) == 'false')
            elif opt == '--commit_id':
                self.commit_id = arg
            # elif opt == '--export_dir':
            #     self.export_dir = arg
            # elif opt == '--apk_dir':
            #     self.apk_dir = arg
            elif opt == '--full_res':
                self.full_res = not (arg == '0' or str.lower(arg) == 'false')

        if False == self.check():
            log('check failed!')
            return False

        self.afterArgsInit()
        log('==========build args init succ: ' + json.dumps(self,
            default=lambda obj: obj.__dict__, sort_keys=True))

    def check(self):
        # TODO:
        # 各个参数输入是否正确
        # 平台是否正确，例如ios是否在mac上
        return True

    def addDefine(self, define):
        if len(self.defines) > 0 and self.defines[len(self.defines) - 1] != ',':
            self.defines = self.defines + ","
        self.defines = self.defines + define

    def afterArgsInit(self):
        # 生成一系列后续参数
        self.unity_project_root_path = os.getcwd()

        self.minor_version = time.strftime(
            '%m%d', time.localtime(time.time())) + '-' + self.commit_id
        self.project_name = self.project + "." + self.env + "." + \
            self.channel_id + "." + self.major_version + "." + self.minor_version

        # TODO：支持windows
        if platform.system() == "Windows":
            self.unity_path = "D:\\\"Program Files\"\\Unity\\Hub\\Editor\\" + self.unity_version + "\\Editor\\Unity.exe"
        elif platform.system() == "Linux":
            os.system("echo -e 'm_EditorVersion: 2020.3.16f1\nm_EditorVersionWithRevision: 2020.3.16f1 (049d6eca3c44)' > ProjectSettings/ProjectVersion.txt")
            # self.unity_path = "/home/ab/Unity/Hub/Editor/" + self.unity_version[0:-2]  + "/Editor/Unity"
            self.unity_path = "/Applications/Unity/Unity.app/Contents/MacOS/Unity"
        else:
            # self.unity_path = "/Applications/Unity/Hub/Editor/" + self.unity_version + "/Unity.app/Contents/MacOS/Unity"
            self.unity_path = "/Applications/Unity/Hub/Editor/2021.3.16f1c1/Unity.app/Contents/MacOS/Unity"
        if self.target_platform == 'google_play':
            self.google_play = True

        # 根据选项配置宏
        if self.env[-6:] == 'online':
            self.release = True
            self.addDefine('RELEASE')
        else:
            self.release = False
            self.addDefine('USER_DEBUG')

        return True
