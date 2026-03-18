using BigBang.UI;
using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace BigBang.Animation
{
    public class LeaguePlayerRankUIAnim : AnimBase
    {
        [SerializeField] private RectTransform topBar = null;
        [SerializeField] private RectTransform bottomBar = null;
        [SerializeField] private LeaguePlayerIntegralAdapter adapter;

        public void InitTopBottomBar()
        {
            topBar.SetAnchoredPositionY(UICommon.TopBarHideY);
            bottomBar.SetAnchoredPositionY(-55f);
        }

        public void PlayEnterTopBottomBar()
        {
            tweens.Add(topBar.DOAnchorPosY(UICommon.TopBarShowY, 0.3f));
            tweens.Add(bottomBar.DOAnchorPosY(194f, 0.3f));
        }

        public void InitAdapter()
        {
            adapter.InitAnim();
        }

        public void PlayEnterAdapter(bool needWait)
        {
            //TouchManager.Instance.DisableTouch();
            adapter.PlayAnim();
            //Timer.Register(this.gameObject, adapter.VisibleItemsCount * 0.03f, TouchManager.Instance.EnableTouch);
        }

    }
}
