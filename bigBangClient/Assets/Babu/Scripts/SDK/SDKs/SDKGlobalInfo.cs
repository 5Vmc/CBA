using LightJson;
using System;
using UnityEngine;

namespace Babu.SDK
{
    class SDKGlobalInfo : Task
    {
        [SerializeField] int timeout = 5;

#if BABU_RELEASE
        const string ENVIRONMENT_URL = "https://cn.api.babuyo.com/online/bigbang/environment.php";
        const bool BABU_RELEASE = true;
#else
        const string ENVIRONMENT_URL = "https://cn.api.babuyo.com/pre/bigbang/environment.php";
        const bool BABU_RELEASE = false;
#endif

        public class Event
        {
            // 外部可以通过这几个事件进行交互
            public const string ShowGetEnviromentFailedTip = "ShowGetEnviromentFailedTip";    // 显示环境变量获取失败提示
        }

        protected TaskExecutor _executor;
        protected int _retryTaskId = 0;

        public override string GetTaskName()
        {
            return "SDKGlobalInfo";
        }

        public override void Run(TaskExecutor executor)
        {
            Environment.SetValue("sdk_global_info", true);

            _executor = executor;

            string channelId = Environment.GetValue<string>("channel_id", "unknown");

            Environment.SetValue("platform", Application.platform.ToString());
            Environment.SetValue("babu_release", BABU_RELEASE);

            string url = ENVIRONMENT_URL + "?appVersion=" + Application.version + "&packageName=" + Application.identifier + "&channel=" + channelId;
            _retryTaskId = RetryTaskService.Instance.RunAsync(3, 5, () =>
            {
                Debug.Log("Request Url: " + url);
                HttpService.Instance.AsyncGet(url, OnGetEnvironment, timeout);
            },
            () =>
            {
                Debug.LogError("Get Environment Failed");
                EventManager.Instance.Dispatch(Event.ShowGetEnviromentFailedTip);
                _executor.OnChildTaskPaused();
            });
        }

        void OnGetEnvironment(bool result, string response)
        {
            try
            {
                // 调用失败;
                if (result == false)
                {
                    RetryTaskService.Instance.OnAsyncTaskFailed(_retryTaskId);
                    return;
                }

                JsonValue value = JsonValue.Parse(response);
                Environment.SetJsonObjectToEnvironment(value.AsJsonObject);

                if (Environment.GetValue<bool>("enable_debug_model", false) == false)
                {
                    // 非调试模式下只显示Warning以下的日志;
                    Debug.unityLogger.filterLogType = LogType.Warning;

                    // 非调试模式下关闭Log类型的日志写入到内部文件
                    Logger.Instance.DisableLogType(LogType.Log);
                }

                RetryTaskService.Instance.OnAsyncTaskSucc(_retryTaskId);
                _executor.OnChildTaskCompleted();
            }
            catch (Exception e)
            {
                Debug.LogError("Get Environment Failed: " + e.Message);
                EventManager.Instance.Dispatch(Event.ShowGetEnviromentFailedTip);
                _executor.OnChildTaskPaused();
            }
        }
    }
}
