using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using DG.Tweening;
using System;
using Coffee.UIEffects;
using BigBang.UI;
using UnityTimer;

namespace BigBang.Animation
{
    public class ClassicCountryUIAnim : AnimBase
    {
        [SerializeField] private RectTransform topTitle;
        [SerializeField] private RectTransform bottomRect;
        [SerializeField] private RectTransform titles;
        [SerializeField] private ClassicTaskProgressItem taskProgressItem;
        [SerializeField] private ClassicTeamItemAdapter adapter;

        public override void Init()
        {
            base.Init();
            adapter.InitAnim();
            taskProgressItem.Anim.Init();
            // 初始化位置
            topTitle.SetAnchoredPositionY(0);
            bottomRect.SetAnchoredPositionY(73);
            // 初始化透明度
            topTitle.gameObject.SetAlpha(0);
            bottomRect.gameObject.DOFade(0, 0.3f);
        }

        public void PlayEnter(int currStar, int maxStar)
        {
            Init();
            AudioManager.Instance.PlaySound(AudioNames.ENT_REG);
            TouchManager.Instance.DisableTouch();
            Timer.Register(this.gameObject, 1, TouchManager.Instance.EnableTouch);
            // 顶部栏下移
            topTitle.DoRelativeAnchorPosY(200, 0.3f).From();
            // 顶部栏淡入
            topTitle.gameObject.DOFade(1, 0.3f);
            // 底部栏上移
            bottomRect.DoRelativeAnchorPosY(-100, 0.3f).From();
            // 底部栏淡入
            bottomRect.gameObject.DOFade(1, 0.3f).OnComplete(() =>
            {
                // 标题淡入
                titles.gameObject.DOFade(1, 0.2f).OnComplete(() =>
                {
                    // 任务块放大
                    adapter.PlayAnim();
                });
                taskProgressItem.Anim.PlayAnim(currStar, maxStar, 0.1f);
            });
        }

        public override void PlayExit(Action callback)
        {
            adapter.PlayExit();
            taskProgressItem.Anim.PlayExit();
            titles.gameObject.DOFade(0, 0.2f);
            topTitle.DORelativePositionY(200, 0.2f);
            bottomRect.gameObject.DOFade(0, 0.2f).OnComplete(() => callback?.Invoke());
        }
    }
}
