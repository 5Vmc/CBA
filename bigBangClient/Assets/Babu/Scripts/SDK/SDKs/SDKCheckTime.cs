using LightJson;
using System;
using UnityEngine;

namespace Babu.SDK
{
    public class SDKCheckTime : Task
    {
        [SerializeField]
        private bool _enalbe = false;

        [SerializeField]
        private int _limitTime = 0;

        protected int _retryTaskId = 0;

        const string URL = "https://cn.api.babuyo.com/time.php";

        public override string GetTaskName()
        {
            return "SDKCheckTime";
        }

        public override void Run(TaskExecutor executor)
        {
            //if (!_enalbe)
            //{
            executor.OnChildTaskCompleted();
            //    return;
            //}

            //_retryTaskId = RetryTaskService.Instance.RunAsync(3, 5, () =>
            //{
            //    Debug.Log("Request Check Time Url: " + URL);
            //    HttpService.Instance.AsyncGet(URL, (result, response) =>
            //    {
            //        try
            //        {
            //            if (result == false)
            //            {
            //                RetryTaskService.Instance.OnAsyncTaskFailed(_retryTaskId);
            //                return;
            //            }

            //            JsonValue value = JsonValue.Parse(response);

            //            int time = (int)Math.Round(value["now"].AsNumber / 1000);
            //            if (time > _limitTime)
            //            {
            //                Debug.Log("Limit");
            //                QuickSDK.getInstance().exitGame();
            //                return;
            //            }

            //            RetryTaskService.Instance.OnAsyncTaskSucc(_retryTaskId);
            //            executor.OnChildTaskCompleted();
            //        }
            //        catch (Exception ex)
            //        {
            //            Debug.LogException(ex);
            //            RetryTaskService.Instance.OnAsyncTaskFailed(_retryTaskId);
            //        }
            //    }, 5);
            //},
            //() =>
            //{
            //    Debug.LogError("Request Check Time Failed");
            //    QuickSDK.getInstance().exitGame();
            //});
        }
    }
}
