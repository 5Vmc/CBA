
using UnityEngine;
using DG.Tweening;
using Utils;
using BigBang.UI;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityTimer;
using TMPro;
using System.Collections.Generic;
using System;

namespace BigBang.Animation
{
    public class BattleUI2Anim : AnimBase
    {
        [SerializeField] private RectTransform top;

        [SerializeField] private RectTransform bottom;
        [SerializeField] private RectTransform bottom2;

        [SerializeField] private RectTransform blueClubIconImage;
        [SerializeField] private RectTransform blueClubNameText;
        [SerializeField] private RectTransform blueDefNameText;
        [SerializeField] private RectTransform blueDefIconImage;
        [SerializeField] private RectTransform blueAtkNameText;
        [SerializeField] private RectTransform blueAtkIconImage;

        [SerializeField] private RectTransform redClubIconImage;
        [SerializeField] private RectTransform redClubNameText;
        [SerializeField] private RectTransform redDefNameText;
        [SerializeField] private RectTransform redDefIconImage;
        [SerializeField] private RectTransform redAtkNameText;
        [SerializeField] private RectTransform redAtkIconImage;

        public override void Init()
        {
            base.Init();

            top.gameObject.SetAlpha(0);

            bottom.gameObject.SetAlpha(0);
            bottom2.gameObject.SetAlpha(0);

            blueClubIconImage.gameObject.SetAlpha(0);
            blueClubNameText.gameObject.SetAlpha(0);
            blueDefNameText.gameObject.SetAlpha(0);
            blueDefIconImage.gameObject.SetAlpha(0);
            blueAtkNameText.gameObject.SetAlpha(0);
            blueAtkIconImage.gameObject.SetAlpha(0);

            redClubIconImage.gameObject.SetAlpha(0);
            redClubNameText.gameObject.SetAlpha(0);
            redDefNameText.gameObject.SetAlpha(0);
            redDefIconImage.gameObject.SetAlpha(0);
            redAtkNameText.gameObject.SetAlpha(0);
            redAtkIconImage.gameObject.SetAlpha(0);

        }

        private float firstWaitTime = 0.2f;

        private float topMoveDistance = 200;
        private float topInTime = 0.3f;

        private float topItemMoveDistance = 150;
        private float topItemInTime = 0.3f;
        private float topItemOffsetTime = 0.1f;
        private Ease topItemEase = Ease.OutBack;

