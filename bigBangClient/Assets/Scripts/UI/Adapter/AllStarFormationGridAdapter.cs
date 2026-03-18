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
    public class AllStarFormationGridAdapter : GridAdapter<AllStarFormationInfoParams, AllStarFormationGridViewsHolder>
    {
        public SimpleDataHelper<PlayerCard> Data { get; private set; }

        private List<PlayerCard> _cardList;

        protected override void Start()
        {
            Data = new SimpleDataHelper<PlayerCard>(this);
            base.Start();
            SetData(_cardList, selectPosition);
        }
        //#if UNITY_WEBGL 
        //        protected override bool IsRecyclable(CellGroupViewsHolder<AllStarFormationGridViewsHolder> potentiallyRecyclable, int indexOfItemThatWillBecomeVisible, double sizeOfItemThatWillBecomeVisible)
        //        {
        //            return potentiallyRecyclable.ItemIndex == indexOfItemThatWillBecomeVisible;
        //        }
        //#endif
        private PositionSeparatedType selectPosition = PositionSeparatedType.All;
        public void SetData(List<PlayerCard> cardList, PositionSeparatedType selectPosition)
        {
            this.selectPosition = selectPosition;
            _cardList = cardList;
            if (!IsInitialized) return;
            if (cardList is null)
            {
                cardList = new List<PlayerCard>();
            }
            Data.ResetItems(cardList);
        }

        protected override void UpdateCellViewsHolder(AllStarFormationGridViewsHolder newOrRecycled)
        {
            var model = Data[newOrRecycled.ItemIndex];
            newOrRecycled.UpdateViews(model, selectPosition);
        }
    }

    [System.Serializable]
    public class AllStarFormationInfoParams : GridParams { }

    public class AllStarFormationGridViewsHolder : CellViewsHolder
    {
        private AllStarFormationCardItem cardItem;

        public override void CollectViews()
        {
            base.CollectViews();
            cardItem = root.GetComponent<AllStarFormationCardItem>();
        }

        public void UpdateViews(PlayerCard card, PositionSeparatedType selectPosition)
        {
            cardItem.SetData(card, false, selectPosition);
        }
    }
}