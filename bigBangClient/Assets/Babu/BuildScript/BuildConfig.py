import json
from Utils import log

class BuildConfig:
    def __init__(self, build_args):
        self.build_args = build_args
    
    def init(self):
        with open("Assets/Babu/BuildConfig/BabuBuildConfig.json", 'r', encoding="utf-8") as f:
            if not f:
                return False
            self.config = root = json.load(f)
            if "default" in root:
                self.config = root["default"]
            if self.build_args.channel_id in root:
                self.merge(self.config, root[self.build_args.channel_id])
            log('==========build config init succ: ' + json.dumps(self.config, default=lambda obj: obj.__dict__, sort_keys=True))
            return True

    def merge(self, config, chanel_config):
        for key in chanel_config.keys():
            if type(chanel_config[key]).__name__ == 'dict':
                self.merge(config[key], chanel_config[key])
            else:
                config[key] = chanel_config[key]
    
    def getConfig(self, config_name):
        attrs = config_name.split(".")
        config = self.config
        for i in range(len(attrs)):
            config = config[attrs[i]]
        return config