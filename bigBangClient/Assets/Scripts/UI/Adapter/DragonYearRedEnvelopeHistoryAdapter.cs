using System.Collections.Generic;
using BigBang.Animation;
using Com.TheFallenGames.OSA.Core;
using Com.TheFallenGames.OSA.CustomParams;
using Com.TheFallenGames.OSA.DataHelpers;
using DG.Tweening;
using Protocol;
using UnityEngine;
using Utils;

namespace BigBang.UI
{
    public class DragonYearRedEnvelopeHistoryAdapter : OSA<BaseParamsWithPrefab, DragonYearRedEnvelopeHistoryViewsHolder>
    {
        public SimpleDataHelper<RedPacketLogInfo> Data { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            Data = new SimpleDataHelper<RedPacketLogInfo>(this);
        }
#if UNITY_WEBGL
        protected override bool IsRecyclable(DragonYearRedEnvelopeHistoryViewsHolder potentiallyRecyclable, int indexOfItemThatWillBecomeVisible, double sizeOfItemThatWillBecomeVisible)
        {
            return potentiallyRecyclable.ItemIndex == indexOfItemThatWillBecomeVisible;
        }
#endif 
        public void SetData(List<RedPacketLogInfo> data)
        {
            if (!IsInitialized) Init();
            Data.ResetItems(data);
        }

        protected override DragonYearRedEnvelopeHistoryViewsHolder CreateViewsHolder(int itemIndex)
        {
            var instance = new DragonYearRedEnvelopeHistoryViewsHolder();
            instance.Init(_Params.ItemPrefab, _Params.Content, itemIndex);
            return instance;
        }

        protected override void UpdateViewsHolder(DragonYearRedEnvelopeHistoryViewsHolder newOrRecycled)
        {
            newOrRecycled.UpdateViews(Data[newOrRecycled.ItemIndex]);
        }
    }

    public class DragonYearRedEnvelopeHistoryViewsHolder : BaseItemViewsHolder
    {
        private DragonYearRedEnvelopeHistoryItem item;

        public override void CollectViews()
        {
            base.CollectViews();
            item = root.GetComponent<DragonYearRedEnvelopeHistoryItem>();
        }

        public void UpdateViews(RedPacketLogInfo redPacketLogInfo)
        {
            item.SetData(redPacketLogInfo);
        }
    }
}
