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
    public class HundredDataUIFightItemAdapter : GridAdapter<HundredDataFightInfoParams, HundredDataFightGridViewsHolder>
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

        protected override void UpdateCellViewsHolder(HundredDataFightGridViewsHolder newOrRecycled)
        {
            var model = Data[newOrRecycled.ItemIndex];
            newOrRecycled.UpdateViews(model, newOrRecycled.ItemIndex);
        }

        public void SetSelect(HundredDataUIFightItem select)
        {
            for (int i = 0; i < Data.Count; i++)
            {
                var holder = GetCellViewsHolderIfVisible(i);
                if (holder != null)
                {
                    HundredDataUIFightItem hundredDataUIFightItem = holder.root.GetComponent<HundredDataUIFightItem>();
                    if (hundredDataUIFightItem == select)
                    {
                        hundredDataUIFightItem.hundredDataUIFightItem.GetComponent<Image>().color = new Color(0, 1, 0, 1);
                    }
                    else
                    {
                        hundredDataUIFightItem.hundredDataUIFightItem.GetComponent<Image>().color = new Color(1, 1, 1, 1);
                    }
                }
            }
        }
    }

    [System.Serializable]
    public class HundredDataFightInfoParams : GridParams { }

    public class HundredDataFightGridViewsHolder : CellViewsHolder
    {
        private HundredDataUIFightItem cardItem;

        public override void CollectViews()
        {
            base.CollectViews();
            cardItem = root.GetComponent<HundredDataUIFightItem>();
        }

        public void UpdateViews(string info, int index)
        {
            cardItem.SetData(info, index);
        }
    }
}