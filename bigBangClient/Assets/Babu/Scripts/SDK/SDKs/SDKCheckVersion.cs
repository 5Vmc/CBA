using System;
using UnityEngine;

namespace Babu.SDK
{
    // 版本检测
    class SDKCheckVersion : Task
    {
        TaskExecutor _executor;

        public class Event
        {
            // 外部可以通过这几个事件进行交互
            public const string ShowGetVersionInfoFailedTip = "__ShowGetVersionInfoFailedTip";    // 显示版本获取失败提示
            public const string ShowForceUpdateTip = "__ShowForceUpdateTip";                      // 显示强更提示
            public const string ShowAskUpdateTip = "__ShowAskUpdateTip";                          // 显示询问更新提示
            public const string ForceUpdate = "__ForceUpdate";                                    // 强更
            public const string AskUpdate = "__AskUpdate";                                        // 进行询问更新
        }

        public override string GetTaskName()
        {
            return "SDKCheckVersion";
        }

        void Awake()
        {
        }

        void Start()
        {
            EventManager.Instance.Register(Event.ForceUpdate, OnForceUpdate);
            EventManager.Instance.Register(Event.AskUpdate, OnAskUpdate);
        }

        void OnForceUpdate(object[] args)
        {
            string forceUpdateUrl = Environment.GetValue<string>("force_update_url", "");
            if (forceUpdateUrl.Length > 0)
            {
                Application.OpenURL(forceUpdateUrl);
            }
            else
            {
                Debug.LogError("Invalid Force Update Url");
            }
        }

        void OnAskUpdate(object[] args)
        {
            bool gotoUpdate = (bool)args[0];
            if (gotoUpdate)
            {
                string askUpdateUrl = Environment.GetValue<string>("ask_update_url", "");
                if (askUpdateUrl.Length > 0)
                {
                    Application.OpenURL(askUpdateUrl);
                }
                else
                {
                    Debug.LogError("Invalid Ask Update Url");
                }
            }
            _executor.OnChildTaskResumed();
            _executor.OnChildTaskCompleted();
        }

        public override void Run(TaskExecutor executor)
        {
            Environment.SetValue("sdk_check_version", true);

            _executor = executor;
            CheckVersion();
        }

        void CheckVersion()
        {
            try
            {
                int curVersion = Utils.VersionToInt(Application.version);
                int forceUpdateVersion = Utils.VersionToInt(Environment.GetValue<string>("force_update_version", "0.0.0"));
                int askUpdateVersion = Utils.VersionToInt(Environment.GetValue<string>("ask_update_version", "0.0.0"));

                Debug.LogFormat("Cur Version: {0}, Force Update Version: {1}, Ask Update Version: {2}", curVersion, forceUpdateVersion, askUpdateVersion);

                if (curVersion < forceUpdateVersion)
                {
                    EventManager.Instance.Dispatch(Event.ShowForceUpdateTip);
                    _executor.OnChildTaskPaused();
                }
                else if (curVersion < askUpdateVersion)
                {
                    EventManager.Instance.Dispatch(Event.ShowAskUpdateTip);
                    _executor.OnChildTaskPaused();
                }
                else
                {
                    _executor.OnChildTaskCompleted();
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Get Version Failed: " + e.Message);
                EventManager.Instance.Dispatch(Event.ShowGetVersionInfoFailedTip);
                _executor.OnChildTaskPaused();
            }
        }
    }
}
