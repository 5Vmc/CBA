using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Utils;
using System;
using System.Collections.Generic;
using BigBang.UI;
using UnityTimer;

namespace BigBang.Animation
{
    public class FBTowerShopUIAnim : AnimBase
    {
         [SerializeField] private RectTransform topTitle;
        [SerializeField] private RectTransform bottomRect;

        public override void Init()
        {
            base.Init();
            // 初始化位置
            topTitle.SetAnchoredPositionY(0);
            bottomRect.SetAnchoredPositionY(73);
            // 初始化透明度
            topTitle.gameObject.SetAlpha(0);
            bottomRect.gameObject.DOFade(0, 0.3f);
        }

        public override void PlayEnter()
        {
            Init();
            AudioManager.Instance.PlaySound(AudioNames.ENT_REG);
            // 顶部栏下移
            tweens.Add(topTitle.DoRelativeAnchorPosY(200, 0.3f).From());
            // 顶部栏淡入
            tweens.Add(topTitle.gameObject.DOFade(1, 0.3f));
            // 底部栏上移
            tweens.Add(bottomRect.DoRelativeAnchorPosY(-100, 0.3f).From());
            // 底部栏淡入
            tweens.Add(bottomRect.gameObject.DOFade(1, 0.3f).OnComplete(() =>
            {

            }));
        }
        public override void PlayExit(Action callback)
        {
            tweens.Add(topTitle.DORelativePositionY(200, 0.2f));
            tweens.Add(bottomRect.gameObject.DOFade(0, 0.2f).OnComplete(() => callback?.Invoke()));
        }
    }
}