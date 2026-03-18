using System;
using System.Collections.Generic;
using System.Linq;
using GameConfig;
using Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Utils.GameItem;

namespace BigBang.UI
{
    public class InventoryRecyclePad : MonoBehaviour
    {
        [SerializeField] private InventoryItem euroItem;
        [SerializeField] private List<InventoryItem> crystalItems;
        [SerializeField] private Button recycleBtn;
        [SerializeField] private Button selectAllBtn;
        [SerializeField] private Button clearAllBtn;
        [SerializeField] private TMP_Text selectCountText;
        [SerializeField] private TMP_Dropdown dropdown;
        [SerializeField] private HorizontalAdapter horizontal;

        public event Action<int> OnQuality;
        public event Action OnSelectAllBtnClick;
        public event Action OnClearAllBtnClick;

        // key为ItemID、value为选中数量
        public static Dictionary<string, int> SelectGoods = new Dictionary<string, int>();
        public static event Action OnSelectCountChanged;

        /// <summary>
        /// 准备出售的道具
        /// </summary>
        private List<Utils.GameItem.GameItem> selList = new List<Utils.GameItem.GameItem>();
        /// <summary>
        /// 可以获得的道具
        /// </summary>
        private List<Utils.GameItem.GameItem> refoundList = new List<Utils.GameItem.GameItem>();

        public int SelectGoodsCount { get => SelectGoods.Sum(item => item.Value); }

        private void OnEnable()
        {
            recycleBtn.onClick.AddListener(OnRecycle);
            selectAllBtn.onClick.AddListener(OnSelectAll);
            clearAllBtn.onClick.AddListener(OnClearAll);
            dropdown.onValueChanged.AddListener(OnDropdown);
            OnSelectCountChanged += RefreshUI;
        }

        private void OnDisable()
        {
            recycleBtn.onClick.RemoveListener(OnRecycle);
            selectAllBtn.onClick.RemoveListener(OnSelectAll);
            clearAllBtn.onClick.RemoveListener(OnClearAll);
            dropdown.onValueChanged.RemoveListener(OnDropdown);
            OnSelectCountChanged -= RefreshUI;
        }

        // 增加选中的元素数量
        public static void AddSelection(string gridID)
        {
            if (SelectGoods.ContainsKey(gridID))
            {
                if (SelectGoods[gridID] >= InventoryGridAdapter.GridToGoodID[gridID].Item2) return;
                SelectGoods[gridID] += 1;
            }
            else
            {
                SelectGoods.Add(gridID, 1);
            }
            OnSelectCountChanged?.Invoke();
        }

        // 减小选中的元素数量
        public static void SubSelection(string itemID)
        {
            if (SelectGoods.ContainsKey(itemID))
            {
                if (SelectGoods[itemID] > 1)
                {
                    SelectGoods[itemID] -= 1;
                }
                else
                {
                    SelectGoods.Remove(itemID);
                }
                OnSelectCountChanged?.Invoke();
            }
        }

        // 清空选中元素列表
        public static void ClearSelect()
        {
            SelectGoods.Clear();
            OnSelectCountChanged?.Invoke();
        }

        // 设置选中元素的数量
        public void SetSelectionToValue(string itemID, int count)
        {
            if (SelectGoods.ContainsKey(itemID))
            {
                SelectGoods[itemID] = count;
            }
            else
            {
                SelectGoods.Add(itemID, count);
            }
            OnSelectCountChanged?.Invoke();
        }

        // 回收
        private void OnRecycle()
        {
            // 请选择回收材料
            if (SelectGoods.Count <= 0)
            {
                Tips.PopError(ErrorID.ChooseMaterial);
                return;
            }
            var recycleGoods = GetSelectionList().ToList();
            selList.Clear();
            recycleGoods.ForEach((p) => { selList.Add(GameItemUtils.CreateGameItem(GameItemType.Goods, p.Id, p.Count)); });
            if (recycleGoods.Exists(item => Configs.Goods.GetConfig(item.Id).Quality >= QualityType.Orange))
            {
                // 打开确认框
                UIController.Instance.OpenWindow<ConfirmationBoxUI>(new ConfirmationBoxUIProperties(Lang.Get(LangID.RecycleConfirmText), () =>
                {
                    NetworkManager.Instance.DelGoodsList(recycleGoods, OnRecycleSuddeed);
                }));
            }
            else
            {
                NetworkManager.Instance.DelGoodsList(recycleGoods, OnRecycleSuddeed);
            }
        }

