using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace BigBang.Animation
{
    public class LeagueRankUIAnim : AnimBase
    {
        [SerializeField] private RectTransform topBar = null;
        [SerializeField] private List<GameObject> panelAlphaList = new();

        public override void Init()
        {
            base.Init();
            // 初始化位置
            topBar.SetAnchoredPositionY(UICommon.TopBarHideY);
            foreach (var item in panelAlphaList)
            {
                item.SetAlpha(0);
            }
        }

        public override void PlayEnter()
        {
            base.PlayEnter();
        }

        public void PlayEnterTop()
        {
            tweens.Add(topBar.DOAnchorPosY(UICommon.TopBarShowY, 0.2f));
        }

        public void PlayEnterAlpha()
        {
            foreach (var item in panelAlphaList)
            {
                tweens.Add(item.DOFade(1f, 0.2f).SetDelay(0.1f));
            }
        }
    }
}
