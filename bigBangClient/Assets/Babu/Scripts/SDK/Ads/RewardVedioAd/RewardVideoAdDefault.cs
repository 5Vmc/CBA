using System;
using UnityEngine;

namespace Babu.SDK
{
    public class RewardVideoAdDefault : RewardVideoAd
    {
        public override void ShowAd(string from, Action<object[]> complateCallback, object[] args)
        {
            base.ShowAd(from, complateCallback, args);

            Debug.Log("RewardVideoAdUnityEditor: showRewardedVideo");
            OnAdPrepareSucc();
            OnAdOpened();
            OnAdRewarded();
            OnAdClosed();
        }

        public override bool IsReady()
        {
            return true;
        }

        public override void Init()
        {
        }

        protected override void LoadAd()
        {
        }
    }
}
