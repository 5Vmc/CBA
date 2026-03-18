using UnityEngine;
using Com.TheFallenGames.OSA.DataHelpers;
using System.Collections.Generic;
using Com.TheFallenGames.OSA.Core;
using System;
using GameConfig;
using Com.TheFallenGames.OSA.CustomParams;
using GameConfig.Config;
using BigBang.Animation;
using DG.Tweening;
using Utils;
using UnityTimer;

namespace BigBang.UI
{
    public class ArenaRewardsUIAdapter : OSA<BaseParamsWithPrefab, ArenaRewardsViewsHolder>
    {
        public SimpleDataHelper<ArenaRewardsItemModel> Data { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            Data = new SimpleDataHelper<ArenaRewardsItemModel>(this);
        }
#if UNITY_WEBGL 
        protected override bool IsRecyclable(ArenaRewardsViewsHolder potentiallyRecyclable, int indexOfItemThatWillBecomeVisible, double sizeOfItemThatWillBecomeVisible)
        {
            return potentiallyRecyclable.ItemIndex == indexOfItemThatWillBecomeVisible;
        }
#endif
        protected override void UpdateViewsHolder(ArenaRewardsViewsHolder newOrRecycled)
        {
            var model = Data[newOrRecycled.ItemIndex];
           
            newOrRecycled.UpdateViews(model);
        }

        // flag为true显示球队排名奖励；为false显示其他奖励
        public void SetData(IList<ArenaRewardsItemModel> items)
        {
            if (!IsInitialized) Init();
            Data.ResetItems(items);

            this.InitAnim();
            this.PlayAnim(); 
        }



        protected override ArenaRewardsViewsHolder CreateViewsHolder(int itemIndex)
        {
            var instance = new ArenaRewardsViewsHolder();
            instance.Init(_Params.ItemPrefab, _Params.Content, itemIndex);
            return instance;
        }

        public void PlayAnim()
        {
            for (int i = 0; i < VisibleItemsCount; i++)
            {
                
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

   /* [Serializable]
    public class ArenaRewardsParams : BaseParams
    {
        public GameObject prefab;
    }*/

    public class ArenaRewardsItemModel
    {
        public ArenaStageRewardType Type;     // true显示排名奖，false显示其他奖
        public ArenaRewardConfig Data;
        
        public ArenaRewardsItemModel(ArenaStageRewardType type, ArenaRewardConfig data)
        {
            Type = type;
            Data = data;
            
        }
    }

    public class ArenaRewardsViewsHolder : BaseItemViewsHolder
    {
        private ArenaRewardsItem item;

        public override void CollectViews()
        {
            base.CollectViews();
            item = root.GetComponent<ArenaRewardsItem>();
        }

        public void UpdateViews(ArenaRewardsItemModel model)
        {
            item.SetData(model);
        }

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
            root.gameObject.DOFade(0, 0.3f);
        }
    }
}