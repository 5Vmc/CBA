using Babu;
using Com.TheFallenGames.OSA.Core;
using Com.TheFallenGames.OSA.CustomParams;
using Com.TheFallenGames.OSA.DataHelpers;
using DG.Tweening;
using frame8.Logic.Misc.Other.Extensions;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace BigBang.UI
{
    public enum LineupCardAdapterShowType
    {
        State,
        Data,
    }

    public class LineupCardListAdapter :
        OSA<BaseParamsWithPrefab, LineupCardItemViewsHolder>,
        LineupCardItem.ISelectListener
    {
        public SimpleDataHelper<PlayerCard> Data { get; private set; }

        private FormationBase _formation;

        //上一次显示类型
        // private LineupCardAdapterShowType _lastShowType = LineupCardAdapterShowType.State;
        private LineupCardAdapterShowType _showType = LineupCardAdapterShowType.State;
        private bool _selectModel = false;

        private PlayerCard _selectCard;
        private int _selectedCardIndex;

        #region OSA implementation

        protected override void Awake()
        {
            base.Awake();
            Data = new SimpleDataHelper<PlayerCard>(this);
        }

        protected override LineupCardItemViewsHolder CreateViewsHolder(int itemIndex)
        {
            var instance = new LineupCardItemViewsHolder();

            instance.Init(_Params.ItemPrefab, _Params.Content, itemIndex);

            instance.ItemComponent.SelectListener = this;
            return instance;
        }
#if UNITY_WEBGL
        protected override bool IsRecyclable(LineupCardItemViewsHolder potentiallyRecyclable, int indexOfItemThatWillBecomeVisible, double sizeOfItemThatWillBecomeVisible)
        {
            return potentiallyRecyclable.ItemIndex == indexOfItemThatWillBecomeVisible;
        }
#endif 
        protected override void UpdateViewsHolder(LineupCardItemViewsHolder newOrRecycled)
        {
            var model = Data[newOrRecycled.ItemIndex];

            newOrRecycled.UpdateViews(model, _formation);
            newOrRecycled.UpdateShowType(_showType);
            newOrRecycled.UpdateSelectModel(_selectModel, _selectCard);
        }

        #endregion

        #region data manipulation

        public void SetItems(IList<PlayerCard> items)
        {
            if (!IsInitialized) Init();
            Data.ResetItems(items);
            //AnimIn();
        }

        public void BindFormation(FormationBase formation)
        {
            _formation = formation;
        }

        public void ResetViewPortPos()
        {
            ScrollTo(0);
            AnimIn();
        }

        #endregion

        public void ChangeShowType(LineupCardAdapterShowType showType)
        {
            if (!IsInitialized) return;
            _showType = showType;
            DoScale(0, 0.15f).OnComplete(() =>
            {
                Refresh();
                DoScale(1, 0.15f);
            });
        }

        private void ChangeSelectModel(bool flag)
        {
            if (!IsInitialized) return;

            _selectModel = flag;
            Refresh();
        }

        public void CancelSelectModel()
        {
            if (_selectModel == false) return;
            _selectCard = null;
            _selectedCardIndex = -1;
            ChangeSelectModel(false);

            EventManager.Instance.Dispatch(EventID.OnLineupChangeSelectModel, false);
        }

        public void OnSelectToExchange(LineupCardItem item)
        {
            if (_selectModel) return;

            var selected = GetItemViewsHolderIfVisible(item.RT);
            if (selected == null)
                return;
            _selectedCardIndex = selected.ItemIndex;
            _selectCard = item.Card;
            ChangeSelectModel(true);
        }

        public void OnExchangeCard(LineupCardItem item)
        {
            var exchangeViewsHolder = GetItemViewsHolderIfVisible(item.RT);
            if (exchangeViewsHolder == null)
                return;
            if (_selectedCardIndex == -1)
                return;

            if (_selectCard == null) return;
            if (item.Card == null) return;
            var aCard = _selectCard;
            var bCard = item.Card;

            _formation.ExchangeCardBoard(aCard, bCard);

            var aIndex = _selectedCardIndex;
            var bIndex = exchangeViewsHolder.ItemIndex;

            Data.List[aIndex] = bCard;
            Data.List[bIndex] = aCard;

            CancelSelectModel();
        }

        #region animations
        private void AnimIn()
        {
            for (int i = 0; i < Data.Count; i++)
            {
                var visibleItem = GetItemViewsHolderIfVisible(i);
                if (visibleItem != null)
                {
                    visibleItem.root.SetAnchoredPositionX(1000);
                    visibleItem.root.DOAnchorPosX(360, 0.3f).SetDelay(i * 0.05f);
                    CanvasGroup canvasGrup = visibleItem.root.GetComponent<CanvasGroup>();
                    canvasGrup.alpha = 0;
                    if (i < 4)
                    {
                        canvasGrup.DOFade(1, 0.3f).SetDelay(i * 0.05f).OnStart(() =>
                        {
                            AudioManager.Instance.PlaySound(AudioNames.ENT_FLOP, 0.5f);
                        });
                    }
                    else
                    {
                        canvasGrup.DOFade(1, 0.3f).SetDelay(i * 0.05f);
                    }
                }
            }
        }

        private Tween DoScale(float scale, float duration)
        {
            Sequence seq = DOTween.Sequence();
            seq.AppendCallback(() =>
            {
                for (int i = 0; i < Data.Count; i++)
                {
                    var visibleItem = GetItemViewsHolderIfVisible(i);
                    if (visibleItem != null)
                        visibleItem.root.DOScaleY(scale, duration);
                }
            });
            seq.AppendInterval(duration);
            return seq;
        }
        #endregion
    }

    public class LineupCardItemViewsHolder : BaseItemViewsHolder
    {
        public LineupCardItem ItemComponent;

        private Image backgroundImage;

        public override void CollectViews()
        {
            base.CollectViews();
            ItemComponent = root.GetComponent<LineupCardItem>();
            root.GetComponentAtPath("BackgroundImage", out backgroundImage);
        }

        public void UpdateViews(PlayerCard card, FormationBase formation)
        {
            backgroundImage.gameObject.SetActive(ItemIndex % 2 == 1);
            ItemComponent.SetData(card, formation, ItemIndex);
        }

        public void UpdateShowType(LineupCardAdapterShowType showType)
        {
            ItemComponent.SetShowType(showType);
        }

        public void UpdateSelectModel(bool selectModel, PlayerCard card)
        {
            ItemComponent.SetSelectModel(selectModel, card);
        }
    }
}
