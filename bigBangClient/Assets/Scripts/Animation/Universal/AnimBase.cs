using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace BigBang.Animation
{
    public abstract class AnimBase : MonoBehaviour
    {
        Lazy<List<Tween>> lazyList = new Lazy<List<Tween>>();

        protected List<Tween> tweens { get => lazyList.Value; }

        public virtual void Init()
        {
            ClearAnim();
        }

        /// <summary>
        /// 播放进入动画
        /// </summary>
        public virtual void PlayEnter()
        {
            Init();
        }

        /// <summary>
        /// 播放退出动画
        /// </summary>
        public virtual void PlayExit()
        {
            ClearAnim();
        }

        public virtual void PlayExit(Action callback)
        {
            ClearAnim();
        }

        protected virtual void ClearAnim()
        {
            if (!lazyList.IsValueCreated) return;
            tweens.ForEach(item => item.Kill());
            tweens.Clear();
        }

        private void OnDisable()
        {
            ClearAnim();
        }
    }
}
