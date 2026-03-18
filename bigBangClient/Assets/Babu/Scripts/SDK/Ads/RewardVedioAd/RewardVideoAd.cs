using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

namespace Babu.SDK
{
    // 激励视频广告基类
    public abstract class RewardVideoAd
    {
        protected string _from;
        protected Action<object[]> _completeCallback;
        protected object[] _args;

        protected int _loadAdFailedCount = 0;       // 连续加载广告错误次数;
        protected int _lastLoadAdErrorId = 0;       // 最后一次加载广告错误id;

        public enum Error
        {
            UNKNOWN = -1,
            OK = 0,

            NOT_READY = 200
        }

        public virtual void ShowAd(string from, Action<object[]> completeCallback, object[] args)
        {
            _from = from;
            _completeCallback = completeCallback;
            _args = args;

            if (IsReady() == false)
            {
                // 点击显示广告的时候进行报错;
                if (_lastLoadAdErrorId != 0)
                {
                    EventManager.Instance.Dispatch(RewardVideoAdManager.Event.REWARD_VIDEO_AD_SHOW_ERROR, _lastLoadAdErrorId);
                }
                else
                {
                    EventManager.Instance.Dispatch(RewardVideoAdManager.Event.REWARD_VIDEO_AD_SHOW_ERROR, (int)Error.NOT_READY);
                }
            }
        }

        public abstract bool IsReady();
        public abstract void Init();
        protected abstract void LoadAd();

        public virtual void Test()
        {

        }

        protected void OnAdPrepareSucc(Dictionary<string, object> ext = null)
        {
            if (MainThreadTaskService.Instance.IsMainThread() == false)
            {
                MainThreadTaskService.Instance.Run(() =>
                {
                    Debug.LogError("Reward Ad Callback(OnAdPrepareSucc) Not In Main Thread");
                });
            }

            Debug.Log("OnAdPrepareSucc");

            AppEventManager.Instance.RewardVideoAdPrepareSucc(ext);
            EventManager.Instance.Dispatch(RewardVideoAdManager.Event.REWARD_VIDEO_AD_READY);

            _lastLoadAdErrorId = 0;
            _loadAdFailedCount = 0;
        }

        protected void OnAdPrepareFailed(int errorId, string desc, Dictionary<string, object> ext = null)
        {
            if (MainThreadTaskService.Instance.IsMainThread() == false)
            {
                MainThreadTaskService.Instance.Run(() =>
                {
                    Debug.LogError("Reward Ad Callback(OnAdPrepareFailed) Not In Main Thread");
                });
            }

            Debug.Log($"OnAdPrepareFailed: [{errorId}]: {desc}");

            AppEventManager.Instance.RewardVideoAdPrepareFiled(errorId, desc, ext);
            _lastLoadAdErrorId = errorId;
            ++_loadAdFailedCount;

            // 重新尝试加载广告
            DelayTaskService.Instance.RunWithNoBindGameObject((float)Math.Min(Math.Pow(5, _loadAdFailedCount), 120), LoadAd);
        }

        protected void OnAdOpened(Dictionary<string, object> ext = null)
        {
            if (MainThreadTaskService.Instance.IsMainThread() == false)
            {
                MainThreadTaskService.Instance.Run(() =>
                {
                    Debug.LogError("Reward Ad Callback(OnAdOpened) Not In Main Thread");
                });
            }

            Debug.Log("OnAdOpened");

            AppEventManager.Instance.RewardVideoAdOpened(_from, ext);
            EventManager.Instance.Dispatch(RewardVideoAdManager.Event.REWARD_VIDEO_AD_OPEN);
        }

        protected void OnAdClosed(Dictionary<string, object> ext = null)
        {
            if (MainThreadTaskService.Instance.IsMainThread() == false)
            {
                MainThreadTaskService.Instance.Run(() =>
                {
                    Debug.LogError("Reward Ad Callback(OnAdClosed) Not In Main Thread");
                });
            }

            Debug.Log("OnAdClosed");

            AppEventManager.Instance.RewardVideoAdClosed(_from, ext);
            EventManager.Instance.Dispatch(RewardVideoAdManager.Event.REWARD_VIDEO_AD_CLOSE);

            // 重新加载新的广告
            DelayTaskService.Instance.RunWithNoBindGameObject(0.5f, LoadAd);
        }

        protected void OnAdError(int errorId, string desc, Dictionary<string, object> ext = null)
        {
            if (MainThreadTaskService.Instance.IsMainThread() == false)
            {
                MainThreadTaskService.Instance.Run(() =>
                {
                    Debug.LogError("Reward Ad Callback(OnAdError) Not In Main Thread");
                });
            }

            Debug.Log($"OnAdError: [{errorId}]: {desc}");

            AppEventManager.Instance.RewardVideoAdError(_from, errorId, desc, ext);
        }

        protected void OnAdClicked(Dictionary<string, object> ext = null)
        {
            if (MainThreadTaskService.Instance.IsMainThread() == false)
            {
                MainThreadTaskService.Instance.Run(() =>
                {
                    Debug.LogError("Reward Ad Callback(OnAdClicked) Not In Main Thread");
                });
            }

            Debug.Log("OnAdClicked");

            AppEventManager.Instance.RewardVideoAdClicked(_from, ext);
        }

        protected void OnAdRewarded(Dictionary<string, object> ext = null)
        {
            if (MainThreadTaskService.Instance.IsMainThread() == false)
            {
                MainThreadTaskService.Instance.Run(() =>
                {
                    Debug.LogError("Reward Ad Callback(OnAdRewarded) Not In Main Thread");
                });
            }

            Debug.Log("OnAdRewarded");

            AppEventManager.Instance.RewardVideoAdRewarded(_from, ext);
            _completeCallback?.Invoke(_args);
        }
    }
}
