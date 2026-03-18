using System.Collections.Generic;
using UnityEngine;
using Com.TheFallenGames.OSA.CustomAdapters.GridView;
using Com.TheFallenGames.OSA.DataHelpers;
using System;
using Utils;
using BigBang.Animation;
using Babu;
using Utils.GameItem;
using GameConfig;
using GameConfig.Config;

namespace BigBang.UI
{
    public class ChooseItemGridAdapter : GridAdapter<GridParams, ChooseItemViewsHolder>
    {
        public static SimpleDataHelper<ChooseItemGridModel> Data { get; private set; }
        public static Dictionary<int, (int, int)> GridToGoodID = new Dictionary<int, (int, int)>();
        public event Action<ChooseItemGridModel> OnSelect;

        private int selectedIndex = 0;
        private bool multipleChoose = false;
        public static bool LongPress = false;

        protected override void Awake()
        {
            Data = new SimpleDataHelper<ChooseItemGridModel>(this);
            base.Awake();
        }
#if UNITY_WEBGL
        protected override bool IsRecyclable(CellGroupViewsHolder<ChooseItemViewsHolder> potentiallyRecyclable, int indexOfItemThatWillBecomeVisible, double sizeOfItemThatWillBecomeVisible)
        {
            return potentiallyRecyclable.ItemIndex == indexOfItemThatWillBecomeVisible;
        }
#endif
        protected override void UpdateCellViewsHolder(ChooseItemViewsHolder newOrRecycled)
        {
            var model = Data[newOrRecycled.ItemIndex];
            if (selectedIndex == newOrRecycled.ItemIndex)
            {
                newOrRecycled.ShowSelectBorder();
            }
            else
            {
                newOrRecycled.HidSelectBorder();
            }
            newOrRecycled.UpdateViews(model, OnSelectItem, GetSelection);
        }

        public List<ChooseItemGridModel> chooseItemGridModel = new();
        public void SetData(List<BoxConfig> boxList, int selectedIndex)
        {
            var itemList = boxList.ConvertAll<GameItem>(t => GameItemUtils.CreateGameItem((GameItemType)t.RewardType, t.RewardId, t.RewardNum));

            if (!IsInitialized) Init();
            GameItem selectedItem = null;
            int index = 0;
            for (int i = 0; i < itemList.Count; i++)
            { 
                if (i == selectedIndex)
                {
                    selectedItem = itemList[i];
                    index = i;
                    break;
                }
            }
            selectedIndex = index;

            chooseItemGridModel = new List<ChooseItemGridModel>();
            foreach (var item in boxList)
            {
                int sumCount = item.RewardNum;
                var gameItem = GameItemUtils.CreateGameItem((GameItemType)item.RewardType, item.RewardId, item.RewardNum);
                GridToGoodID[item.Id] = (item.Id, sumCount);
                chooseItemGridModel.Add(new ChooseItemGridModel() { GridID = item.Id, Data = gameItem, Count = sumCount });
            }
            Data.ResetItems(chooseItemGridModel);
            Refresh();
        }

        public void SetData(List<GameItem> itemList, int setSelectedConfId)
        {
            if (!IsInitialized) Init();
            GameItem selectedItem = null;
            int index = 0;
            for (int i = 0; i < itemList.Count; i++)
            {
                if (itemList[i].Id == setSelectedConfId)
                {
                    selectedItem = itemList[i];
                    index = i;
                    break;
                }
            }
            selectedIndex = index;

            var list = new List<ChooseItemGridModel>();
            foreach (var item in itemList)
            {
                int sumCount = item.Count;
                GridToGoodID[item.Id] = (item.Id, sumCount);
                list.Add(new ChooseItemGridModel() { GridID = item.Id, Data = item, Count = sumCount });
            }
            Data.ResetItems(list);
            Refresh();
        }

        public void SetEmptyData()
        {
            if (!IsInitialized) Init();
            SetData(new List<GameItem>(), -1);
        }

        // 获得选择的Item下标
        public void GetSelection(int viewHolderIndex)
        {
            selectedIndex = viewHolderIndex;
            SetSelection(selectedIndex);
        }

        // 元素选中事件
        public void OnSelectItem(ChooseItemGridModel selectItem)
        {
            OnSelect?.Invoke(selectItem);
        }

        // 设置选中项并显示选中边框
        public void SetSelection(int index)
        {
            for (int i = 0; i < VisibleItemsCount; i++)
            {
                var groupVH = GetItemViewsHolder(i);
                for (int j = 0; j < groupVH.NumActiveCells; j++)
                {
                    var item = groupVH.ContainingCellViewsHolders[j];
                    if (item.ItemIndex == index)
                    {
                        item.ShowSelectBorder();
                    }
                    else
                    {
                        item.HidSelectBorder();
                    }
                }
            }
        }
    }

    public class ChooseItemGridModel
    {
        // 格子ID
        public int GridID;
        public GameItem Data;
        // 物品数量
        public int Count;
    }

    public class ChooseItemViewsHolder : CellViewsHolder
    {
        private InventoryItemInSelect item;
        private Action<ChooseItemGridModel> selectAction;
        private Action<int> sendSelection;
        private ChooseItemGridModel model;
        private LongPress longPress;
        public override void CollectViews()
        {
            base.CollectViews();
            longPress = root.GetComponentInChildren<LongPress>();
            item = root.GetComponent<InventoryItemInSelect>();
            longPress.Press += OnPress;
            longPress.Release += OnRelease;
            InventoryRecyclePad.OnSelectCountChanged += RefreshUI;
        }

        // 显示选中边框
        public void ShowSelectBorder()
        {
            item.ShowSelectBorder();
        }

        // 隐藏选中遍历
        public void HidSelectBorder()
        {
            item.HidSelectBorder();
        }

        // 长按触发时间
        float pressTime = 0;
        // 时间间隔
        float gapTime = 0;

        private void OnPress()
        {
            if (!ChooseItemGridAdapter.LongPress) return;
            pressTime += Time.deltaTime;
            if (pressTime < 1.5f)
            {
                gapTime += Time.deltaTime;
                if (gapTime > 0.5f)
                {
                    gapTime = 0;
                }
            }
            if (pressTime > 1.5f)
            {
                gapTime += Time.deltaTime;
                if (gapTime > 0.01)
                {
                    gapTime = 0;
                }
            }
        }

        private void OnRelease()
        {
            AudioManager.Instance.PlaySound(AudioNames.BTN_SELECT);
            sendSelection(ItemIndex);
            selectAction?.Invoke(model);
            pressTime = 0;
        }

        protected override RectTransform GetViews()
        {
            return root.Find("Views").transform as RectTransform;
        }

        public void UpdateViews(ChooseItemGridModel model, Action<ChooseItemGridModel> selectionAction, Action<int> sendSelection)
        {
            this.model = model;
            this.sendSelection = sendSelection;
            selectAction = selectionAction;
            RefreshUI();
        }

        public void RefreshUI()
        {
            if (root == null) return;
            if (model == null || model.Data == null) return;
            if (item == null) item = root.GetComponent<InventoryItemInSelect>();
            item.SetData(model.Data, false, true);
        }
    }
}
