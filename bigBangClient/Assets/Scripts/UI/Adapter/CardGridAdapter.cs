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
    public class CardGridAdapter : GridAdapter<CardInfoParams, CardGridViewsHolder>
    {
        public SimpleDataHelper<PlayerCard> Data { get; private set; }

        private List<PlayerCard> _cardList;

        protected override void Start()
        {
            Data = new SimpleDataHelper<PlayerCard>(this);
            base.Start();
            SetData(_cardList);
        }
        //#if UNITY_WEBGL 
        //        protected override bool IsRecyclable(CellGroupViewsHolder<CardGridViewsHolder> potentiallyRecyclable, int indexOfItemThatWillBecomeVisible, double sizeOfItemThatWillBecomeVisible)
        //        {
        //            return potentiallyRecyclable.ItemIndex == indexOfItemThatWillBecomeVisible;
        //        }
        //#endif
        public void SetData(List<PlayerCard> cardList, bool playAnim = true)
        {
            _cardList = cardList;
            if (!IsInitialized) return;
            if (cardList is null)
            {
                cardList = new List<PlayerCard>();
            }
            Data.ResetItems(cardList);
            //if (!playAnim) return;
            if (CardUI.isFirstEnter)
            {
                PlayAnim();
                CardUI.isFirstEnter = false;
            }
            CardUI.isTurnCardOnce = false;
        }

        public void PlayAnim()
        {
            for (int i = 0; i < VisibleItemsCount; i++)
            {
                var groupVH = GetItemViewsHolder(i);

                for (int j = 0; j < groupVH.NumActiveCells; j++)
                {
                    var item = groupVH.ContainingCellViewsHolders[j];
                    item?.InitAnim();
                    item?.PlayEnter((i + 1) * 0.1f);
                }
            }
        }

        protected override void UpdateCellViewsHolder(CardGridViewsHolder newOrRecycled)
        {
            var model = Data[newOrRecycled.ItemIndex];
            newOrRecycled.UpdateViews(model, this.isUsingInCillectionUI, newOrRecycled.ItemIndex);
        }

        public void RefreshSelectCard()
        {
            for (int i = 0; i < VisibleItemsCount; i++)
            {
                var groupVH = GetItemViewsHolder(i);
                for (int j = 0; j < groupVH.NumActiveCells; j++)
                {
                    var item = groupVH.ContainingCellViewsHolders[j];
                    item.cardItem.RefreshSelect();
                }
            }
        }
        public void PlayHighlightAnim()
        {
            for (int i = 0; i < VisibleItemsCount; i++)
            {
                var groupVH = GetItemViewsHolder(i);
                for (int j = 0; j < groupVH.NumActiveCells; j++)
                {
                    var item = groupVH.ContainingCellViewsHolders[j];
                    item.cardItem.PlayHighlightAnim();
                }
            }
        }

        /// <summary>
        /// 用于数字藏品界面
        /// </summary>
        public bool isUsingInCillectionUI = false;
    }

    [System.Serializable]
    public class CardInfoParams : GridParams { }

    public class CardGridViewsHolder : CellViewsHolder
    {
        //点击按钮
        private Button btn;
        public PlayerCard _card;
        public CardItem cardItem;
        private CardItemAnim _anim;
        public bool isUsingInCillectionUI;
        public int index = -1;

        public override void CollectViews()
        {
            base.CollectViews();
            cardItem = root.GetComponent<CardItem>();

            _anim = root.GetComponent<CardItemAnim>();
            _anim.needResetOnDisable = true;

            btn = views.GetComponent<Button>();
            btn.onClick.AddListener(OnClickCard);
        }

        private void OnClickCard()
        {
            if (isUsingInCillectionUI)
            {
                EventManager.Instance.Dispatch(EventID.OnClickCollectionUICard, _card, index);

                return;
            }
            EventManager.Instance.Dispatch(EventID.OnClickCardUICard);
            TouchManager.Instance.DisableTouch();
            AudioManager.Instance.PlaySound(AudioNames.BTN_1);
            var clone = GameObject.Instantiate(cardItem.gameObject, CardUI.staticCenterPoint.parent).GetComponent<RectTransform>();
            clone.transform.position = cardItem.transform.position;
            cardItem.gameObject.SetAlpha(0);
            clone.SetPriority(2);
            // 卡片放大
            clone.DOScale(2f, 0.3f).OnComplete(() =>
            {

                clone.gameObject.SetAlpha(0);
                cardItem.gameObject.SetAlpha(1);
                GameObject.Destroy(clone.gameObject, 1);
                //CardUI.Anim.PlayExit();
                //CardUI.Anim.PlayExit(()=>
                //{
                //UIController.Instance.ShowPanel<CardDetailUI>(new CardDetailProperties(_card));//点击球员界面对应球员
                //});

                UIController.Instance.ShowPanel<CardUpUI>(new CardUpUIProperties(_card));
            }).AddTo(this.cardItem.gameObject);
            // 淡出
            clone.gameObject.DOFade(0, 0.2f).SetEase(Ease.Linear).AddTo(this.cardItem.gameObject);
            //Babu.DelayTaskService.Instance.Run(this.gameObject, 0.1f, () =>
            //{
            //    UIController.Instance.ShowPanel<CardDetailUI>(new CardDetailProperties(_card));
            //});
            Babu.DelayTaskService.Instance.Run(this.cardItem.gameObject, 0.6f, () =>
            {
                TouchManager.Instance.EnableTouch();
                CardUI.Anim.Init();
            });
            CardUI.Anim.PlayNext(null);
        }

        public void UpdateViews(PlayerCard card, bool isUsingInCillectionUI, int index)
        {
            this.index = index;
            this.isUsingInCillectionUI = isUsingInCillectionUI;
            _card = card;
            cardItem.isUsingInCillectionUI = isUsingInCillectionUI;
            cardItem.needShowStarterSign = true;
            cardItem.SetData(_card, true, true);

            if (isUsingInCillectionUI)
            {
                cardItem.transform.localScale = Vector3.one * 0.8f;
            }
        }

        public void PlayEnter(float delay)
        {
            _anim.PlayEnter(delay);
        }
        public void InitAnim()
        {
            _anim.Init();
        }
    }
}