        public void SetUIActive(bool value) {
            top.gameObject.SetActive(value);
            bottom.gameObject.SetActive(value);
            bottom2.gameObject.SetActive(value);
        }
        public override void PlayEnter()
        {
            base.PlayEnter();
            SetUIActive(true);
            // 顶部栏下移
            tweens.Add(top.DoRelativeAnchorPosY(topMoveDistance, topInTime).From().SetDelay(firstWaitTime));
            // 顶部栏淡入
            tweens.Add(top.gameObject.DOFade(1, topInTime).SetDelay(firstWaitTime));

            // 底部栏上移
            tweens.Add(bottom.DoRelativeAnchorPosY(-200, topInTime).From().SetDelay(firstWaitTime));
            // 底部栏淡入
            tweens.Add(bottom.gameObject.DOFade(1, topInTime).SetDelay(firstWaitTime));

            // 底部栏上移
            tweens.Add(bottom2.DoRelativeAnchorPosY(-200, topInTime).From().SetDelay(firstWaitTime));
            // 底部栏淡入
            tweens.Add(bottom2.gameObject.DOFade(1, topInTime).SetDelay(firstWaitTime));

            // 左侧顶栏内容从左侧飞入
            tweens.Add(blueClubIconImage.DoRelativeAnchorPosX(-topItemMoveDistance, topItemInTime).From().SetDelay(firstWaitTime + topInTime + topItemOffsetTime * 0).SetEase(topItemEase));
            tweens.Add(blueClubIconImage.gameObject.DOFade(1, topItemInTime).SetDelay(firstWaitTime + topInTime + topItemOffsetTime * 0).SetEase(topItemEase));
            tweens.Add(blueClubNameText.DoRelativeAnchorPosX(-topItemMoveDistance, topItemInTime).From().SetDelay(firstWaitTime + topInTime + topItemOffsetTime * 1).SetEase(topItemEase));
            tweens.Add(blueClubNameText.gameObject.DOFade(1, topItemInTime).SetDelay(firstWaitTime + topInTime + topItemOffsetTime * 1).SetEase(topItemEase));
            tweens.Add(blueDefNameText.DoRelativeAnchorPosX(-topItemMoveDistance, topItemInTime).From().SetDelay(firstWaitTime + topInTime + topItemOffsetTime * 3).SetEase(topItemEase));
            tweens.Add(blueDefNameText.gameObject.DOFade(1, topItemInTime).SetDelay(firstWaitTime + topInTime + topItemOffsetTime * 3).SetEase(topItemEase));
            tweens.Add(blueDefIconImage.DoRelativeAnchorPosX(-topItemMoveDistance, topItemInTime).From().SetDelay(firstWaitTime + topInTime + topItemOffsetTime * 3).SetEase(topItemEase));
            tweens.Add(blueDefIconImage.gameObject.DOFade(1, topItemInTime).SetDelay(firstWaitTime + topInTime + topItemOffsetTime * 3).SetEase(topItemEase));
            tweens.Add(blueAtkNameText.DoRelativeAnchorPosX(-topItemMoveDistance, topItemInTime).From().SetDelay(firstWaitTime + topInTime + topItemOffsetTime * 4).SetEase(topItemEase));
            tweens.Add(blueAtkNameText.gameObject.DOFade(1, topItemInTime).SetDelay(firstWaitTime + topInTime + topItemOffsetTime * 4).SetEase(topItemEase));
            tweens.Add(blueAtkIconImage.DoRelativeAnchorPosX(-topItemMoveDistance, topItemInTime).From().SetDelay(firstWaitTime + topInTime + topItemOffsetTime * 4).SetEase(topItemEase));
            tweens.Add(blueAtkIconImage.gameObject.DOFade(1, topItemInTime).SetDelay(firstWaitTime + topInTime + topItemOffsetTime * 4).SetEase(topItemEase));

            // 右侧侧顶栏内容从右侧飞入
            tweens.Add(redClubIconImage.DoRelativeAnchorPosX(topItemMoveDistance, topItemInTime).From().SetDelay(firstWaitTime + topInTime + topItemOffsetTime * 0).SetEase(topItemEase));
            tweens.Add(redClubIconImage.gameObject.DOFade(1, topItemInTime).SetDelay(firstWaitTime + topInTime + topItemOffsetTime * 0).SetEase(topItemEase));
            tweens.Add(redClubNameText.DoRelativeAnchorPosX(topItemMoveDistance, topItemInTime).From().SetDelay(firstWaitTime + topInTime + topItemOffsetTime * 1).SetEase(topItemEase));
            tweens.Add(redClubNameText.gameObject.DOFade(1, topItemInTime).SetDelay(firstWaitTime + topInTime + topItemOffsetTime * 1).SetEase(topItemEase));
            tweens.Add(redDefNameText.DoRelativeAnchorPosX(topItemMoveDistance, topItemInTime).From().SetDelay(firstWaitTime + topInTime + topItemOffsetTime * 3).SetEase(topItemEase));
            tweens.Add(redDefNameText.gameObject.DOFade(1, topItemInTime).SetDelay(firstWaitTime + topInTime + topItemOffsetTime * 3).SetEase(topItemEase));
            tweens.Add(redDefIconImage.DoRelativeAnchorPosX(topItemMoveDistance, topItemInTime).From().SetDelay(firstWaitTime + topInTime + topItemOffsetTime * 3).SetEase(topItemEase));
            tweens.Add(redDefIconImage.gameObject.DOFade(1, topItemInTime).SetDelay(firstWaitTime + topInTime + topItemOffsetTime * 3).SetEase(topItemEase));
            tweens.Add(redAtkNameText.DoRelativeAnchorPosX(topItemMoveDistance, topItemInTime).From().SetDelay(firstWaitTime + topInTime + topItemOffsetTime * 4).SetEase(topItemEase));
            tweens.Add(redAtkNameText.gameObject.DOFade(1, topItemInTime).SetDelay(firstWaitTime + topInTime + topItemOffsetTime * 4).SetEase(topItemEase));
            tweens.Add(redAtkIconImage.DoRelativeAnchorPosX(topItemMoveDistance, topItemInTime).From().SetDelay(firstWaitTime + topInTime + topItemOffsetTime * 4).SetEase(topItemEase));
            tweens.Add(redAtkIconImage.gameObject.DOFade(1, topItemInTime).SetDelay(firstWaitTime + topInTime + topItemOffsetTime * 4).SetEase(topItemEase));

            Sequence waitSeq = DOTween.Sequence();
            waitSeq.AppendCallback(() => { afterPlayEndCallBack?.Invoke(); });
            tweens.Add(waitSeq);
        }

        private Action afterPlayEndCallBack;
        public void SetData(Action afterPlayEndCallBack)
        {
            this.afterPlayEndCallBack = afterPlayEndCallBack;
        }
    }
}