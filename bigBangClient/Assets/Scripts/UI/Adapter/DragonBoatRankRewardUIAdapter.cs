using UnityEngine;
using Com.TheFallenGames.OSA.DataHelpers;
using System.Collections.Generic;
using Com.TheFallenGames.OSA.Core;
using System;
using GameConfig;
using GameConfig.Config;
using System.Linq;

namespace BigBang.UI
{
    public class DragonBoatRankRewardUIAdapter : OSA<DragonBoatRankRewardParams, DragonBoatRankRewardViewsHolder>
    {
        public SimpleDataHelper<DragonBoatRewardConfig> Data { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            Data = new SimpleDataHelper<DragonBoatRewardConfig>(this);
        }

        protected override void UpdateViewsHolder(DragonBoatRankRewardViewsHolder newOrRecycled)
        {
            var model = Data[newOrRecycled.ItemIndex];
            newOrRecycled.UpdateViews(model, newOrRecycled.ItemIndex);
        }
#if UNITY_WEBGL
        protected override bool IsRecyclable(DragonBoatRankRewardViewsHolder potentiallyRecyclable, int indexOfItemThatWillBecomeVisible, double sizeOfItemThatWillBecomeVisible)
        {
            return potentiallyRecyclable.ItemIndex == indexOfItemThatWillBecomeVisible;
        }
#endif

        public void SetData()
        {
            if (!IsInitialized) Init();
            List<DragonBoatRewardConfig> result = Configs.DragonBoatReward.GetConfigList().Where((item) => item.Type == 1).ToList();
            Data.ResetItems(result);
        }

        protected override DragonBoatRankRewardViewsHolder CreateViewsHolder(int itemIndex)
        {
            var instance = new DragonBoatRankRewardViewsHolder();
            instance.Init(_Params.prefab, _Params.Content, itemIndex);
            return instance;
        }
    }

    [Serializable]
    public class DragonBoatRankRewardParams : BaseParams
    {
        public GameObject prefab;
    }

    public class DragonBoatRankRewardViewsHolder : BaseItemViewsHolder
    {
        private DragonBoatRankRewardItem item;

        public override void CollectViews()
        {
            base.CollectViews();
            item = root.GetComponent<DragonBoatRankRewardItem>();
        }

        public void UpdateViews(DragonBoatRewardConfig dragonBoatRewardConfig, int index)
        {
            item.SetData(dragonBoatRewardConfig, index);
        }
    }
}