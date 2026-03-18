using System;

namespace Babu.SDK
{
    public class RewardVideoAdManager : Task
    {
        public static RewardVideoAdManager Instance;

        public class Event
        {
            public const string REWARD_VIDEO_AD_READY = "REWARD_VIDEO_AD_READY";            // 激励广告准备好了;
            public const string REWARD_VIDEO_AD_OPEN = "REWARD_VIDEO_AD_OPEN";              // 激励广告被打开;
            public const string REWARD_VIDEO_AD_CLOSE = "REWARD_VIDEO_AD_CLOSE";            // 激励广告被关闭;
            public const string REWARD_VIDEO_AD_SHOW_ERROR = "REWARD_VIDEO_AD_SHOW_ERROR";  // 激励广告显示错误;
        }

        protected RewardVideoAd _rewardVideoAd = new RewardVideoAdDefault();

        public override string GetTaskName()
        {
            return "RewardVideoAdManager";
        }

        public void SetRewardVideoAdHandler(RewardVideoAd rewardVideoAd)
        {
            _rewardVideoAd = rewardVideoAd;
        }

        public bool IsReady { get => _rewardVideoAd.IsReady(); }

        void Awake()
        {
            Instance = this;
        }

        public override void Run(TaskExecutor executor)
        {
            _rewardVideoAd.Init();
            executor.OnChildTaskCompleted();
        }

        public void ShowRewardVideoAd(string from, Action<object[]> complateCallback, params object[] args)
        {
            AppEventManager.Instance.RewardVideoAdButtonClicked(from);
            _rewardVideoAd.ShowAd(from, complateCallback, args);
        }

        public void Test()
        {
            _rewardVideoAd.Test();
        }
    }
}
