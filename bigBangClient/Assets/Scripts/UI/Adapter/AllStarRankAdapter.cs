using UnityEngine;
using Com.TheFallenGames.OSA.DataHelpers;
using Protocol;
using System.Collections.Generic;
using Com.TheFallenGames.OSA.Core;
using System;
using DG.Tweening;

namespace BigBang.UI
{
    public class AllStarRankAdapter : OSA<AllStarRankParams, AllStarRankViewsHolder>
    {
        public SimpleDataHelper<AllStarRankInfo> Data { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            Data = new SimpleDataHelper<AllStarRankInfo>(this);
        }

        protected override void UpdateViewsHolder(AllStarRankViewsHolder newOrRecycled)
        {
            newOrRecycled.UpdateViews(Data[newOrRecycled.ItemIndex], newOrRecycled.ItemIndex);
        }
#if UNITY_WEBGL
        protected override bool IsRecyclable(AllStarRankViewsHolder potentiallyRecyclable, int indexOfItemThatWillBecomeVisible, double sizeOfItemThatWillBecomeVisible)
        {
            return potentiallyRecyclable.ItemIndex == indexOfItemThatWillBecomeVisible;
        }
#endif 
        public void SetData(List<AllStarRankInfo> data)
        {
            if (!IsInitialized) Init();
            Data.ResetItems(data);
            for (int i = 0; i < Data.Count; ++i)
            {
                if (data[i].Gbid == Player.GbId)
                {
                    ScrollTo(Mathf.Max(0, i - VisibleItemsCount / 2));
                    return;
                }
            }
        }

        protected override AllStarRankViewsHolder CreateViewsHolder(int itemIndex)
        {
            var instance = new AllStarRankViewsHolder();
            instance.Init(_Params.prefab, _Params.Content, itemIndex);
            return instance;
        }
    }

    [Serializable]
    public class AllStarRankParams : BaseParams
    {
        public GameObject prefab;
    }

    public class AllStarRankViewsHolder : BaseItemViewsHolder
    {
        private AllStarRankItem item;

        public override void CollectViews()
        {
            base.CollectViews();
            item = root.GetComponent<AllStarRankItem>();
        }

        public void UpdateViews(AllStarRankInfo allStarRankInfo, int index)
        {
            item.SetData(allStarRankInfo, false, index);
        }

        public void InitAnim()
        {
            root.transform.localScale = new Vector3(1, 0, 1);
        }

        public Tweener PlayAnim(float delay)
        {
            return root.transform.DOScale(Vector3.one, 0.15f).SetDelay(delay);
        }
    }
}