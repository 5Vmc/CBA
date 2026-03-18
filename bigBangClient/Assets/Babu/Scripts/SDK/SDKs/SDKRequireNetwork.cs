using UnityEngine;

namespace Babu.SDK
{
    class SDKRequireNetwork : Task
    {
        [SerializeField] int timeout = 5;

        const string URL = "https://cn.api.babuyo.com/time.php";

        public override string GetTaskName()
        {
            return "SDKRequireNetwork";
        }

        public override void Run(TaskExecutor executor)
        {
            Environment.SetValue("sdk_requre_network", true);

            // 请求一次无用的网络，把第一次网络请求时候的失败情况过滤掉
            HttpService.Instance.AsyncGet(URL, delegate(bool result, string response)
            {
                executor.OnChildTaskCompleted();
            }, timeout);
        }
        
    }
}
