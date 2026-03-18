# coding=utf-8

from Utils import logFail
from Utils import log, sendDingMsg, sendFeiShuLinkMsg , sendFeiShuMsg
import sys
from BuildArgs import BuildArgs
from BuildConfig import BuildConfig
from AndroidBuilder import AndroidBuilder
from IOSBuilder import IOSBuilder
from GooglePlayBuilder import GooglePlayBuilder
from GlobalizationBuilder import GlobalizationBuilder
import traceback

def createBuilder(build_args, build_config):
    if build_args.target_platform == 'android':
        return AndroidBuilder(build_args, build_config)
    if build_args.target_platform == 'ios':
        return IOSBuilder(build_args, build_config)
    if build_args.target_platform == "google_play":
        return GooglePlayBuilder(build_args, build_config)

    print("error target :", build_args.target_platform)
    sys.exit(-1)

if __name__ == '__main__':
    build_args = BuildArgs()
    if False == build_args.init():
        sys.exit(1)
    
    build_config = BuildConfig(build_args)
    if False == build_config.init():
        sys.exit(1)

    

    try:
        builder = createBuilder(build_args, build_config)
        builder.build()
        # # 国际化预处理
        # globalization_builder = GlobalizationBuilder(build_args, build_config)
        # globalization_builder.build()
    except Exception as e:
        logFail(str(e))
        traceback.print_exc()
        ding_msg = "【打包结果】{} {} 打包失败".format(build_args.target_platform, build_args.env)
        # sendDingMsg(build_args.ding_url, ding_msg)
        sendFeiShuMsg(build_args.feishu_url, ding_msg)
        sys.exit(1)