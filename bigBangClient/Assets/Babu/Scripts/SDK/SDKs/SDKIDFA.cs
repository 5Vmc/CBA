using UnityEngine;
#if UNITY_IOS && !UNITY_EDITOR
using System;
using System.Collections;
#endif

namespace Babu.SDK
{
    // IDFA检测
    class SDKIDFA : Task
    {
        public class Event
        {
            // 外部可以通过这几个事件进行交互
            public const string ShowIDFATip = "__ShowIDFATip";                    // 弹出IDFA请求框的之前显示IDFA Tip
            public const string HideIDFATip = "__HideIDFATip";                    // 隐藏IDFA Tip
        }

        public override string GetTaskName()
        {
            return "SDKIDFA";
        }

#if UNITY_IOS && !UNITY_EDITOR
        protected TaskExecutor _executor;
        bool _isIOSVerifing = true;

        public override void Run(TaskExecutor executor)
        {
            Environment.SetValue("sdk_idfa", true);

            _executor = executor;
            int iosVersion = 99999;

            try
            {
                iosVersion = Utils.VersionToInt(UnityEngine.iOS.Device.systemVersion);
                Debug.Log("IOS Version: " + iosVersion);

                if (iosVersion < 1400)
                {
                    // 小于14不用管;
                    _executor.OnChildTaskCompleted();
                    return;
                }

                if (Platform.GetIDFAAuthorizationStatus() != 0)
                {
                    // 已经选择过IDFA是否授权，直接跳过
                    _executor.OnChildTaskCompleted();
                    return;
                }

                // 审核中的版本信息
                string iosVerifyVersion = Environment.GetValue<string>("verify_version", "0.0.0");
                Debug.Log("IOS Verify Version: " + iosVerifyVersion);
                Debug.Log("App Version: " + Application.version);
                if (Utils.VersionToInt(Application.version) < Utils.VersionToInt(iosVerifyVersion))
                {
                    // 版本比配置小，说明不在审核中
                    _isIOSVerifing = false;
                }

                Environment.SetValue("is_ios_verifing", _isIOSVerifing);
            }
            catch (Exception e)
            {
                Debug.LogError("SDKIDFA Catch Exception: " + e.Message);
            }

            if (iosVersion >= 1405)
            {
                RequireIDFA1405();
            }
            else
            {
                RequireIDFAOther();
            }
        }

        void RequireIDFA1405()
        {
            // 14.5及以上版本，如果审核中，则弹没有tips的权限申请，否则弹有tips的申请
            if (_isIOSVerifing)
            {
                StartCoroutine(StartRequireIDFA(false));
            }
            else
            {
                StartCoroutine(StartRequireIDFA(true));
            }
        }

        void RequireIDFAOther()
        {
            // < 14.5的版本，如果在审核中，则弹权限框，否则直接跳过权限申请
            if (_isIOSVerifing)
            {
                StartCoroutine(StartRequireIDFA(false));
            }
            else
            {
                _executor.OnChildTaskCompleted();
            }
        }

        IEnumerator StartRequireIDFA(bool showIDFATips)
        {
            Debug.Log("Start Require IDFA: " + showIDFATips);
            if (showIDFATips)
            {
                EventManager.Instance.Dispatch(new object[] { Event.ShowIDFATip });
                _executor.OnChildTaskPaused();
                yield return new WaitForSeconds(0.1f);
            }

            Platform.RequireIDFA();

            if (showIDFATips)
            {
                EventManager.Instance.Dispatch(new object[] { Event.HideIDFATip });
            }

            yield return new WaitForSeconds(0);
            _executor.OnChildTaskResumed();
            _executor.OnChildTaskCompleted();
        }
#else
        public override void Run(TaskExecutor executor)
        {
            Debug.Log(GetTaskName() + " Ignored");
            executor.OnChildTaskCompleted();
        }
#endif
    }
}