using Babu;
using BigBang.UI;
using Com.TheFallenGames.OSA.Core;
using Com.TheFallenGames.OSA.CustomParams;
using Com.TheFallenGames.OSA.DataHelpers;
using DG.Tweening;
using Protocol;
using System.Collections.Generic;
using UnityTimer;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using BigBang.Animation;
namespace BigBang.UI
{
    public class ArenaRecordListAdapter :
        OSA<BaseParamsWithPrefab, ArenaRecordItemViewsHolder>
    {

        public SimpleDataHelper<Protocol.ArenaLogInfo> Data { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            Data = new SimpleDataHelper<ArenaLogInfo>(this);
        }
#if UNITY_WEBGL
        protected override bool IsRecyclable(ArenaRecordItemViewsHolder potentiallyRecyclable, int indexOfItemThatWillBecomeVisible, double sizeOfItemThatWillBecomeVisible)
        {
            return potentiallyRecyclable.ItemIndex == indexOfItemThatWillBecomeVisible;
        }
#endif
        public void SetItems(IList<ArenaLogInfo> items)
        {
            if (!IsInitialized) Init();
           
            Data.ResetItems(items);
            
        }
        protected override ArenaRecordItemViewsHolder CreateViewsHolder(int itemIndex)
        {
            var instance = new ArenaRecordItemViewsHolder();

            instance.Init(_Params.ItemPrefab, _Params.Content, itemIndex);

            //instance.ItemComponent.SelectListener = this;
            return instance;
        }

        protected override void UpdateViewsHolder(ArenaRecordItemViewsHolder newOrRecycled)
        {
            var modelData = Data[newOrRecycled.ItemIndex];

            newOrRecycled.UpdateViews(modelData);
            
        }
    }

    public class ArenaRecordItemViewsHolder : BaseItemViewsHolder
    {

        public ArenaRecordItem ItemComponent;
        public override void CollectViews()
        {
            base.CollectViews();
            ItemComponent = root.GetComponent<ArenaRecordItem>();
        }

        public void UpdateViews(ArenaLogInfo data)
        {
            ItemComponent.SetData(data);
        }
    }
}
