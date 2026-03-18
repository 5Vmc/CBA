using System.Collections.Generic;
using Babu;
using BigBang.Animation;
using Com.TheFallenGames.OSA.CustomAdapters.GridView;
using Com.TheFallenGames.OSA.DataHelpers;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Utils.GameItem;

namespace BigBang.UI
{
    public class GameItemGridAdapter : GridAdapter<GameitemInfoParams, GameitemGridViewsHolder>
    {
        public SimpleDataHelper<GameItem> Data { get; private set; }

        private List<GameItem> _gameItemList;

        protected override void Start()
        {
            Data = new SimpleDataHelper<GameItem>(this);
            base.Start();
            SetData(_gameItemList);
        }
#if UNITY_WEBGL
        protected override bool IsRecyclable(CellGroupViewsHolder<GameitemGridViewsHolder> potentiallyRecyclable, int indexOfItemThatWillBecomeVisible, double sizeOfItemThatWillBecomeVisible)
        {
            return potentiallyRecyclable.ItemIndex == indexOfItemThatWillBecomeVisible;
        }
#endif
        public void SetData(List<GameItem> gameItemList, bool playAnim = true)
        {
            _gameItemList = gameItemList;
            if (!IsInitialized) return;
            if (gameItemList is null)
            {
                gameItemList = new List<GameItem>();
            }
            Data.ResetItems(gameItemList);
            PlayAnim();
        }

        public void PlayAnim()
        {
            for (int i = 0; i < VisibleItemsCount; i++)
            {
                var groupVH = GetItemViewsHolder(i);

                for (int j = 0; j < groupVH.NumActiveCells; j++)
                {
                    var item = groupVH.ContainingCellViewsHolders[j];
                    item?.PlayEnter((i + 1) * 0.1f);
                }
            }
        }

        protected override void UpdateCellViewsHolder(GameitemGridViewsHolder newOrRecycled)
        {
            var model = Data[newOrRecycled.ItemIndex];
            newOrRecycled.UpdateViews(model);
        }
    }

    [System.Serializable]
    public class GameitemInfoParams : GridParams { }

    public class GameitemGridViewsHolder : CellViewsHolder
    {
        private GameItem gameItem;
        private InventoryItem inventoryItem;

        public override void CollectViews()
        {
            base.CollectViews();
            inventoryItem = root.GetComponent<InventoryItem>();
        }

        public void UpdateViews(GameItem gameItem)
        {
            this.gameItem = gameItem;
            inventoryItem.SetData(gameItem);
        }

        public void PlayEnter(float delay)
        {
            //_anim.PlayEnter(delay);
        }
    }
}