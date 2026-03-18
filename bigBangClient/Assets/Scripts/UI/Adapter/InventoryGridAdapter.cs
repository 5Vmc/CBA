using System.Collections.Generic;
using UnityEngine;
using Com.TheFallenGames.OSA.CustomAdapters.GridView;
using Com.TheFallenGames.OSA.DataHelpers;
using System;
using Utils;
using BigBang.Animation;
using Babu;

namespace BigBang.UI
{
    public class InventoryGridAdapter : GridAdapter<GridParams, PackageGridItemViewsHolder>
    {
        public static SimpleDataHelper<InventoryGridModel> Data { get; private set; }
        public static Dictionary<string, (int, int)> GridToGoodID = new Dictionary<string, (int, int)>();
        public event Action<GameObject> OnSelect;

        private int selectedIndex = 0;
        private bool multipleChoose = false;
        public static bool LongPress = false;

        protected override void Awake()
        {
            Data = new SimpleDataHelper<InventoryGridModel>(this);
            base.Awake();
        }
#if UNITY_WEBGL
        protected override bool IsRecyclable(CellGroupViewsHolder<PackageGridItemViewsHolder> potentiallyRecyclable, int indexOfItemThatWillBecomeVisible, double sizeOfItemThatWillBecomeVisible)
        {
            return potentiallyRecyclable.ItemIndex == indexOfItemThatWillBecomeVisible;
        }
#endif
        public void PlayAnim()
        {
            // 播放动画是停用触摸
            TouchManager.Instance.DisableTouch();
            Babu.DelayTaskService.Instance.Run(this.gameObject, 1f, () =>
            {
                TouchManager.Instance.EnableTouch();
            });
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

        protected override void UpdateCellViewsHolder(PackageGridItemViewsHolder newOrRecycled)
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

        public void SetData(List<GoodsData> itemList, int setSelectedConfId)
        {
            if (!IsInitialized) Init();
            GoodsData selectedGoods = null;
            int index = 0;
            for (int i = 0; i < itemList.Count; i++)
            {
                if (itemList[i].Config.Id == setSelectedConfId)
                {
                    selectedGoods = itemList[i];
                    index = i;
                    break;
                }
            }
            selectedIndex = index;

            var list = new List<InventoryGridModel>();
            foreach (var item in itemList)
            {
                int sumCount = item.Count;
                int pileCount = 0;
                string gridID;
                while (sumCount > item.Config.StackNum)
                {
                    sumCount -= item.Config.StackNum;
                    gridID = item.Config.Id + $" {pileCount}";
                    GridToGoodID[gridID] = (item.Id, item.Config.StackNum);
                    list.Add(new InventoryGridModel() { GridID = gridID, Data = item, Count = item.Config.StackNum });
                    pileCount++;
                }
                gridID = item.Config.Id + $" {pileCount}";
                GridToGoodID[gridID] = (item.Id, sumCount);
                list.Add(new InventoryGridModel() { GridID = gridID, Data = item, Count = sumCount });
            }
            Data.ResetItems(list);
            Refresh();
        }

        public void SetEmptyData()
        {
            if (!IsInitialized) Init();
            SetData(new List<GoodsData>(), -1);
        }

        // 获得选择的Item下标
        public void GetSelection(int viewHolderIndex)
        {
            selectedIndex = viewHolderIndex;
            SetSelection();
        }

        // 元素选中事件
        public void OnSelectItem(GameObject selectItem)
        {
            OnSelect?.Invoke(selectItem);
            if (multipleChoose)
            {
                for (int i = 0; i < VisibleItemsCount; i++)
                {
                    var groupVH = GetItemViewsHolder(i);
                    for (int j = 0; j < groupVH.NumActiveCells; j++)
                    {
                        var item = groupVH.ContainingCellViewsHolders[j];
                        if (item.ItemIndex == selectedIndex)
                        {
                            item.AddCount();
                            return;
                        }
                    }
                }
            }
        }

        // 设置选中项并显示选中边框
        public void SetSelection()
        {
            for (int i = 0; i < VisibleItemsCount; i++)
            {
                var groupVH = GetItemViewsHolder(i);
                for (int j = 0; j < groupVH.NumActiveCells; j++)
                {
                    var item = groupVH.ContainingCellViewsHolders[j];
                    if (item.ItemIndex == selectedIndex)
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

        // 启用多选
        public void EnableMultipleChoose()
        {
            LongPress = true;
            multipleChoose = true;
            InventoryRecyclePad.ClearSelect();
        }

        // 关闭多选
        public void DisableMultipleChoose()
        {
            LongPress = false;
            multipleChoose = false;
            InventoryRecyclePad.ClearSelect();
        }
    }

    public class InventoryGridModel
    {
        // 格子ID
        public string GridID;
        public GoodsData Data;
        // 物品数量
        public int Count;
    }

    public class PackageGridItemViewsHolder : CellViewsHolder
    {
        private InventoryItem item;
        private Action<GameObject> selectAction;
        private Action<int> sendSelection;
        private InventoryGridModel model;
        private LongPress longPress;
        private InventoryItemAnim _anim;
        public override void CollectViews()
        {
            base.CollectViews();
            longPress = root.GetComponentInChildren<LongPress>();
            item = root.GetComponent<InventoryItem>();
            _anim = root.GetComponent<InventoryItemAnim>();
            item.OnSub += SubCount;
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

        // 显示减号按钮
        public void ShowSubButton()
        {
            item.ShowSubButton();
        }

        // 隐藏减号按钮
        public void HidSubButton()
        {
            item.HidSubButton();
        }

        // 长按触发时间
        float pressTime = 0;
        // 时间间隔
        float gapTime = 0;

        private void OnPress()
        {
            if (!InventoryGridAdapter.LongPress) return;
            pressTime += Time.deltaTime;
            if (pressTime < 1.5f)
            {
                gapTime += Time.deltaTime;
                if (gapTime > 0.5f)
                {
                    AddCount();
                    gapTime = 0;
                }
            }
            if (pressTime > 1.5f)
            {
                gapTime += Time.deltaTime;
                if (gapTime > 0.01)
                {
                    gapTime = 0;
                    AddCount();
                }
            }
        }

        private void OnRelease()
        {
            AudioManager.Instance.PlaySound(AudioNames.BTN_SELECT);
            sendSelection(ItemIndex);
            selectAction?.Invoke(root.gameObject);
            pressTime = 0;
            item.HidNewTag();
        }

        private void SubCount()
        {
            InventoryRecyclePad.SubSelection(model.GridID);
        }

        public void AddCount()
        {
            InventoryRecyclePad.AddSelection(model.GridID);
        }

        protected override RectTransform GetViews()
        {
            return root.Find("Views").transform as RectTransform;
        }

        public void UpdateViews(InventoryGridModel model, Action<GameObject> selectionAction, Action<int> sendSelection)
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
            if (item == null) item = root.GetComponent<InventoryItem>();
            item.dotnodePath = PanelNodePath.Home_Bag + "/Storage";
            item.SetData(model.Data, model.Count, false, true);
            if (InventoryRecyclePad.SelectGoods.ContainsKey(model.GridID))
            {
                // 设置选中数量
                item.SetSubText(InventoryRecyclePad.SelectGoods[model.GridID], model.Count);
                ShowSubButton();
            }
            else
            {
                HidSubButton();
            }
        }

        public void PlayEnter(float delay)
        {
            _anim.PlayEnter(delay);
        }
    }
}
