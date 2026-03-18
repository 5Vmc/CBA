using UnityEngine;

namespace Babu.SDK
{
    public class SDKManagerBeforeHotFix : SequentialTaskExecutor
    {

        public static SDKManagerBeforeHotFix Instance;

        void Awake()
        {
            Instance = this;

            Debug.Log("MiGuPlay初始化开始");
            MiGuPlayManager.Instance.Init();
            Debug.Log("ByteDance初始化开始");
            ByteDanceManager.Instance.Init();
        }
    }
}
