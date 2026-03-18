using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace BigBang.Animation
{
    public class LeagueHomeUIAnim : AnimBase
    {
        [SerializeField] private RectTransform topBar = null;

        [SerializeField] private RectTransform timePanel = null;
        [SerializeField] private Image cupImage = null;
        [SerializeField] private RectTransform signBtnPanel = null;
        [SerializeField] private RectTransform rightBtnLayout = null;


        [SerializeField] private RectTransform matchPanel = null;
        [SerializeField] private RectTransform rankPanel = null;

        public override void Init()
        {
            base.Init();
            // 初始化位置
            topBar.SetAnchoredPositionY(UICommon.TopBarHideY);

            timePanel.SetLocalScale(0.6f);
            timePanel.gameObject.SetAlpha(0);
            cupImage.SetAlpha(0);
            signBtnPanel.gameObject.SetAlpha(0);
            rightBtnLayout.gameObject.SetAlpha(0);

            matchPanel.SetLocalScaleY(0.6f);
            matchPanel.gameObject.SetAlpha(0);
            rankPanel.SetLocalScaleY(0.6f);
            rankPanel.gameObject.SetAlpha(0);
        }

        public void PlayEnter(bool isSignStage, bool needMoveTop = true)
        {
            base.PlayEnter();

            if (needMoveTop)
                tweens.Add(topBar.DOAnchorPosY(UICommon.TopBarShowY, 0.3f));
            else
                topBar.SetAnchoredPositionY(UICommon.TopBarShowY);

            if (isSignStage)
            {
                tweens.Add(timePanel.DOScale(1f, 0.6f).SetEase(Ease.OutBack));
                tweens.Add(timePanel.gameObject.DOFade(1f, 0.4f));
                tweens.Add(cupImage.DOFade(1f, 0.4f));
                tweens.Add(signBtnPanel.gameObject.DOFade(1f, 0.4f));
                tweens.Add(rightBtnLayout.gameObject.DOFade(1f, 0.4f));
            }
            else
            {
                float delayTime = needMoveTop ? 0.15f : 0f;
                tweens.Add(matchPanel.DOScaleY(1f, 0.5f).SetEase(Ease.OutBack).SetDelay(delayTime));
                tweens.Add(matchPanel.gameObject.DOFade(1f, 0.4f).SetDelay(delayTime));
                tweens.Add(rankPanel.DOScaleY(1f, 0.5f).SetEase(Ease.OutBack).SetDelay(delayTime + 0.15f));
                tweens.Add(rankPanel.gameObject.DOFade(1f, 0.4f).SetDelay(delayTime + 0.15f));
            }

        }
    }
}
