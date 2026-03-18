using BigBang.UI;
using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace BigBang.Animation
{
    public class LeagueHistoryUIAnim : AnimBase
    {
        [SerializeField] private RectTransform topBar = null;
        [SerializeField] private LeagueHistoryAdapter adapter;

        public void InitTopBar()
        {
            topBar.SetAnchoredPositionY(UICommon.TopBarHideY);
        }

        public void PlayEnterTopBar()
        {
            tweens.Add(topBar.DOAnchorPosY(UICommon.TopBarShowY, 0.3f));
        }

        public void InitAdapter()
        {
            for (int i = 0; i < adapter.VisibleItemsCount; i++)
            {
                var holder = adapter.GetItemViewsHolder(i);
                holder.InitAnim();
            }
        }

        public void PlayEnterAdapter(bool needWait)
        {
            //TouchManager.Instance.DisableTouch();
            for (int i = 0; i < adapter.VisibleItemsCount; i++)
            {
                var holder = adapter.GetItemViewsHolder(i);
                float waitTime = needWait ? 0.15f : 0f;
                holder.PlayAnim(waitTime + i * 0.03f);
            }
            //Timer.Register(this.gameObject, adapter.VisibleItemsCount * 0.03f, TouchManager.Instance.EnableTouch);
        }

    }
}
