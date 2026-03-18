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
    public class ServerListAdapter :
        OSA<BaseParamsWithPrefab, ServerItemViewsHolder>
    {

        public SimpleDataHelper<ServerData> Data { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            Data = new SimpleDataHelper<ServerData>(this);
        }

        public void SetItems(IList<ServerData> items)
        {
            if (!IsInitialized) Init();
           
            Data.ResetItems(items);
            
        }
#if UNITY_WEBGL
        protected override bool IsRecyclable(ServerItemViewsHolder potentiallyRecyclable, int indexOfItemThatWillBecomeVisible, double sizeOfItemThatWillBecomeVisible)
        {
            return potentiallyRecyclable.ItemIndex == indexOfItemThatWillBecomeVisible;
        }
#endif 
        protected override ServerItemViewsHolder CreateViewsHolder(int itemIndex)
        {
            var instance = new ServerItemViewsHolder();

            instance.Init(_Params.ItemPrefab, _Params.Content, itemIndex);

            //instance.ItemComponent.SelectListener = this;
            return instance;
        }

        protected override void UpdateViewsHolder(ServerItemViewsHolder newOrRecycled)
        {
            var modelData = Data[newOrRecycled.ItemIndex];

            newOrRecycled.UpdateViews(modelData);
            
        }
    }

    public class ServerItemViewsHolder : BaseItemViewsHolder
    {

        public ServerListItem ItemComponent;
        public override void CollectViews()
        {
            base.CollectViews();
            ItemComponent = root.GetComponent<ServerListItem>();
        }

        public void UpdateViews(ServerData data)
        {
            ItemComponent.SetData(data);
        }
    }
}
