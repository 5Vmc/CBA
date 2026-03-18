using UnityEngine;
using Com.TheFallenGames.OSA.DataHelpers;
using System.Collections.Generic;
using Com.TheFallenGames.OSA.Core;
using System;
using GameConfig;
using GameConfig.Config;
using System.Linq;
using Protocol;

namespace BigBang.UI
{
    public class DragonBoatRankUIAdapter : OSA<DragonBoatRankParams, DragonBoatRankViewsHolder>
    {
        public SimpleDataHelper<AllStarRankInfo> Data { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            Data = new SimpleDataHelper<AllStarRankInfo>(this);
        }

        protected override void UpdateViewsHolder(DragonBoatRankViewsHolder newOrRecycled)
        {
            var model = Data[newOrRecycled.ItemIndex];
            newOrRecycled.UpdateViews(model, newOrRecycled.ItemIndex);
        }
#if UNITY_WEBGL
        protected override bool IsRecyclable(DragonBoatRankViewsHolder potentiallyRecyclable, int indexOfItemThatWillBecomeVisible, double sizeOfItemThatWillBecomeVisible)
        {
            return potentiallyRecyclable.ItemIndex == indexOfItemThatWillBecomeVisible;
        }
#endif

        public void SetData(List<AllStarRankInfo> allStarRankInfoList)
        {
            if (!IsInitialized) Init();
            List<AllStarRankInfo> result = allStarRankInfoList;
            Data.ResetItems(result);
            for (int i = 0; i < Data.Count; ++i)
            {
                if (result[i].Gbid == Player.GbId)
                {
                    ScrollTo(i);
                    return;
                }
            }
        }

        protected override DragonBoatRankViewsHolder CreateViewsHolder(int itemIndex)
        {
            var instance = new DragonBoatRankViewsHolder();
            instance.Init(_Params.prefab, _Params.Content, itemIndex);
            return instance;
        }
    }

    [Serializable]
    public class DragonBoatRankParams : BaseParams
    {
        public GameObject prefab;
    }

    public class DragonBoatRankViewsHolder : BaseItemViewsHolder
    {
        private DragonBoatRankItem item;

        public override void CollectViews()
        {
            base.CollectViews();
            item = root.GetComponent<DragonBoatRankItem>();
        }

        public void UpdateViews(AllStarRankInfo allStarRankInfo, int index)
        {
            item.SetData(allStarRankInfo, index);
        }
    }
}