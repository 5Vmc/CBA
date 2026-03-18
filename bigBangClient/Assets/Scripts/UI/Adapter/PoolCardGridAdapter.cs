using System;
using System.Collections.Generic;
using Com.TheFallenGames.OSA.CustomAdapters.GridView;
using Com.TheFallenGames.OSA.DataHelpers;
using GameConfig.Config;
using UnityEngine.UI;

namespace BigBang.UI
{
    public class PoolCardGridAdapter : GridAdapter<PoolCardParams, PoolCardGridViewsHolder>
    {
        public SimpleDataHelper<CardModelConfig> Data { get; private set; }

        private List<CardModelConfig> _cardList;
        private int _poolId;

        private int _selectCardId;

        private Action<CardModelConfig> _selectAction;

        protected override void Awake()
        {
            base.Awake();
            Data = new SimpleDataHelper<CardModelConfig>(this);
        }
#if UNITY_WEBGL
        protected override bool IsRecyclable(CellGroupViewsHolder<PoolCardGridViewsHolder> potentiallyRecyclable, int indexOfItemThatWillBecomeVisible, double sizeOfItemThatWillBecomeVisible)
        {
            return potentiallyRecyclable.ItemIndex == indexOfItemThatWillBecomeVisible;
        }
#endif
        public void SetData(List<CardModelConfig> cardList, int poolId)
        {
            if (!IsInitialized)
            {
                Init();
            }
            _cardList = cardList;
            _poolId = poolId;
            cardList ??= new List<CardModelConfig>();
            Data.ResetItems(cardList);
        }

        protected override void UpdateCellViewsHolder(PoolCardGridViewsHolder newOrRecycled)
        {
            var model = Data[newOrRecycled.ItemIndex];
            newOrRecycled.UpdateViews(model, _poolId, OnSelectCard);
        }

        public void SelectActionRegister(Action<CardModelConfig> selectAction)
        {
            _selectAction = selectAction;
        }

        private void OnSelectCard(CardModelConfig selectCard)
        {
            _selectCardId = selectCard.Id;

            Player.CardManager.RecruitController.DoAddAppoint(_poolId, _selectCardId, SelectCardSuccess);
        }

        private void SelectCardSuccess()
        {
            Refresh();
        }
    }

    [System.Serializable]
    public class PoolCardParams : GridParams { }

    public class PoolCardGridViewsHolder : CellViewsHolder
    {
        private CardModelConfig _config;
        private CardSelectIcon cardItem;
        private BabuButton _btn;

        private Action<CardModelConfig> _selectAction;

        public override void CollectViews()
        {
            base.CollectViews();
            cardItem = root.GetComponent<CardSelectIcon>();
            _btn = root.GetComponent<BabuButton>();

            _btn.OnClick += OnClickSelect;
        }

        private void OnClickSelect(BabuButton _)
        {
            _selectAction?.Invoke(_config);
        }

        public void UpdateViews(CardModelConfig config, int poolId, Action<CardModelConfig> selectAction)
        {
            _config = config;
            _selectAction = selectAction;
            var isSelect = Player.CardManager.RecruitController.IsAppointSelect(poolId, config.Id);
            cardItem.SetData(_config, isSelect);
        }
    }
}