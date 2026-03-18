using LightJson;
using System;
using UnityEngine;

namespace Babu.SDK
{
    public class RealnameInfo
    {
        public string Id;
        public string Name;
        public int Status;
        public int Year;
        public int Month;
        public int Day;
    }

    public class SDKAntiAddiction : Task
    {
        public static SDKAntiAddiction Instance;

        public class Event
        {
            public const string ShowRealnameVerifyPanel = "ShowRealnameVerifyPanel";      // 显示实名认证信息
            public const string RealnameVerify = "RealnameVerify";                        // 进行实名认证
            public const string RealnameVerifyResult = "RealnameVerifyResult";            // 实名认证结果
            public const string AntiAddiction = "AntiAddiction";                          // 防沉谜启动，弹出提示，关闭游戏
            public const string AntiAddictionFailed = "AntiAddictionFailed";              // 防沉迷失败，弹出提示，关闭游戏
        }

        public class RealnameVerifyErrorCode
        {
            public const int Succ = 0;
            public const int NetworkError = 1;      // 网络错误
            public const int InputInvalid = 2;      // 无效输入
        }

        public RealnameInfo RealnameInfo = new RealnameInfo();

        const string REALNAME_VERIFY_URL = "https://cn.api.babuyo.com/realname_verify/verify.php";
        const string ANTI_ADDICTION_URL = "https://cn.api.babuyo.com/realname_verify/anti_addiction.php";

        Coroutine _antiAddictionTask;

        enum RealnameStatus
        {
            UnVerify,       // 未实名;
            UnAdult,        // 未成年人;
            Adult           // 成年人;
        }

        private void Start()
        {
            Instance = this;
            EventManager.Instance.Register(Event.RealnameVerify, OnRealnameVerify);
        }

        public override string GetTaskName()
        {
            return "SDKRealnameVerify";
        }

        public void OnRealnameVerify(object[] args)
        {
            string name = (string)args[0];
            string id = (string)args[1];

            RealnameInfo.Name = name;
            RealnameInfo.Id = id;

            string url = REALNAME_VERIFY_URL + "?name=" + name + "&id=" + id;
            HttpService.Instance.AsyncGet(url, OnRealnameVerifyResponse, 10);
        }

