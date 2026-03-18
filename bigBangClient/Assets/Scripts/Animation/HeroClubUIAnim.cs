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
    public class HeroClubUIAnim : AnimBase
    {
        [SerializeField] private RectTransform topHeroPanel;
        [SerializeField] private RectTransform topTitle;
        [SerializeField] private RectTransform bottomRect;
        [SerializeField] private RectTransform titles;
        [SerializeField] private ClassicTaskProgressItem taskProgressItem;
        [SerializeField] private HeroClubItemAdapter adapter;
        [SerializeField] private RectTransform teamPanel = null;

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
            bottomRect.gameObject.SetAlpha(0);
            titles.gameObject.SetAlpha(0);
            topHeroPanel.gameObject.SetAlpha(0);
            teamPanel.gameObject.SetAlpha(0);
        }

        public override void PlayEnter()
        {
            isAnimSuccess = false;
            Init();
            AudioManager.Instance.PlaySound(AudioNames.ENT_REG);
            TouchManager.Instance.DisableTouch();
            Timer.Register(this.gameObject, 1, TouchManager.Instance.EnableTouch);
            // 顶部栏下移
            tweens.Add(topTitle.DoRelativeAnchorPosY(200, 0.3f).From());
            // 顶部栏淡入
            tweens.Add(topTitle.gameObject.DOFade(1, 0.3f));
            // 底部栏上移
            tweens.Add(bottomRect.DoRelativeAnchorPosY(-100, 0.3f).From());
            // 底部栏淡入
            tweens.Add(bottomRect.gameObject.DOFade(1, 0.3f).OnComplete(() =>
            {
                isAnimSuccess = true;
                CheckPlayDataAni();
            }));
            tweens.Add(topHeroPanel.gameObject.DOFade(1, 0.7f));
            tweens.Add(teamPanel.gameObject.DOFade(1, 1.3f));
        }

        public override void PlayExit(Action callback)
        {
            isDataSetSuccess = false;
            isAnimSuccess = false;
            adapter.PlayExit();
            taskProgressItem.Anim.PlayExit();
            tweens.Add(titles.gameObject.DOFade(0, 0.2f));
            tweens.Add(topHeroPanel.gameObject.DOFade(0, 0.2f));
            tweens.Add(topTitle.DORelativePositionY(200, 0.2f));
            tweens.Add(bottomRect.gameObject.DOFade(0, 0.2f).OnComplete(() => callback?.Invoke()));
            tweens.Add(teamPanel.gameObject.DOFade(0, 0.15f));
        }

        bool isDataSetSuccess = false;
        int currStar;
        int maxStar;
        public void SetDataSuccess(bool isSuccess, int currStar = 0, int maxStar = 0)
        {
            isDataSetSuccess = isSuccess;
            this.currStar = currStar;
            this.maxStar = maxStar;
            CheckPlayDataAni();
        }

        bool isAnimSuccess = false;
        private void CheckPlayDataAni()
        {
            if (isDataSetSuccess == false) return;
            if (isAnimSuccess == false) return;

            // 标题淡入
            tweens.Add(titles.gameObject.DOFade(1, 0.2f).OnComplete(() =>
            {
                // 任务块放大
                adapter.PlayAnim();
            }));
            taskProgressItem.Anim.PlayAnim(currStar, maxStar, 0.1f);
        }

    }
}
