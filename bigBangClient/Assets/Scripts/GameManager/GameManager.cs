using System;
using Babu;
using Babu.SDK;
using BigBang.UI;
using UnityEngine;
using static Babu.SDK.SDKAntiAddiction;

namespace BigBang
{
    public class GameManager : BabuSingleton<GameManager>
    {
        private bool gameInBackground = false;
        private long enterBackgroundTime = 0;
        private long enterForegroundTime = 0;
        private void OnApplicationPause(bool focus)
        {
            if (focus)
            {
                Debug.Log("进入后台");
                gameInBackground = true;
                enterBackgroundTime = TimeUtils.Now();
                ReportPlayTimeToReYun();
            }
            else
            {
                Debug.Log("进入前台");
                gameInBackground = false;
                enterForegroundTime = TimeUtils.Now();

                //if (enterBackgroundTime > 0 && (TimeUtils.Now() - enterBackgroundTime) > 20)
                //{
                //    //超过一定时间，需要重新登陆
                //    //todo 
                //}

                //LoginManager.Instance.CheckSilenceReLogin();
            }
        }

        private void ReportPlayTimeToReYun()
        {
            if (enterBackgroundTime == 0 || enterForegroundTime == 0) return;
        }

        private void OnApplicationFocus(bool focus)
        {
            if (focus)
            {
                Debug.Log("获得焦点");
                LoginManager.Instance.CheckSilenceReLogin();
                EventManager.Instance.Dispatch(EventID.OnApplicationFocusTrue);
            }
            else
            {
                Debug.Log("失去焦点");
            }
        }

        void Start()
        {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
#if UNITY_WEBGL
            Debug.unityLogger.logEnabled = false;
#endif

            //实名认证由quick完成

            // 防沉谜事件;
            // 弹出防沉迷谜框;
            //EventManager.Instance.Register(SDKAntiAddiction.Event.ShowRealnameVerifyPanel, (args) =>
            //{
            //    Debug.Log("OnShowRealnameVerifyPanel");
            //    UIController.Instance.OpenWindow<RealnameVerifyUI>();
            //});

            //// 防沉迷启动;
            //EventManager.Instance.Register(SDKAntiAddiction.Event.AntiAddiction, (args) =>
            //{
            //    Debug.Log("OnAntiAddiction");
            //    UIController.Instance.OpenWindow<AntiAddictionUI>();
            //});

            //// 防沉迷失败;
            //EventManager.Instance.Register(SDKAntiAddiction.Event.AntiAddictionFailed, (args) =>
            //{
            //    Debug.LogError("AntiAddictionFailed!!!");
            //    UIController.Instance.OpenWindow<AntiAddictionUI>();
            //});

            // 实名认证成功;
            //EventManager.Instance.Register(SDKAntiAddiction.Event.RealnameVerifyResult, (args) =>
            //{
            //    Debug.Log("OnRealnameVerifyResult");
            //    if ((int)args[0] == RealnameVerifyErrorCode.Succ)
            //    {
            //        LoginManager.Instance.SaveRealnameInfo();
            //    }
            //});
        }

        /// <summary>
        /// 首次抽到橙卡(抽卡次数 50 次)
        /// 七日签到领取了一个球员
        /// </summary>
        public void TrigIosShopReview()
        {
#if UNITY_IOS || UNITY_EDITOR
            bool needShowStoreReview = PlayerPrefs.GetInt(PlayerPrefsKeys.NeedShowStoreReview, 1) == 1;
            if (needShowStoreReview)
            {
                UIController.Instance.OpenWindow<StoreReviewUI>();
            }
#endif
        }

        public static void InitManager()
        {
            Player.TrainManager = new PlayerTrainManager();
            Player.PackageManager = new PlayerPackageManager();
            Player.CardManager = new PlayerCardManager();
            Player.FightManager = new PlayerFightManager();
            Player.EmailManager = new PlayerEmailManager();
            Player.ChallengeManager = new PlayerChallengeManager();
            Player.ShopManager = new PlayerShopManager();
            Player.TaskManager = new PlayerTaskManager();
            Player.ActivityManager = new ActivityManager();
            Player.OnoffManager = new PlayerOnoffManager();
            Player.PVPManager = new PlayerPVPManager();
            Player.AchievementManager = new PlayerAchievementManager();
            Player.BattleManager = new BattleManager();
            Player.NoviceTaskManager = new NoviceTaskManager();

            Player.TrainManager.Init();
            Player.CardManager.Init();
            Player.PackageManager.Init();
            Player.FightManager.Init();
            Player.ActivityManager.Init();
            Player.ShopManager.Init();
            Player.EmailManager.Init();
            CollectionManager.Instance.Clear();
        }
    }
}
