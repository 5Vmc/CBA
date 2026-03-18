using UnityEngine;

namespace Babu.SDK
{
    public class BannerAdManager : Task
    {
        public static BannerAdManager Instance;

        protected BannerAd _bannerAd = new BannerAdDefault();

        public override string GetTaskName()
        {
            return "BannerAdManager";
        }

        void Awake()
        {
            Instance = this;
        }

        public override void Run(TaskExecutor executor)
        {
            executor.OnChildTaskCompleted();
        }

        public void SetBannerAdHandler(BannerAd bannerAd)
        {
            _bannerAd = bannerAd;
        }

        public bool IsReady { get => _bannerAd != null && _bannerAd.IsReady(); }

        public void Init(Vector2 adPos, Vector2 adSize = new Vector2())
        {
            _bannerAd?.Init(adPos, adSize);
        }

        public void UpdatePos(Vector2 adPos)
        {
            _bannerAd?.UpdatePos(adPos);
        }

        public Rect GetLayout()
        {
            if (_bannerAd == null)
            {
                return Rect.zero;
            }
            return _bannerAd.GetLayout();
        }

        public void ShowAd()
        {
            _bannerAd?.ShowAd();
        }

        public void HideAd()
        {
            _bannerAd?.HideAd();
        }
    }
}