        public void OnRealnameVerifyResponse(bool result, string response)
        {
            try
            {
                if (result == false)
                {
                    throw new Exception("Timeout");
                }

                JsonValue value = JsonValue.Parse(response);
                bool valid = value["valid"].AsBoolean;

                Debug.Log($"valid: {valid}");

                if (valid == false)
                {
                    EventManager.Instance.Dispatch(Event.RealnameVerifyResult, RealnameVerifyErrorCode.InputInvalid);
                    return;
                }

                bool isAdult = value["is_adult"].AsBoolean;

                //PlayerPrefs.SetInt("realname_status", (int)(isAdult ? RealnameStatus.Adult : RealnameStatus.UnAdult));
                RealnameInfo.Status = (int)(isAdult ? RealnameStatus.Adult : RealnameStatus.UnAdult);
                RealnameInfo.Year = value["birthday"]["year"].AsInteger;
                RealnameInfo.Month = value["birthday"]["month"].AsInteger;
                RealnameInfo.Day = value["birthday"]["day"].AsInteger;
                EventManager.Instance.Dispatch(Event.RealnameVerifyResult, RealnameVerifyErrorCode.Succ);
                if (isAdult == false)
                {
                    //PlayerPrefs.SetInt("birthday_year", value["birthday"]["year"].AsInteger);
                    //PlayerPrefs.SetInt("birthday_month", value["birthday"]["month"].AsInteger);
                    //PlayerPrefs.SetInt("birthday_day", value["birthday"]["day"].AsInteger);
                    CheckAntiAddiction();
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Request Realname Verify Failed: " + e.Message);
                EventManager.Instance.Dispatch(Event.RealnameVerifyResult, RealnameVerifyErrorCode.NetworkError);
            }
        }

        void CheckAntiAddiction()
        {
            //int year = PlayerPrefs.GetInt("birthday_year", 99999);
            //int month = PlayerPrefs.GetInt("birthday_month", 99999);
            //int day = PlayerPrefs.GetInt("birthday_day", 99999);
            int year = RealnameInfo.Year;
            int month = RealnameInfo.Month;
            int day = RealnameInfo.Day;
            string url = $"{ANTI_ADDICTION_URL}?year={year}&month={month}&day={day}";
            HttpService.Instance.AsyncGet(url, OnCheckAntiAddictionResponse, 10);
        }

        void OnCheckAntiAddictionResponse(bool result, string response)
        {
            try
            {
                if (result == false)
                {
                    throw new Exception("Timeout");
                }

                JsonValue value = JsonValue.Parse(response);
                bool isAdult = value["is_adult"].AsBoolean;
                if (isAdult)
                {
                    // 已成年;
                    // PlayerPrefs.SetInt("realname_status", (int)RealnameStatus.Adult);
                    RealnameInfo.Status = (int)RealnameStatus.Adult;
                    EventManager.Instance.Dispatch(Event.RealnameVerifyResult, RealnameVerifyErrorCode.Succ);
                    return;
                }

                bool pass = value["pass"].AsBoolean;
                int now = value["now"].AsInteger;

                Debug.Log($"OnCheckAntiAddictionResponse: pass={pass}, now={now}");

                if (pass == false)
                {
                    EventManager.Instance.Dispatch(Event.AntiAddiction);
                    return;
                }

                DateTime dateTime = TimeUtils.ToUtcDateTime(now);
                float second = (60 - dateTime.Minute) * 60;
                _antiAddictionTask = DelayTaskService.Instance.Run(this.gameObject, second, () =>
                {
                    CheckAntiAddiction();
                });
            }
            catch (Exception e)
            {
                Debug.LogError("Anti Addiction Failed: " + e.Message);
                EventManager.Instance.Dispatch(Event.AntiAddictionFailed);
            }
        }

        public override void Run(TaskExecutor executor)
        {
            //if (Environment.GetValue("enable_anti_addiction", true) && Environment.GetValue("use_sdk_anti_addiction", false) == false)
            //{
            //    RealnameStatus realnameStatus = (RealnameStatus)PlayerPrefs.GetInt("realname_status", 0);
            //    Debug.Log("RealnameStatus: " + realnameStatus);
            //    if (realnameStatus == RealnameStatus.UnVerify)
            //    {
            //        EventManager.Instance.Dispatch(Event.ShowRealnameVerifyPanel);
            //    }
            //    else if (realnameStatus == RealnameStatus.UnAdult)
            //    {
            //        // 时间验证;
            //        CheckAntiAddiction();
            //    }
            //}

            executor.OnChildTaskCompleted();
        }

        public void BeginCheck()
        {
            if (Environment.GetValue("enable_anti_addiction", true) && Environment.GetValue("use_sdk_anti_addiction", false) == false)
            {
                //RealnameStatus realnameStatus = (RealnameStatus)PlayerPrefs.GetInt("realname_status", 0);
                RealnameStatus realnameStatus = (RealnameStatus)RealnameInfo.Status;
                Debug.Log("RealnameStatus: " + realnameStatus);
                if (realnameStatus == RealnameStatus.UnVerify)
                {
                    EventManager.Instance.Dispatch(Event.ShowRealnameVerifyPanel);
                }
                else if (realnameStatus == RealnameStatus.UnAdult)
                {
                    // 时间验证;
                    CheckAntiAddiction();
                }
            }
        }

        private void OnApplicationFocus(bool focus)
        {
            Debug.Log($"SDKAntiAddiction OnApplicationFocus, {focus}");
            if (focus)
            {
                if (_antiAddictionTask != null)
                {
                    Debug.Log("CheckAntiAddiction");
                    DelayTaskService.Instance.StopCoroutine(_antiAddictionTask);
                    _antiAddictionTask = null;

                    CheckAntiAddiction();
                }
                else
                {
                    Debug.Log("No Need To CheckAntiAddiction");
                }
            }
        }
    }
}
