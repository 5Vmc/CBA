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
    public class HundredRewardUIAdapter : OSA<HundredRewardParams, HundredRewardViewsHolder>
    {
        public SimpleDataHelper<HundredRewardConfig> Data { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            Data = new SimpleDataHelper<HundredRewardConfig>(this);
        }

        protected override void UpdateViewsHolder(HundredRewardViewsHolder newOrRecycled)
        {
            var model = Data[newOrRecycled.ItemIndex];
            newOrRecycled.UpdateViews(model, newOrRecycled.ItemIndex);
        }
#if UNITY_WEBGL
        protected override bool IsRecyclable(HundredRewardViewsHolder potentiallyRecyclable, int indexOfItemThatWillBecomeVisible, double sizeOfItemThatWillBecomeVisible)
        {
            return potentiallyRecyclable.ItemIndex == indexOfItemThatWillBecomeVisible;
        }
#endif

        public void SetData(HundredProgress hundredProgress)
        {
            if (!IsInitialized) Init();
            List<HundredRewardConfig> result = Configs.HundredReward.GetConfigList().Where((item) => item.Type == (int)hundredProgress).ToList();
            Data.ResetItems(result);
        }

        protected override HundredRewardViewsHolder CreateViewsHolder(int itemIndex)
        {
            var instance = new HundredRewardViewsHolder();
            instance.Init(_Params.prefab, _Params.Content, itemIndex);
            return instance;
        }
    }

    [Serializable]
    public class HundredRewardParams : BaseParams
    {
        public GameObject prefab;
    }

    public class HundredRewardViewsHolder : BaseItemViewsHolder
    {
        private HundredRewardItem item;

        public override void CollectViews()
        {
            base.CollectViews();
            item = root.GetComponent<HundredRewardItem>();
        }

        public void UpdateViews(HundredRewardConfig hundredRewardConfig, int index)
        {
            item.SetData(hundredRewardConfig, index);
        }
    }
}