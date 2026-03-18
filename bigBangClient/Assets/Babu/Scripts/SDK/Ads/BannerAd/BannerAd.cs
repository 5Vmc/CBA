using System.Collections.Generic;
using System;
using UnityEngine;

namespace Babu.SDK
{
    public abstract class BannerAd
    {
        protected int _loadAdFailedCount = 0;       // 连续加载广告错误次数;

        public abstract bool IsReady();
        
        public abstract void Init(Vector2 adPos, Vector2 adSize = new Vector2());

        public abstract void UpdatePos(Vector2 adPos);

        public abstract Rect GetLayout();

        protected abstract void LoadAd();
        
        public abstract void ShowAd();
        
        public abstract void HideAd();

        protected void OnAdPrepareSucc(Dictionary<string, object> ext = null)
        {
            if (MainThreadTaskService.Instance.IsMainThread() == false)
            {
                MainThreadTaskService.Instance.Run(() =>
                {
                    Debug.LogError("Banner Ad Callback(OnAdPrepareSucc) Not In Main Thread");
                });
            }

            Debug.Log("OnAdPrepareSucc");
            _loadAdFailedCount = 0;

            AppEventManager.Instance.BannerAdPrepareSucc(ext);
        }

        protected void OnAdPrepareFailed(int errorId, string desc, Dictionary<string, object> ext = null)
        {
            if (MainThreadTaskService.Instance.IsMainThread() == false)
            {
                MainThreadTaskService.Instance.Run(() =>
                {
                    Debug.LogError("Banner Ad Callback(OnAdPrepareFailed) Not In Main Thread");
                });
            }

            Debug.Log($"OnAdPrepareFailed: [{errorId}]: {desc}");
            AppEventManager.Instance.BannerAdPrepareFiled(errorId, desc, ext);
        }

        protected void OnAdOpened(Dictionary<string, object> ext = null)
        {
            if (MainThreadTaskService.Instance.IsMainThread() == false)
            {
                MainThreadTaskService.Instance.Run(() =>
                {
                    Debug.LogError("Banner Ad Callback(OnAdOpened) Not In Main Thread");
                });
            }

            Debug.Log("OnAdOpened");
            AppEventManager.Instance.BannerAdOpened(ext);
        }

        protected void OnAdClosed(Dictionary<string, object> ext = null)
        {
            if (MainThreadTaskService.Instance.IsMainThread() == false)
            {
                MainThreadTaskService.Instance.Run(() =>
                {
                    Debug.LogError("Banner Ad Callback(OnAdClosed) Not In Main Thread");
                });
            }

            Debug.Log("OnAdClosed");
            AppEventManager.Instance.BannerAdClosed(ext);
        }
    }
}
