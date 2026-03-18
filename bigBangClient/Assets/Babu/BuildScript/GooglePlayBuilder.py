# coding=utf-8

import os
import uuid
from Utils import log, sendDingMsg
from Builder import Builder


class GooglePlayBuilder(Builder):
    def __init__(self, build_args, build_config):
        Builder.__init__(self, build_args, build_config)

    def build(self):
        self.buildPackage()
        download_url = self.uploadToServer(
            self.build_args.project_name + ".aab")
        if self.build_args.remove_package:
            self.removePackage()
        self.sendSuccDingMsg(download_url)

    def removePackage(self):
        os.remove(self.build_args.project_name + ".aab")

    def sendSuccDingMsg(self, download_url):
        ding_msg = "【打包结果】{} {} {}打包成功，下载地址：{}".format(
            self.build_args.target_platform, self.build_args.env, self.build_args.channel_id, download_url)
        sendDingMsg(self.build_args.feishu_url, ding_msg)
