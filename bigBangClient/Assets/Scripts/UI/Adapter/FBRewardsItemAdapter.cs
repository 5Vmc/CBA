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
using static BigBang.ClassicManager;
using Utils.GameItem;

namespace BigBang.UI
{
    public class FBRewardsItemAdapter : OSA<FBRewardsItemParams, FBRewardsItemHolder>
    {
        public SimpleDataHelper<FBRewardsItemData> Data { get; private set; }

        private List<FBRewardsItemData> DataSource;

        protected override void Awake()
        {
            base.Awake();
            Data = new SimpleDataHelper<FBRewardsItemData>(this);
        }
#if UNITY_WEBGL
        protected override bool IsRecyclable(FBRewardsItemHolder potentiallyRecyclable, int indexOfItemThatWillBecomeVisible, double sizeOfItemThatWillBecomeVisible)
        {
            return potentiallyRecyclable.ItemIndex == indexOfItemThatWillBecomeVisible;
        }
#endif 
        protected override void UpdateViewsHolder(FBRewardsItemHolder newOrRecycled)
        {
            var model = Data[newOrRecycled.ItemIndex];
            newOrRecycled.UpdateViews(model);
        }

        public void SetData(object dataList)
        {
            DataSource = (List<FBRewardsItemData>)dataList;
            if (!IsInitialized) Init();
            Data.ResetItems(this.DataSource);
            //PlayExit();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
        }

        protected override FBRewardsItemHolder CreateViewsHolder(int itemIndex)
        {
            var instance = new FBRewardsItemHolder();
            instance.Init(_Params.prefab, _Params.Content, itemIndex);
            return instance;
        }

        // 播放动画
        public void PlayAnim()
        {
            for (int i = 0; i < VisibleItemsCount; i++)
            {
                if (i < 3)
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
    public class FBRewardsItemParams : BaseParams
    {
        public GameObject prefab;
    }

    public class FBRewardsItemHolder : BaseItemViewsHolder
    {
        private FBRewardsItem item;

        public override void CollectViews()
        {
            base.CollectViews();
            item = root.GetComponent<FBRewardsItem>();
        }

        public void UpdateViews(FBRewardsItemData data)
        {
            item.SetData(data);
        }

        // 播放动画
        public void PlayAnim(float delay)
        {
            root.gameObject.DOFade(1, 0.3f).SetDelay(delay);
            root.DOScale(1, 0.3f).SetDelay(delay);
        }

        public void InitAnim()
        {
            root.gameObject.SetAlpha(0);
            root.localScale = Vector3.one * 0.8f;
        }

        public void PlayExit()
        {
            root.gameObject.SetAlpha(0);
        }
    }
}