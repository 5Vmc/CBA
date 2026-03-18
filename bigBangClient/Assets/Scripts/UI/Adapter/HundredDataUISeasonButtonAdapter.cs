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
    public class HundredDataUISeasonButtonAdapter : GridAdapter<HundredDataInfoParams, HundredDataGridViewsHolder>
    {
        public SimpleDataHelper<string> Data { get; private set; }

        private List<string> _cardList;

        protected override void Start()
        {
            Data = new SimpleDataHelper<string>(this);
            base.Start();
            SetData(_cardList);
        }
        //#if UNITY_WEBGL 
        //        protected override bool IsRecyclable(CellGroupViewsHolder<HundredDataGridViewsHolder> potentiallyRecyclable, int indexOfItemThatWillBecomeVisible, double sizeOfItemThatWillBecomeVisible)
        //        {
        //            return potentiallyRecyclable.ItemIndex == indexOfItemThatWillBecomeVisible;
        //        }
        //#endif
        public void SetData(List<string> cardList)
        {
            _cardList = cardList;
            if (!IsInitialized) return;
            if (cardList is null)
            {
                cardList = new List<string>();
            }
            Data.ResetItems(cardList);
        }

        protected override void UpdateCellViewsHolder(HundredDataGridViewsHolder newOrRecycled)
        {
            var model = Data[newOrRecycled.ItemIndex];
            newOrRecycled.UpdateViews(model, newOrRecycled.ItemIndex);
        }

        public void SetSelect(HundredDataUISeasonButton select)
        {
            for (int i = 0; i < Data.Count; i++)
            {
                var holder = GetCellViewsHolderIfVisible(i);
                if (holder != null)
                {
                    HundredDataUISeasonButton hundredDataUISeasonButton = holder.root.GetComponent<HundredDataUISeasonButton>();
                    if (hundredDataUISeasonButton == select)
                    {
                        hundredDataUISeasonButton.hundredDataUISeasonButton.GetComponent<Image>().color = new Color(0, 1, 0, 1);
                    }
                    else
                    {
                        hundredDataUISeasonButton.hundredDataUISeasonButton.GetComponent<Image>().color = new Color(1, 1, 1, 1);
                    }
                }
            }
        }
        
    }

    [System.Serializable]
    public class HundredDataInfoParams : GridParams { }

    public class HundredDataGridViewsHolder : CellViewsHolder
    {
        private HundredDataUISeasonButton cardItem;

        public override void CollectViews()
        {
            base.CollectViews();
            cardItem = root.GetComponent<HundredDataUISeasonButton>();
        }

        public void UpdateViews(string info, int index)
        {
            cardItem.SetData(info, index);
        }
    }
}