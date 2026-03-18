using System.Text;
using System;
using System.Collections.Generic;
using UnityEngine;
using LightJson;

namespace Babu
{

    /*
{
	"package_name": "com.qiduo.cba.bigbang",
	"channel_id": "MIGU",
	"release": false,
	"major_version": "1.0.0",
	"minor_version": "0317-0",
	"purchase_product_ids": "",
	"client_creat_time": "202303171723",
	"full_res": true
}
     */
    /// <summary>
    /// 打包前，会在Resources下生成Environment.json，内容如上
    /// </summary>
    public class Environment
    {
        public static string bundleCreatTime = "";

        public static void LoadEnvironment()
        {
            try
            {
                TextAsset textAsset = Resources.Load<TextAsset>("Environment");
                if (textAsset != null)
                {
                    JsonValue jsonValue = JsonValue.Parse(textAsset.text);
                    SetJsonObjectToEnvironment(jsonValue.AsJsonObject);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("LoadEnvironment Catch Exception: " + e.Message);
            }
        }

        public static void SetJsonObjectToEnvironment(JsonObject jsonObject)
        {
            foreach (var iter in jsonObject)
            {
                if (iter.Value.IsBoolean)
                {
                    Environment.SetValue(iter.Key, iter.Value.AsBoolean);
                }
                else if (iter.Value.IsInteger)
                {
                    Environment.SetValue(iter.Key, iter.Value.AsInteger);
                }
                else if (iter.Value.IsNumber)
                {
                    Environment.SetValue(iter.Key, iter.Value.AsNumber);
                }
                else if (iter.Value.IsString)
                {
                    Environment.SetValue(iter.Key, iter.Value.AsString);
                }
            }
        }

        private static Dictionary<string, object> _environmentValues = new Dictionary<string, object>();

        public static void SetValue(string key, object value)
        {
            //Debug.LogFormat("Set Environment: {0}: {1}", key, value.ToString());
            _environmentValues[key] = value;
        }

        public static T GetValue<T>(string key, T defaultValue)
        {
            try
            {
                object ret;
                if (_environmentValues.TryGetValue(key, out ret))
                {
                    return (T)ret;
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Environment GetValue Failed: " + e.Message);
            }

            return defaultValue;
        }

        public static List<string> GetContainKeys(string startWith)
        {
            List<string> ret = new List<string>();
            foreach (var iter in _environmentValues)
            {
                if (iter.Key.StartsWith(startWith))
                {
                    ret.Add(iter.Key);
                }
            }
            return ret;
        }

        public static string GetEnvironmentData()
        {
            StringBuilder builder = new StringBuilder();
            foreach (var item in _environmentValues)
            {
                builder.Append(item.Key);
                builder.Append(':');
                builder.Append(item.Value.ToString());
                builder.Append('\n');
            }
            return builder.ToString();
        }
    }
}