        // 回收成功回调
        public void OnRecycleSuddeed(DelGoodsResponse response)
        {
            //这句不能在 clearselect后面，会被清理掉refoundList;
            UIController.Instance.OpenWindow<InventoryObtainedUI>(new InventoryObtainedUIProperties(refoundList));
            ClearSelect();
            Debug.Log("回收成功");
            Babu.EventManager.Instance.Dispatch(EventID.OnRefreshInventoryAdapter);
        }

        // 一键全选
        private void OnSelectAll()
        {
            foreach (var item in InventoryGridAdapter.Data)
            {
                SetSelectionToValue(item.GridID, item.Count);
            }
        }

        // 一键取消
        private void OnClearAll()
        {
            ClearSelect();
        }

        // 获得选中元素列表
        public IEnumerable<Goods> GetSelectionList()
        {
            var dic = new Dictionary<int, int>();
            foreach (var item in SelectGoods)
            {
                var goodsID = InventoryGridAdapter.GridToGoodID[item.Key].Item1;
                if (!dic.ContainsKey(goodsID))
                {
                    dic.Add(goodsID, item.Value);
                }
                else
                {
                    dic[goodsID] += item.Value;
                }
            }
            foreach (var item in dic)
            {
                yield return new Goods() { Id = item.Key, Count = item.Value };
            }
            yield break;
        }

        private void RefreshUI()
        {
            var recycleGoods = GetSelectionList();
            int price = 0;
            int[] qualityCount = { 0, 0, 0, 0, 0 };
            refoundList.Clear();
            foreach (var item in recycleGoods)
            {
                var cfg = Configs.Goods.GetConfig(item.Id);
                if (cfg.Type == (int)GoodsType.Pieces)
                {
                    // 当获得星晶时，获得数量 = 0.1 * 道具价值 / 道具品质档次 (向下取整)
                    qualityCount[cfg.Quality - 1] += ((int)(0.1f * cfg.Price / cfg.Quality)) * item.Count;
                }
                else
                {
                    // 当获得欧元时，获得数量 = 道具价值。
                    price += cfg.Price * item.Count;
                }
            }
            // 设置已选文本
            selectCountText.text = Lang.Get(LangID.HasBennSelected).Replace("{value}", SelectGoodsCount.ToString());
            // 设置一键全选按钮的启用状态
            selectAllBtn.gameObject.SetActive(SelectGoodsCount == 0);
            // 设置一键清除按钮的启用状态
            clearAllBtn.gameObject.SetActive(SelectGoodsCount > 0);
            // 欧元
            if (price > 0)
            {
                euroItem.gameObject.SetActive(true);
                var gameItemMoney = GameItemUtils.CreateGameItem(GameItemType.Resource, ResourceId.Money, price);
                euroItem.SetData(gameItemMoney);
                refoundList.Add(gameItemMoney);
            }
            else {
                euroItem.gameObject.SetActive(false);
            }
            
            
            for (int i = 0; i < crystalItems.Count; i++)
            {
                if (qualityCount[i] > 0) {
                    crystalItems[i].gameObject.SetActive(true);
                    var gameItemCrystal = GameItemUtils.CreateGameItem(GameItemType.Goods, GoodsId.CrystalGoodsId[i], qualityCount[i]);
                    crystalItems[i].SetData(gameItemCrystal);
                    refoundList.Add(gameItemCrystal);
                } 
                else {
                    crystalItems[i].gameObject.SetActive(false);
                }
                
            }
            horizontal.Calculate();
        }

        // 清空回收栏
        public void ClearPad()
        {
            euroItem.gameObject.SetActive(false);
            for (int i = 0; i < crystalItems.Count; i++)
            {
                crystalItems[i].gameObject.SetActive(false);
            }
            dropdown.value = 0;
        }

        private void OnDropdown(int index)
        {
            OnQuality(index);
        }
    }
}
