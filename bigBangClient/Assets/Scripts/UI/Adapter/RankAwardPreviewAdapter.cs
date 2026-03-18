using UnityEngine;
using Com.TheFallenGames.OSA.DataHelpers;
using System.Collections.Generic;
using Com.TheFallenGames.OSA.Core;
using System;
using GameConfig;
using GameConfig.Config;

namespace BigBang.UI
{
    public class RankAwardPreviewAdapter : OSA<RankAwardPreviewParams, RankAwardPreviewViewsHolder>
    {
        public SimpleDataHelper<ActivityTopRewardConfig> Data { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            Data = new SimpleDataHelper<ActivityTopRewardConfig>(this);
        }

        protected override void UpdateViewsHolder(RankAwardPreviewViewsHolder newOrRecycled)
        {
            var model = Data[newOrRecycled.ItemIndex];
            newOrRecycled.UpdateViews(model);
        }
#if UNITY_WEBGL
        protected override bool IsRecyclable(RankAwardPreviewViewsHolder potentiallyRecyclable, int indexOfItemThatWillBecomeVisible, double sizeOfItemThatWillBecomeVisible)
        {
            return potentiallyRecyclable.ItemIndex == indexOfItemThatWillBecomeVisible;
        }
#endif 
        // flag为true显示球队排名奖励；为false显示其他奖励
        public void SetData(List<ActivityTopRewardConfig> list)
        {
            if (!IsInitialized) Init();
            Data.ResetItems(list);
        }

        protected override RankAwardPreviewViewsHolder CreateViewsHolder(int itemIndex)
        {
            var instance = new RankAwardPreviewViewsHolder();
            instance.Init(_Params.prefab, _Params.Content, itemIndex);
            return instance;
        }
    }

    [Serializable]
    public class RankAwardPreviewParams : BaseParams
    {
        public GameObject prefab;
    }

    public class RankAwardPreviewViewsHolder : BaseItemViewsHolder
    {
        private RankAwardPreviewItem item;

        public override void CollectViews()
        {
            base.CollectViews();
            item = root.GetComponent<RankAwardPreviewItem>();
        }

        public void UpdateViews(ActivityTopRewardConfig model)
        {
            item.SetData(model);
        }
    }
}