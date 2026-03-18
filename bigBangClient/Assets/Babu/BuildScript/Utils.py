# coding=utf-8

import sys
import os
import json
import re
import time
from colorama import init, Fore, Back, Style

init(autoreset=True)
class Color(object):
    
    #  前景色:红色  背景色:默认
    def red(self, s):
        return Fore.RED + s + Fore.RESET

    #  前景色:绿色  背景色:默认
    def green(self, s):
        return Fore.GREEN + s + Fore.RESET

    #  前景色:黄色  背景色:默认
    def yellow(self, s):
        return Fore.YELLOW + s + Fore.RESET

    #  前景色:蓝色  背景色:默认
    def blue(self, s):
        return Fore.BLUE + s + Fore.RESET

    #  前景色:洋红色  背景色:默认
    def magenta(self, s):
        return Fore.MAGENTA + s + Fore.RESET

    #  前景色:青色  背景色:默认
    def cyan(self, s):
        return Fore.CYAN + s + Fore.RESET

    #  前景色:白色  背景色:默认
    def white(self, s):
        return Fore.WHITE + s + Fore.RESET

    #  前景色:黑色  背景色:默认
    def black(self, s):
        return Fore.BLACK

    #  前景色:白色  背景色:绿色
    def white_green(self, s):
        return Fore.WHITE + Back.GREEN + s

    def dave(self, s):
        return Style.BRIGHT + Fore.GREEN + s

color = Color()

def logColor(str):
    print(color.cyan(str))
    sys.stdout.flush()
    
def logFail(str):
    print(color.red(str))
    sys.stdout.flush()

def logSucc(str):
    print(color.green(str)) 
    sys.stdout.flush()

def log(str):
    print(str)
    sys.stdout.flush()

def sendDingMsg(ding_url, message):
    return
    # TODO：在windows下发送钉钉
    content = {}
    content['msgtype'] = 'text'
    text = {}
    text['content'] = message
    content['text'] = text
    cmd = "curl '" + ding_url + "' -H 'Content-Type: application/json' -d " + \
        '\'' + json.dumps(content) + '\''
    log(cmd)
    os.system(cmd)

def sendFeiShuMsg(fieshu_url, message):
    # 发送飞书,仅有文本
    jsonStr = "{\"msg_type\":\"text\",\"content\":{\"text\":\"" + message + "\"}}";
    cmd = "curl -X POST -H \"Content-Type: application/json\" -d \'" + jsonStr + "\' " + fieshu_url
    log(cmd)
    os.system(cmd)

def sendFeiShuLinkMsg(fieshu_url, titleText, messageText, link , LinkText):
    # 发送飞书,带链接
    jsonStr = "{\"msg_type\": \"post\",\"content\": {\"post\": {\"zh_cn\": {\"title\": \"" + titleText + "\",\"content\": [[{\"tag\": \"text\",\"text\": \"" + messageText + "\"},{\"tag\": \"a\",\"text\": \"" + LinkText + "\",\"href\": \"" + link + "\"}]]}}}}"
    cmd = "curl -X POST -H \"Content-Type: application/json\" -d \'" + jsonStr + "\' " + fieshu_url
    log(cmd)
    os.system(cmd)


CHINESE_REGEX = re.compile(u'[\u4e00-\u9fa5]+')

def hasChinese(text):
    return CHINESE_REGEX.search(text)