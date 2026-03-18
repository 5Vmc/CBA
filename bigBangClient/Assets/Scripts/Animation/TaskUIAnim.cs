using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using Utils;
using BigBang.UI;
using System.Collections.Generic;
using UnityTimer;

namespace BigBang.Animation
{
    public class TaskUIAnim : AnimBase
    {
        [SerializeField] private RectTransform resourceTitle;
        [SerializeField] private RectTransform bottomRect;

        public override void Init()
        {
            base.Init();
            // 初始化位置
            resourceTitle.SetAnchoredPositionY(-60);
            bottomRect.SetAnchoredPositionY(73);
            // 初始化透明度
            resourceTitle.gameObject.SetAlpha(0);
            bottomRect.gameObject.DOFade(0, 0.3f);
        }

        public override void PlayEnter()
        {
            Init();
            AudioManager.Instance.PlaySound(AudioNames.ENT_REG);
            TouchManager.Instance.DisableTouch();
            Timer.Register(this.gameObject, 1, TouchManager.Instance.EnableTouch);
            // 顶部栏下移
            resourceTitle.DoRelativeAnchorPosY(200, 0.3f).From();
            // 顶部栏淡入
            resourceTitle.gameObject.DOFade(1, 0.3f);
            // 底部栏上移
            bottomRect.DoRelativeAnchorPosY(-100, 0.3f).From();
            // 底部栏淡入
            bottomRect.gameObject.DOFade(1, 0.3f).OnComplete(() =>
            {

            });
        }

        public override void PlayExit(Action callback)
        {
            resourceTitle.DORelativePositionY(200, 0.2f);
            bottomRect.gameObject.DOFade(0, 0.2f).OnComplete(() => callback?.Invoke());
        }
    }
}