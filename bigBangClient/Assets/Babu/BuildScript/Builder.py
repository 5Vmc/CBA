# coding=utf-8

import io
import os
from sys import platform
import platform
import time
import uuid
import threading
from BuildArgs import BuildArgs
from Utils import logFail, logSucc, logColor
from Utils import log
import sys
import paramiko
import requests

class UnityOutputLogThread(threading.Thread):
    def __init__(self, log_path):
        threading.Thread.__init__(self)
        self.stop = False
        self._log_path = log_path

    def run(self):
        read_pos = 0
        fp = None
        while self.stop == False:
            if os.path.isfile(self._log_path):
                fp = fp or io.open(self._log_path, 'r', encoding='utf-8')

            if fp != None:
                fp.seek(read_pos)
                all_lines = fp.readlines()
                read_pos = fp.tell()
                fp.close()
                fp = None
                for line in all_lines:
                    log(line)
                    #log(line.encode(encoding="utf-8"))

            time.sleep(1)

class Builder:
    def __init__(self, build_args: BuildArgs, build_config):
        self.build_args = build_args
        self.build_config = build_config
    
    def build(self):
        log('invalid build')
        raise Exception()

    def buildAsset(self):
        log_path = "asset.log"
        if os.path.isfile(log_path):
            os.remove(log_path)

        cmd = self.getUnityCmd(log_path, "asset")
        log(cmd)

        log('===============begin build asset=============')

        log_thread = UnityOutputLogThread(log_path)
        log_thread.start()
        ret = os.system(cmd)

        time.sleep(10)
        log_thread.stop = True
        log_thread.join()

        if ret != 0:
            logFail('==============build asset failed!============')
            raise Exception()
        logSucc('============build asset succ!============')

    def buildPackage(self):
        log_path = "build.log"
        if os.path.isfile(log_path):
            os.remove(log_path)

        cmd = self.getUnityCmd(log_path, "package")
        log(cmd)
        
        log('===============begin build package=============')

        log_thread = UnityOutputLogThread(log_path)
        log_thread.start()
        ret = os.system(cmd)

        time.sleep(10)
        log_thread.stop = True
        log_thread.join()

        if ret != 0:
            logFail('==============build package failed!============')
            raise Exception()
        logSucc('============build package succ!============')

    def setDefines(self, defines):
        log_path = "define.log"
        if os.path.isfile(log_path):
            os.remove(log_path)
        cmd = self.getUnityCmd(log_path, defines)
        log(cmd)
        log('===============begin set defines=============')
        log_thread = UnityOutputLogThread(log_path)
        log_thread.start()
        ret = os.system(cmd)

        time.sleep(10)
        log_thread.stop = True
        log_thread.join()
        if ret != 0:
            logFail('==============begin set defines failed!============')
            raise Exception()
        logSucc('============begin set defines succ!============')

    def exportAs(self):
        log_path = "buildAs.log"
        if os.path.isfile(log_path):
            os.remove(log_path)
        cmd = self.getUnityCmd(log_path, "exportAs")
        log(cmd)
        log('===============begin export as=============')
        log_thread = UnityOutputLogThread(log_path)
        log_thread.start()
        ret = os.system(cmd)

        time.sleep(10)
        log_thread.stop = True
        log_thread.join()
        if ret != 0:
            logFail('==============begin export as failed!============')
            raise Exception()
        logSucc('============begin export as succ!============')

    def exportXcodeProject(self):
        log_path = "buildXcodeProject.log"
        if os.path.isfile(log_path):
            os.remove(log_path)
        cmd = self.getUnityCmd(log_path, "exportXcodeProject")
        log(cmd)
        log('===============begin exportXcodeProject=============')
        log_thread = UnityOutputLogThread(log_path)
        log_thread.start()
        ret = os.system(cmd)

        time.sleep(10)
        log_thread.stop = True
        log_thread.join()
        if ret != 0:
            logFail('==============begin exportXcodeProject failed!============')
            raise Exception()
        logSucc('============begin exportXcodeProject succ!============')
    
    def getUnityCmd(self, log_path, buildWhich):
        release_flag = "1" if self.build_args.release else "0"
        full_res_flag  = "1" if self.build_args.full_res else "0"
        if platform.system() == "Linux":
            cmd = "export DISPLAY=:0 && "
        else:
            cmd = ""
        
        build_target = "iOS" if self.build_args.target_platform == "ios" else "Android"

        method = ""
        if buildWhich == "setDefines":
            method = self.getUnitySetDefinesMethod()
        elif buildWhich == "setDefinesTestBundle":
            method = self.getUnitySetDefinesTestBundleMethod()
        elif buildWhich == "setDefinesDaChenBundle":
            method = self.getUnitySetDefinesDaChenBundleMethod()
        elif buildWhich == "exportAs":
            method = self.getUnityExportAsMethod()
        elif buildWhich == "exportXcodeProject":
            method = self.getUnityExportXcodeProjectMethod()
        elif buildWhich == "package":
            method = self.getUnityBuildMethod()
        elif buildWhich == "asset":
            method = self.getUnityBuildAddressablesMethod()
        else:
            print("only support unity method (package, asset)")
            sys.exit(-1)

        cmd = cmd + self.build_args.unity_path + \
            " -batchmode " + \
            " -buildTarget " + build_target + \
            " -projectPath " + self.getProjectPath() + \
            " -export_dir " + self.build_args.export_dir + \
            " -executeMethod " + method + \
            " -project " + self.build_args.project_name + \
            " -target_platform " + self.build_args.target_platform + \
            " -major_version " + self.build_args.major_version + \
            " -minor_version " + self.build_args.minor_version + \
            " -defines " + self.build_args.defines + \
            " -channel_id " + self.build_args.channel_id + \
            " -release " + release_flag + \
            " -from_script 1" + \
            " -logfile " + log_path + \
            " -full_res " + full_res_flag + \
            " -quit"
        return cmd
    
    #项目地址
    def getProjectPath(self):
        return self.build_args.unity_project_root_path

    def getUnityBuildMethod(self):
        return "Babu.Editor.Build.Builder.Build"

    def getUnitySetDefinesMethod(self):
        return "Babu.Editor.Build.Builder.SetDefines"
    
    def getUnitySetDefinesTestBundleMethod(self):
        return "Babu.Editor.Build.Builder.SetDefinesTestBundle"
    
    def getUnitySetDefinesDaChenBundleMethod(self):
        return "Babu.Editor.Build.Builder.SetDefinesDaChenBundle"

    def getUnityExportAsMethod(self):
        return "Babu.Editor.Build.Builder.ExportAs"
    
    def getUnityExportXcodeProjectMethod(self):
        return "Babu.Editor.Build.Builder.ExportXcodeProject"

    def getUnityBuildAddressablesMethod(self):
        return "AssetBuilder.Build"
    
    def uploadToServer(self, package):
        if self.build_args.public_package:
            return self.uploadToPublicServer(package)
        else:
            return self.uploadToInnerServer(package)
    
    def uploadToPublicServer(self, package):
        log('================begin upload public server!================')
        dir = str(uuid.uuid1())
        cmd = "source ~/.bashrc && obsutil cp " + package + " obs://babuyo-packages/" + dir + "/"
        if os.system(cmd) != 0:
            log('================upload public server failed!================')
            raise Exception()
        log('================upload public server succ!================')
        return "http://packages.babuyo.com/{}/{}".format(dir, package)

    def sftpPutCallbak(self, transferred, toBeTransferred):
        logColor("Transferred: {0}\tOut of: {1}".format(transferred, toBeTransferred))

    def uploadToInnerServer(self, package):
        os.chdir(os.path.dirname(package))
        packageName = os.path.basename(package)
        log('================begin upload inner server!================')

        ssh = paramiko.SSHClient()
