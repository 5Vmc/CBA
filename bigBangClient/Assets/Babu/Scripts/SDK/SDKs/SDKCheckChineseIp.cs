using Babu.Globalization;
using LightJson;
using System;
using UnityEngine;

namespace Babu.SDK
{
    public class SDKCheckChineseIp : Task
    {
        public class Event
        {
            public const string CheckChineseFailed = "__CheckChineseFailed";
            public const string ChineseIp = "__ChineseIp";
        }

        public override string GetTaskName()
        {
            return "SDKCheckChineseIp";
        }

        protected TaskExecutor _executor;

        const string CHECK_URL = "https://cdn.api.babuyo.com/is_chinese_ip.php";
        public override void Run(TaskExecutor executor)
        {
#if UNITY_EDITOR
            executor.OnChildTaskCompleted();
#else
            Environment.SetValue("sdk_check_chinese", true);

            if (Globalizer.Instance.IsInternationalVersion() == false)
            {
                Debug.Log("Not International Version, Ignore Check Chinese Ip");
                executor.OnChildTaskCompleted();
                return;
            }

            _executor = executor;
            HttpService.Instance.AsyncGet(CHECK_URL, OnGetResult, 60);
#endif
        }

        void OnGetResult(bool result, string response)
        {
            try
            {
                if (false == result)
                {
                    throw new Exception("Network Error");
                }

                JsonValue json = JsonValue.Parse(response);
                string clientIp = json["ip"].AsString;
                bool isChineseIp = json["is_chinese_ip"].AsBoolean;
                Debug.Log($"Client Ip: {clientIp}, Is Chinese Ip: {isChineseIp}");
                if (isChineseIp == false)
                {
                    _executor.OnChildTaskCompleted();
                }
                else
                {
                    EventManager.Instance.Dispatch(new object[] { Event.ChineseIp });
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Check Chinese Ip Catch Exception: " + e.Message);
                EventManager.Instance.Dispatch(new object[] { Event.CheckChineseFailed });
            }
        }
    }
}
