import os
from Utils import log
from Builder import Builder

class GlobalizationBuilder(Builder):
    def __init__(self, build_args, build_config):
      Builder.__init__(self, build_args, build_config)
    
    def build(self):
        self.preOprateConfig()
        self.preOprateUI()
    
    def preOprateUI(self):
        log('================begin ui globalize package===============')
        self.buildPackage()
        log('================end ui globalize package===============')

    def getUnityBuildMethod(self):
        return "Babu.Globalization.Editor.Editor.PreOprateUIChineseText"
    
    def preOprateConfig(self):
        log('================begin config globalize package===============')
        ret = os.system('python3 Assets/Babu/Globalization/Python/PreOprateConfigChineseText.py')
        if ret != 0:
            log('config globalize failed')
            raise Exception()
        log('================end config globalize package===============')