# 这行代码的作用是允许连接不在know_hosts文件中的主机。
        ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())
        ssh.connect("10.101.101.101", 22, "root", "boyou2015")
        #ssh.connect("183.134.211.10", 10022, "root", "boyou2015")
        
        ssh.open_sftp().put(packageName, '/ftp/www/cba/'+packageName, self.sftpPutCallbak)

        ssh.close()
        logSucc('<<<<<<<<<<<<<<<<<upload inner server Succ>>>>>>>>>>>>>>>>>>')
        # and then
        
    '''
    def uploadToInnerServer(self, package):
        log('================begin upload inner server!================')
        cmd = 'scp ' + package + ' root@10.101.101.101:/ftp/www/cba'
        if os.system(cmd) != 0:
            log('=================upload inner server failed!==============')
            raise Exception()
        log('================upload public server succ!================')
        return "http://192.168.0.34:5005/" + package
    '''
    def uploadPackageToServer(self, package):
        log('================begin upload ' + package + ' to server!================')

        files = {'file_name': open(package, 'rb')}
        log('self.build_args.package_upload_url = ' + self.build_args.package_upload_url)
        response = requests.post(self.build_args.package_upload_url, files = files)
        if (response.ok):
            log("===============upload to server success!===================")
        else:
            log("===============upload to server fialed!====================")
            raise Exception()

    def uploadFileToFileServerUseCsharp(self , log_path , folder_name , file_path):
        print("\n==================== start upload " + file_path + " =========================\n")

        cmd = self.build_args.unity_path + \
            " -batchmode " + \
            " -executeMethod " + "Babu.Editor.Build.Builder.UploadByPython3" + \
            " -folderName " + folder_name + \
            " -filePath " + file_path + \
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