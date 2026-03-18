using UnityEngine;

namespace Babu.SDK.ThirdPart.Demo
{
    public class SDKDemo : Task
    {
        // 启动的时候注册
        //[RuntimeInitializeOnLoadMethod]
        //static void InitOnRuntime()
        //{
        //    var sdk = SDKThirdPart.Instance.gameObject.AddComponent<SDKDemo>();
        //    SDKThirdPart.Instance.AddThirdPartSDK(sdk);
        //}

        public override string GetTaskName()
        {
            return "SDKDemo";
        }

        public override void Run(TaskExecutor executor)
        {
            // TODO：初始化操作
        }
    }

}