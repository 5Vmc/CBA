using LightJson;
using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;

namespace Babu
{
    public class BabuHttpService : BabuSingleton<BabuHttpService>
    {
        public class Error
        {
            public const int UNKNOWN = -1;
            public const int SUCC = 0;
        }

        const string SIGN_KEY = "ey0Jm8uNBJBKXEZY";

        public void Request(string url, JsonValue requestData, Action<JsonValue> callback)
        {
            if (requestData.AsJsonObject.ContainsKey("package_name") == false)
            {
                requestData["package_name"] = Application.identifier;
            }
            
            Request(url, requestData.ToString(), callback);
        }

        public void Request(string url, Dictionary<string, object> requestData, Action<JsonValue> callback)
        {
            if (requestData.ContainsKey("package_name") == false)
            {
                requestData.Add("package_name", Application.identifier);
            }

            string jsonString = JsonConvert.SerializeObject(requestData);
            Request(url, jsonString, callback);
        }

        void Request(string url, string jsonString, Action<JsonValue> callback)
        {
            JsonObject data = new JsonObject();
            data["json"] = jsonString;
            data["sign"] = CodecUtils.Hex(CodecUtils.Md5(Encoding.UTF8.GetBytes(jsonString + SIGN_KEY)));
            HttpService.Instance.AsyncPost(url, data.ToString(), (result, response)=>
            {
                try
                {
                    if (false == result)
                    {
                        throw new Exception("Request Failed!");
                    }

                    JsonValue data = GetResponseData(response);
                    callback(data);
                }
                catch (Exception e)
                {
                    Debug.LogError("Catch Exception: " + e.Message);

                    JsonObject reponseJson = new JsonObject();
                    reponseJson["state"] = Error.UNKNOWN;
                    callback(reponseJson);
                }
            }, 10);
        }

        JsonValue GetResponseData(string response)
        {
            JsonValue data = JsonValue.Parse(response);
            int state = data["state"];
            string msg = data["message"];

            Debug.Log($"State: {state}, Msg: {msg}");
            return data;
        }
    }
}
