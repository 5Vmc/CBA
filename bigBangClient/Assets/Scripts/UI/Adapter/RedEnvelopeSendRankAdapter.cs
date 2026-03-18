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
    public class RedEnvelopeSendRankAdapter : OSA<BaseParamsWithPrefab, RedEnvelopeSendRankViewsHolder>
    {
        public SimpleDataHelper<RedPacketRankInfo> Data { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            Data = new SimpleDataHelper<RedPacketRankInfo>(this);
        }
#if UNITY_WEBGL
        protected override bool IsRecyclable(RedEnvelopeSendRankViewsHolder potentiallyRecyclable, int indexOfItemThatWillBecomeVisible, double sizeOfItemThatWillBecomeVisible)
        {
            return potentiallyRecyclable.ItemIndex == indexOfItemThatWillBecomeVisible;
        }
#endif 
        public void SetData(List<RedPacketRankInfo> data)
        {
            if (!IsInitialized) Init();
            Data.ResetItems(data);
        }

        protected override RedEnvelopeSendRankViewsHolder CreateViewsHolder(int itemIndex)
        {
            var instance = new RedEnvelopeSendRankViewsHolder();
            instance.Init(_Params.ItemPrefab, _Params.Content, itemIndex);
            return instance;
        }

        protected override void UpdateViewsHolder(RedEnvelopeSendRankViewsHolder newOrRecycled)
        {
            newOrRecycled.UpdateViews(Data[newOrRecycled.ItemIndex]);
        }
    }

    public class RedEnvelopeSendRankViewsHolder : BaseItemViewsHolder
    {
        private RedEnvelopeSendRankItem item;

        public override void CollectViews()
        {
            base.CollectViews();
            item = root.GetComponent<RedEnvelopeSendRankItem>();
        }

        public void UpdateViews(RedPacketRankInfo redPacketRankInfo)
        {
            item.SetData(redPacketRankInfo);
        }
    }
}
