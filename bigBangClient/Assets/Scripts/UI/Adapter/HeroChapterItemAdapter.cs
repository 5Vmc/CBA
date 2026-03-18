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

namespace BigBang.UI
{
    public class HeroChapterItemAdapter : OSA<HeroChapterItemParams, HeroChapterItemHolder>
    {
        public SimpleDataHelper<HeroChapterData> Data { get; private set; }

        private List<HeroChapterData> heroChapterDataList;

        protected override void Awake()
        {
            base.Awake();
            Data = new SimpleDataHelper<HeroChapterData>(this);
        }

        protected override void UpdateViewsHolder(HeroChapterItemHolder newOrRecycled)
        {
            var model = Data[newOrRecycled.ItemIndex];
            newOrRecycled.UpdateViews(model);
        }
#if UNITY_WEBGL 
        protected override bool IsRecyclable(HeroChapterItemHolder potentiallyRecyclable, int indexOfItemThatWillBecomeVisible, double sizeOfItemThatWillBecomeVisible)
        {
            return potentiallyRecyclable.ItemIndex == indexOfItemThatWillBecomeVisible;
        }
#endif 

        public void SetData(List<HeroChapterData> heroChapterDataList)
        {
            this.heroChapterDataList = heroChapterDataList;
            if (!IsInitialized) Init();
            Data.ResetItems(this.heroChapterDataList);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
        }

        protected override HeroChapterItemHolder CreateViewsHolder(int itemIndex)
        {
            var instance = new HeroChapterItemHolder();
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
    public class HeroChapterItemParams : BaseParams
    {
        public GameObject prefab;
    }

    public class HeroChapterItemHolder : BaseItemViewsHolder
    {
        private HeroChapterItem item;

        public override void CollectViews()
        {
            base.CollectViews();
            item = root.GetComponent<HeroChapterItem>();
        }

        public void UpdateViews(HeroChapterData data)
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