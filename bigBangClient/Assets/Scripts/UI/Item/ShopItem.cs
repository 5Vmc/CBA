using Babu;
using Coffee.UIEffects;
using DG.Tweening;
using GameConfig;
using GameConfig.Config;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Utils.GameItem;

namespace BigBang.UI
{
    public class ShopItemData
    {
        public int ShopItemId;
        public GameItem CostItem;
        public int boughtCountDaily;
        public int boughtCountTotal;
        public int boughtCountWeek;

        public GameItemShopConfig cfg;
        public List<GoodsConfig> goodCfgList;
        public List<GameItem> gameItemList;

        /// <summary>
        /// 是否可以购买
        /// </summary>
        public bool Status;
        /// <summary>
        /// 球探商店的数据，只限制每日购买数量
        /// </summary>
        /// <param name="_cfg"></param>
        /// <param name="boughtCount">已经购买的数量</param>
        public ShopItemData(GameItemShopConfig _cfg)
        {
            cfg = _cfg;
            gameItemList = GameItemUtils.CreateGameItems(cfg.Item).ToList();
            goodCfgList = new List<GoodsConfig>();
            foreach (GameItem item in gameItemList)
            {
                goodCfgList.Add(Configs.Goods.GetConfig(item.Id));
            }

            this.boughtCountDaily = Player.ShopManager.BuyCount.FirstOrDefault(item => item.Key == _cfg.Id).Value;
            this.boughtCountWeek = Player.ShopManager.WeekCount.FirstOrDefault(item => item.Key == _cfg.Id).Value;
            this.boughtCountTotal = Player.ShopManager.SumCount.FirstOrDefault(item => item.Key == _cfg.Id).Value;


            CostItem = GameItemUtils.CreateGameItem(cfg.Cost);
            if (cfg.Limit > -1)
            {
                Status = this.boughtCountTotal < cfg.Limit;
            }

            if (cfg.DayLimit > -1)
            {
                Status |= this.boughtCountDaily < cfg.DayLimit;
            }

            if (cfg.WeekLimit > -1)
            {
                Status |= this.boughtCountWeek < cfg.WeekLimit;
            }

        }
    }

    public class ShopItem : MonoBehaviour
    {
        [SerializeField] private InventoryItem inv_Item;
        [SerializeField] private TMP_Text txtItemName;
        [SerializeField] private Text descTextLabel = null;
        [SerializeField] private TMP_Text txtItemLimit;
        [SerializeField] private Button costBtn;
        [SerializeField] private Image priceIcon;
        [SerializeField] private TMP_Text price;


        public ShopItemData data;

        private void OnEnable()
        {
            costBtn.onClick.AddListener(OnCost);
        }

        private void OnDisable()
        {
            costBtn.onClick.RemoveListener(OnCost);
        }

        public async void SetData(ShopItemData _data)
        {
            //这里只有1个道具
            this.data = _data;

            inv_Item.SetData(_data.gameItemList[0]);
            txtItemName.text = _data.gameItemList[0].GetName();

            txtItemName.color = CBAColorUtil.Instance.GetColor(_data.gameItemList[0].GetQuality());
            descTextLabel.text = _data.gameItemList[0].GetDescription();


            costBtn.interactable = _data.Status;
            costBtn.image.sprite = _data.Status ? await SpriteProxy.YellowBtnEnable : await SpriteProxy.YellowBtnDisable;

            if (_data.CostItem.Type == GameItemType.Resource && _data.CostItem.Id == ResourceId.Diamond)
            {
                priceIcon.sprite = await SpriteProxy.GetHomeIcon("diamondOnBtn");
            }
            else
            {
                priceIcon.sprite = await _data.CostItem.GetIcon();
            }

            price.text = _data.CostItem.Count.ToString();

            if (_data.cfg.Limit != -1)
            {
                txtItemLimit.text = "限购: " + _data.boughtCountTotal + "/" + _data.cfg.Limit;
            }
            else if (_data.cfg.SeasonLimit != -1)
            {
                txtItemLimit.text = "赛季限购: " + "undo/" + _data.cfg.SeasonLimit;
            }
            else if (_data.cfg.WeekLimit != -1)
            {
                txtItemLimit.text = "每周限购: " + "undo/" + _data.cfg.WeekLimit;
            }
            else if (_data.cfg.DayLimit != -1)
            {
                txtItemLimit.text = "每日限购: " + _data.boughtCountDaily + "/" + _data.cfg.DayLimit;
            }
            else
            {
                txtItemLimit.text = "";
            }
        }

        private void OnCost()
        {
            Player.ShopManager.ExChangeItem(data.cfg);
        }
    }
}