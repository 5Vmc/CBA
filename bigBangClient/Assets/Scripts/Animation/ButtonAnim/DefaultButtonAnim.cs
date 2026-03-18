using System;
using UnityEngine;
using DG.Tweening;

namespace BigBang.Animation
{
    public class DefaultButtonAnim : IBabuButtonAnim
    {
        private static DefaultButtonAnim instance;
        public static DefaultButtonAnim Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new DefaultButtonAnim();
                }
                return instance;
            }
        }

        public bool IsPlaying { get => isPlaying; }

        private bool isPlaying = false;

        Tween tweenSmall = null;
        Tween tweenNormal = null;
        Transform transform = null;
        public void Play(Transform transform, Action callback)
        {
            this.transform = transform;
            ClearAnim();
            isPlaying = true;
            tweenSmall = transform.DOScale(0.8f, 0.05f).OnComplete(() =>
            {
                tweenNormal = transform.DOScale(1, 0.05f).OnComplete(() =>
                {
                    isPlaying = false;
                    try
                    {
                        callback?.Invoke();
                    }
                    catch (Exception e)
                    {
                        if (transform != null)
                            Debug.LogWarningFormat("DefaultButtonAnim , Play , transform = {0} , " + e, transform.name);
                        else
                            Debug.LogWarningFormat("DefaultButtonAnim , Play , transform = null , " + e);
                    }
                });
            });
        }

        public void ClearAnim()
        {
            tweenSmall?.Kill();
            tweenSmall = null;
            tweenNormal?.Kill();
            tweenNormal = null;
        }
        public void ResetAnim()
        {
            ClearAnim();
            if (transform != null) transform.localScale = Vector3.one;
        }
    }
}