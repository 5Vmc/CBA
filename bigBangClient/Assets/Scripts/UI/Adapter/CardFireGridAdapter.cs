using System.Collections.Generic;
using BigBang.Animation;
using Com.TheFallenGames.OSA.CustomAdapters.GridView;
using Com.TheFallenGames.OSA.DataHelpers;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace BigBang.UI
{
    public class CardFireGridAdapter : GridAdapter<GridParams, CardFireGridViewsHolder>
    {
        public SimpleDataHelper<PlayerCard> Data { get; private set; }

        private List<PlayerCard> _cardList;

        protected override void Start()
        {
            Data = new SimpleDataHelper<PlayerCard>(this);
            base.Start();
            SetData(_cardList);
        }
#if UNITY_WEBGL
        protected override bool IsRecyclable(CellGroupViewsHolder<CardFireGridViewsHolder> potentiallyRecyclable, int indexOfItemThatWillBecomeVisible, double sizeOfItemThatWillBecomeVisible)
        {
            return potentiallyRecyclable.ItemIndex == indexOfItemThatWillBecomeVisible;
        }
#endif
        public void SetData(List<PlayerCard> cardList)
        {
            _cardList = cardList;
            if (!IsInitialized) return;
            if (cardList is null)
            {
                cardList = new List<PlayerCard>();
            }
            Data.ResetItems(cardList);
            
        }

        public void RefreshUI(IList<PlayerCard> delList=null)
        {
            if(delList != null){
                foreach(PlayerCard card in delList){
                    _cardList.Remove(card);
                }
            }
            Data.ResetItems(_cardList);
            //Data.NotifyListChangedExternally();
        }


        protected override void UpdateCellViewsHolder(CardFireGridViewsHolder newOrRecycled)
        {
            var model = Data[newOrRecycled.ItemIndex];
            newOrRecycled.UpdateViews(model);
        }
    }


    public class CardFireGridViewsHolder : CellViewsHolder
    {
        //点击按钮
        private Button btn;
        private PlayerCard _card;
        private SmallCardItem cardItem;
        //private CardItemAnim _anim;

        public override void CollectViews()
        {
            base.CollectViews();
            cardItem = root.GetComponent<SmallCardItem>();
           
            //_anim = root.GetComponent<CardItemAnim>();

           // btn = views.GetComponent<Button>();
           // btn.onClick.AddListener(OnClickCard);
        }

        private void OnClickCard()
        {
            
        }

        public void UpdateViews(PlayerCard card)
        {
            _card = card;

            cardItem.SetData(_card);
        }

       
    }
}