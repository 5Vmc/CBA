using LightJson;
using System;
using UnityEditor;
using UnityEngine;

namespace Babu.Editor.Build
{
    public class BuildConfig
    {
        public JsonValue Config;

        public void Load(string filePath, string channelId)
        {
            TextAsset textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(filePath);
            if (textAsset == null)
            {
                throw new Exception($"Can Not Find: {filePath}");
            }

            JsonValue root = JsonValue.Parse(textAsset.text);
            Config = root;
            if (root.AsJsonObject.ContainsKey("default"))
            {
                // 如果有default，优先使用default
                Config = root["default"];
            }
            if (root.AsJsonObject.ContainsKey(channelId))
            {
                // 合并channel的特殊配置
                Merge(Config, root[channelId]);
            }

            Debug.Log($"{filePath} Config Load Succ: " + Config.ToString(true));
        }

        void Merge(JsonValue config, JsonValue channelConfig)
        {
            foreach (var iter in channelConfig.AsJsonObject)
            {
                if (iter.Value.IsJsonObject)
                {
                    Merge(config[iter.Key], iter.Value);
                }
                else
                {
                    config[iter.Key] = iter.Value;
                }
            }
        }

        public void Load(string filePath)
        {
            Load(filePath, BuildArgs.Instance.ChannelId);
        }

        public string GetConfig(string configName)
        {
            return Config[configName].AsString;
        }
        public string GetConfig(string configName1, string configName2)
        {
            return Config[configName1][configName2].AsString;
        }
    }
}
