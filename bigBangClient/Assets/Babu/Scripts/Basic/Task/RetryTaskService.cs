using System;
using System.Collections.Generic;
using UnityEngine;

namespace Babu
{
    class RetryTaskService : BabuSingleton<RetryTaskService>
    {
        public delegate bool SyncTaskCallback();
        public delegate void AsyncTaskCallback();

        static int _asyncRetryTaskIdGenerator = 0;
        class AsyncTaskInfo
        {
            public int curRetryCount;
            public int maxRetryCount;
            public float delayTime;
            public AsyncTaskCallback callback;
            public Action retryFailedCallback;
        }
        Dictionary<int, AsyncTaskInfo> _asyncRetryTaskInfos = new Dictionary<int, AsyncTaskInfo>();

        public void Run(int maxRetryCount, float delayTime, SyncTaskCallback callback, Action retryFailedCallback)
        {
            Run(0, maxRetryCount, delayTime, callback, retryFailedCallback);
        }

        void Run(int curRetryCount, int maxRetryCount, float delayTime, SyncTaskCallback callback, Action retryFailedCallback)
        {
            if (false == callback())
            {
                if (maxRetryCount != -1 && curRetryCount >= maxRetryCount)
                {
                    retryFailedCallback();
                    return;
                }

                DelayTaskService.Instance.Run(this.gameObject, delayTime, () =>
                {
                    Run(curRetryCount + 1, maxRetryCount, delayTime, callback, retryFailedCallback);
                });
            }
        }

        public int RunAsync(int maxRetryCount, float delayTime, AsyncTaskCallback callback, Action retryFailedCallback)
        {
            int id = ++_asyncRetryTaskIdGenerator;
            AsyncTaskInfo asyncTaskInfo = new AsyncTaskInfo();
            asyncTaskInfo.curRetryCount = 0;
            asyncTaskInfo.maxRetryCount = maxRetryCount;
            asyncTaskInfo.delayTime = delayTime;
            asyncTaskInfo.callback = callback;
            asyncTaskInfo.retryFailedCallback = retryFailedCallback;

            _asyncRetryTaskInfos.Add(id, asyncTaskInfo);
            callback();
            return id;
        }

        public void OnAsyncTaskSucc(int id)
        {
            _asyncRetryTaskInfos.Remove(id);
        }

        public void OnAsyncTaskFailed(int id)
        {
            AsyncTaskInfo asyncTaskInfo;
            if (_asyncRetryTaskInfos.TryGetValue(id, out asyncTaskInfo) == false)
            {
                Debug.LogError("Can Not Find Task Id: " + id);
                return;
            }

            ++asyncTaskInfo.curRetryCount;
            if (asyncTaskInfo.maxRetryCount != -1 && asyncTaskInfo.curRetryCount >= asyncTaskInfo.maxRetryCount)
            {
                asyncTaskInfo.retryFailedCallback();
                _asyncRetryTaskInfos.Remove(id);
            }
            else
            {
                DelayTaskService.Instance.Run(this.gameObject, asyncTaskInfo.delayTime, ()=>
                {
                    asyncTaskInfo.callback();
                });
            }
        }
    }
}
