

using Com.TheFallenGames.OSA.Core;
using Com.TheFallenGames.OSA.CustomParams;
using Com.TheFallenGames.OSA.DataHelpers;

using System.Collections.Generic;

using GameConfig.Config;
using UnityEngine;

namespace BigBang.UI
{
    public class DevelopGoodsAdapter :
        OSA<BaseParamsWithPrefab, GoodsItemViewsHolder>
    {

        public SimpleDataHelper<GoodsConfig> Data { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            Data = new SimpleDataHelper<GoodsConfig>(this);
        }
#if UNITY_WEBGL
        protected override bool IsRecyclable(GoodsItemViewsHolder potentiallyRecyclable, int indexOfItemThatWillBecomeVisible, double sizeOfItemThatWillBecomeVisible)
        {
            return potentiallyRecyclable.ItemIndex == indexOfItemThatWillBecomeVisible;
        }
#endif
        public void SetItems(IList<GoodsConfig> items)
        {
            
            if (!IsInitialized) Init();
           
            Data.ResetItems(items);
            
        }

        public void SearchItem(string name)
        {
            
            for(int i=0; i<Data.Count; i++)
            { 
                if(Data[i].Name == name){
                  
                    this.ScrollTo(i);
                    
                    return;
                }
            }

            for(int i=0; i<Data.Count; i++)
            { 
                if(Data[i].Name.IndexOf(name) >= 0){
                  
                    this.ScrollTo(i);
                    
                    return;
                }
            }
           
        }
        protected override GoodsItemViewsHolder CreateViewsHolder(int itemIndex)
        {
            var instance = new GoodsItemViewsHolder();

            instance.Init(_Params.ItemPrefab, _Params.Content, itemIndex);

            //instance.ItemComponent.SelectListener = this;
            return instance;
        }

        protected override void UpdateViewsHolder(GoodsItemViewsHolder newOrRecycled)
        {
            var modelData = Data[newOrRecycled.ItemIndex];

            newOrRecycled.UpdateViews(modelData);
            
        }
    }

    public class GoodsItemViewsHolder : BaseItemViewsHolder
    {

        public DevelopGoodsItem ItemComponent;
        public override void CollectViews()
        {
            base.CollectViews();
            ItemComponent = root.GetComponent<DevelopGoodsItem>();
        }

        public void UpdateViews(GoodsConfig data)
        {
            ItemComponent.SetData(data);
        }
    }
}
