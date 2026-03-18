using System;
using UnityEngine;
using DG.Tweening;

namespace BigBang.Animation
{
    public class LongPressButtonAnim : IBabuButtonAnim
    {
        private static LongPressButtonAnim instance;
        public static LongPressButtonAnim Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new LongPressButtonAnim();
                }
                return instance;
            }
        }

        public bool IsPlaying { get => isPlaying; }

        private bool isPlaying = false;

        Tween tweenBig = null;
        Tween tweenNormal = null;
        Transform transform = null;
        public void Play(Transform transform, Action callback)
        {
            this.transform = transform;
            ClearAnim();
            isPlaying = true;
            tweenBig = transform.DOScale(1.1f, 0.05f).OnComplete(() =>
            {
                tweenNormal = transform.DOScale(1, 0.05f).OnComplete(() =>
                {
                    callback?.Invoke();
                    isPlaying = false;
                });
            });
        }

        public void ClearAnim()
        {
            tweenBig?.Kill();
            tweenBig = null;
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