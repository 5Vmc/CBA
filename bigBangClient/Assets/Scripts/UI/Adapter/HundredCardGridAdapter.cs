using System.Collections.Generic;
using Babu;
using BigBang.Animation;
using Com.TheFallenGames.OSA.CustomAdapters.GridView;
using Com.TheFallenGames.OSA.DataHelpers;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace BigBang.UI
{
    public class HundredCardGridAdapter : GridAdapter<HundredCardInfoParams, HundredCardGridViewsHolder>
    {
        public SimpleDataHelper<HundredCardData> Data { get; private set; }

        private List<HundredCardData> _cardList;

        protected override void Start()
        {
            Data = new SimpleDataHelper<HundredCardData>(this);
            base.Start();
            SetData(_cardList);
        }
        //#if UNITY_WEBGL 
        //        protected override bool IsRecyclable(CellGroupViewsHolder<HundredCardGridViewsHolder> potentiallyRecyclable, int indexOfItemThatWillBecomeVisible, double sizeOfItemThatWillBecomeVisible)
        //        {
        //            return potentiallyRecyclable.ItemIndex == indexOfItemThatWillBecomeVisible;
        //        }
        //#endif
        public void SetData(List<HundredCardData> cardList)
        {
            _cardList = cardList;
            if (!IsInitialized) return;
            if (cardList is null)
            {
                cardList = new List<HundredCardData>();
            }
            Data.ResetItems(cardList);
        }

        protected override void UpdateCellViewsHolder(HundredCardGridViewsHolder newOrRecycled)
        {
            var model = Data[newOrRecycled.ItemIndex];
            newOrRecycled.UpdateViews(model);
        }
    }

    [System.Serializable]
    public class HundredCardInfoParams : GridParams { }

    public class HundredCardGridViewsHolder : CellViewsHolder
    {
        private HundredCardItem cardItem;

        public override void CollectViews()
        {
            base.CollectViews();
            cardItem = root.GetComponent<HundredCardItem>();
        }

        public void UpdateViews(HundredCardData card)
        {
            cardItem.SetData(card);
        }
    }
}