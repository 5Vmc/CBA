using UnityEngine;
using Com.TheFallenGames.OSA.DataHelpers;
using System.Collections.Generic;
using Com.TheFallenGames.OSA.Core;
using System;
using System.Linq;
using BigBang.Animation;
using DG.Tweening;
using Utils;
using UnityTimer;
using GameConfig.Config;
using Babu;
using static BigBang.HeroManager;
using static BigBang.BountyTaskManager;

namespace BigBang.UI
{
    public class BountyTaskAdapter : OSA<BountyTaskItemParams, BountyTaskItemHolder>
    {
        public SimpleDataHelper<BountyTaskData> Data { get; private set; }

        private List<BountyTaskData> heroClubDataList;

        protected override void Awake()
        {
            base.Awake();
            Data = new SimpleDataHelper<BountyTaskData>(this);
        }

        protected override void UpdateViewsHolder(BountyTaskItemHolder newOrRecycled)
        {
            var model = Data[newOrRecycled.ItemIndex];
            newOrRecycled.UpdateViews(model);
        }
#if UNITY_WEBGL
        protected override bool IsRecyclable(BountyTaskItemHolder potentiallyRecyclable, int indexOfItemThatWillBecomeVisible, double sizeOfItemThatWillBecomeVisible)
        {
            return potentiallyRecyclable.ItemIndex == indexOfItemThatWillBecomeVisible;
        }
#endif 
        public void SetData(List<BountyTaskData> heroClubDataList)
        {
            this.heroClubDataList = heroClubDataList;
            if (enabled == false) return;
            if (!IsInitialized) Init();
            Data.ResetItems(this.heroClubDataList);
        }
        public void ScrollToTop()
        {
            this.SetNormalizedPosition(1);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
        }

        protected override BountyTaskItemHolder CreateViewsHolder(int itemIndex)
        {
            var instance = new BountyTaskItemHolder();
            instance.Init(_Params.prefab, _Params.Content, itemIndex);
            return instance;
        }

        // 播放动画
        public void PlayAnim()
        {
            for (int i = 0; i < VisibleItemsCount; i++)
            {
                if (i < 4)
                {
                    Timer.Register(this.gameObject, i * 0.1f, () => AudioManager.Instance.PlaySound(AudioNames.ENT_FLOP));
                }
                GetItemViewsHolder(i).PlayAnim(i * 0.1f);
            }
        }

        public void InitAnim()
        {
            for (int i = 0; i < VisibleItemsCount; i++)
            {
                GetItemViewsHolder(i).InitAnim();
            }
        }

        public void PlayExit()
        {
            for (int i = 0; i < VisibleItemsCount; i++)
            {
                GetItemViewsHolder(i).PlayExit();
            }
        }
    }

    [Serializable]
    public class BountyTaskItemParams : BaseParams
    {
        public GameObject prefab;
    }

    public class BountyTaskItemHolder : BaseItemViewsHolder
    {
        private BountyTaskItem item;

        public override void CollectViews()
        {
            base.CollectViews();
            item = root.GetComponent<BountyTaskItem>();
        }

        public void UpdateViews(BountyTaskData data)
        {
            item.SetData(data);
        }

        Tween fadeTween;
        Tween scaleTween;
        // 播放动画
        public void PlayAnim(float delay)
        {
            fadeTween?.Kill();
            scaleTween?.Kill();
            fadeTween = root.gameObject.DOFade(1, 0.3f).SetDelay(delay);
            scaleTween = root.DOScale(1, 0.3f).SetDelay(delay);
        }

        public void InitAnim()
        {
            fadeTween?.Kill();
            scaleTween?.Kill();
            root.gameObject.SetAlpha(0);
            root.localScale = Vector3.one * 0.8f;
        }

        public void PlayExit()
        {
            fadeTween?.Kill();
            scaleTween?.Kill();
            fadeTween = root.gameObject.DOFade(0, 0.3f);
        }
    }
}